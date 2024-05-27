using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static CSharpToTypeSpec.TypeNaming;

// ReSharper disable IdentifierTypo

namespace CSharpToTypeSpec;

public interface IJsonAttributor
{
    // Return a string if the member has an associated attribute with a name for the json field, otherwise null.
    string? JsonPropertyName(MemberInfo member);
    
    // Returns true iff the member has been marked as not to be converted to Json.
    bool IsJsonIgnored(MemberInfo member);
    bool IsNullableProperty(MemberInfo member);
}
public class TsTranslator(IJsonAttributor jsonAttributor, IReadOnlyDictionary<string, string>? typeOverrides = null)
{
    private readonly IReadOnlyDictionary<string, string> typeOverrides = typeOverrides ?? ImmutableDictionary<string, string>.Empty;
    private readonly List<TsType> tsTypes = [];

    public IEnumerable<TsType> TsTypes => tsTypes;

    public void AddType(Type aType)
    {
        if (IsAlreadyRegistered(aType))
            return;

        var parentType = aType.TryParentType();
        if (parentType != null) {
            AddType(parentType);
        }

        if (aType.IsEnum)
            AddEnum(aType);
        else
            AddModel(aType, parentType);
    }

    private void AddEnum(Type aType) => tsTypes.Add(aType.ToTsEnum());

    private void AddModel(Type aType, Type? parentType)
    {
        var model = new TsModel(aType, TsModelFor(parentType));
        tsTypes.Add(model);

        foreach (var property in PropertyModelsFrom(aType))
        {
            AddTsTypeFor(property.Type);
            model.AddProperty(DescriptionOf(property));
        }
    }

    private TsModel? TsModelFor(Type? parentType) =>
        parentType == null 
            ? null 
            : tsTypes.Find(tsType => tsType.HasType(parentType)) as TsModel;

    private void AddTsTypeFor(Type aType)
    {
        if (TryArrayGenericType(aType, out var genericType))
        {
            AddType(genericType);
        }
        else if (aType.ShouldBeAdded())
        {
            AddType(aType);
        }
    }

    private bool IsAlreadyRegistered(Type aType) => tsTypes.Any(tt => tt.HasType(aType));

    private static bool TryArrayGenericType(Type aType, [MaybeNullWhen(false)] out Type genericType)
    {
        genericType = null;
        if (!aType.IsAnArray()) return false;
        
        var arrayType = aType.ArrayType();
        if (arrayType?.IsNotASystemType() ?? false)
        {
            genericType = arrayType;
            return true;
        }
        return false;
    }
    
    private TsModelDescription DescriptionOf(PropertyModel property) =>
        typeOverrides.TryGetValue(property.Name, out var overrideType) 
            ? new TsModelDescription(property.Name, overrideType) 
            : property.ToDescription();

    private IEnumerable<PropertyModel> PropertyModelsFrom(IReflect aType) =>
        aType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Select(ToPropertyModel)
            .Where(IsNotIgnored)
            .Select(ToNullableChecked);


    private bool IsNotIgnored(PropertyModel p) => ! jsonAttributor.IsJsonIgnored(p.Property);
    private PropertyModel ToNullableChecked(PropertyModel p)
    {
        var nullableChecked = p.ToNullableChecked();
        return nullableChecked.IsNullable
            ? nullableChecked
            : p.ToNullablePropertyModel(jsonAttributor.IsNullableProperty(p.Property));
    }

    private PropertyModel ToPropertyModel(PropertyInfo property) => new(property, JsonName(property), property.PropertyType);

    private string JsonName(MemberInfo member) => (jsonAttributor.JsonPropertyName(member) ?? member.Name).ToLowerCamelCase();

    public static void WriteAsTypeSpec(Type rootType, string nameSpace, IJsonAttributor jsonAttributor, TextWriter writer, 
        IReadOnlyDictionary<string, string>? typeOverrides = null)
    {
        writer.WriteLine("// This file was generated at " + DateTime.Now);
        writer.WriteLine();
        
        var translator = new TsTranslator(jsonAttributor, typeOverrides);
        translator.AddType(rootType);

        writer.WriteLine("namespace " + nameSpace);
        writer.WriteLine();
        
        foreach (var tsType in translator.TsTypes)
        {
            writer.WriteLine(tsType);
        }
        writer.WriteLine();
    }
}

internal record PropertyModel(PropertyInfo Property, string Name, Type Type, bool IsNullable = false)
{
    internal PropertyModel ToNullableChecked() => Type.IsValueType ? NullCheckedValueType() : NullCheckedReferenceType();
    internal PropertyModel ToNullablePropertyModel(bool isNullable) => isNullable ? this with { Name = NullableName, IsNullable = true} : this;
    
    private PropertyModel NullCheckedValueType()
    {
        var underlying = Nullable.GetUnderlyingType(Type);
        return underlying != null ? this with { Name = NullableName, Type = underlying, IsNullable = true} : this; 
    }

    private PropertyModel NullCheckedReferenceType() => ToNullablePropertyModel(IsNullableReferenceType);

    private string NullableName => Name + "?";

    private bool IsNullableReferenceType => new NullabilityInfoContext().Create(Property).ReadState is NullabilityState.Nullable;


    public TsModelDescription ToDescription()
    {
        if (TryBuiltIn(Type, out var typeName))
            return new TsModelDescription(Name, typeName);

        if (Type.IsADictionary())
            return new TsModelDescription(Name, UnknownTypeName);

        if (Type.IsAnArray())
            return new TsModelDescription(Name, ArrayTypeName(this));

        return new TsModelDescription(Name, Type.Name);
    }
}

internal static class TypeNaming
{
    public const string UnknownTypeName = "unknown";
    private const string ArraySuffix = "[]";
    
    private static readonly IReadOnlyDictionary<Type, string> BuiltInTypeNames = new Dictionary<Type, string>()
    {
        { typeof(bool), "boolean" },
        { typeof(int), "integer" },
        { typeof(uint), "uint32" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(string), "string" },
        { typeof(DateTime), "utcDateTime" },
        { typeof(object), UnknownTypeName }
    };

    internal static bool TryBuiltIn(Type aType, [MaybeNullWhen(false)] out string typeName) => 
        BuiltInTypeNames.TryGetValue(aType, out typeName);

    internal static string ArrayTypeName(PropertyModel property) => MaybeTypeName(property.Type.ArrayType()) + ArraySuffix;

    private static string MaybeTypeName(Type? elementType) => 
        elementType == null ? UnknownTypeName : BuiltInTypeName(elementType, elementType.Name);

    private static string BuiltInTypeName(Type aType, string aDefault) =>
        BuiltInTypeNames.GetValueOrDefault(aType, aDefault);
}

internal static class TypeExtensions
{
    public static Type? ArrayType(this Type type) => type.IsArray ? type.GetElementType() : type.GetGenericArguments().ElementAtOrDefault(0);

    public static Type? TryParentType(this Type aType)
    {
        var parentType = aType.BaseType;
        return parentType.ShouldAddParentType() ? parentType : null;
    }

    private static bool ShouldAddParentType(this Type? baseType) =>
        baseType is { IsClass: true } && baseType != typeof(ValueType) && baseType.IsNotASystemType();

    public static bool IsADictionary(this Type type) => type.IsAssignableTo(typeof(IDictionary)) || type.IsAGenericDictionary();
    public static bool IsAnArray(this Type type) => type.IsAssignableTo(typeof(IList));
    public static bool ShouldBeAdded(this Type type) => type.IsNotASystemType() && (type.IsClass || type.GetProperties().Length > 0 || type.IsEnum);
    public static TsEnum ToTsEnum(this Type aType) => new(aType, Enum.GetNames(aType));

    private static bool IsAGenericDictionary(this Type type) => type.MatchesGenericDictionary() || type.InheritsFromAGenericDictionary();
    private static bool InheritsFromAGenericDictionary(this Type type) => type.FindInterfaces((t, _) => t.MatchesGenericDictionary(), null).Length > 0;
    private static bool MatchesGenericDictionary(this Type t) => t.GUID == typeof(IDictionary<,>).GUID || t.GUID == typeof(IReadOnlyDictionary<,>).GUID;
    internal static bool IsNotASystemType(this Type type) => (!type.Namespace?.Contains("System") ?? true);
}

internal static class TsExtensions
{
    internal static string ToLowerCamelCase(this string str) => char.ToLowerInvariant(str[0]) + str[1..];
}
