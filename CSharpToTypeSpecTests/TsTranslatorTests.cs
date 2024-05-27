using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CSharpToTypeSpec;
using static CSharpToTypeSpec.TsSystemJsonAttributor;

// ReSharper disable UnusedMember.Local

namespace CSharpToTypeSpecTests;

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class TsTranslatorTests
{
    internal static readonly TsEnum EnumerationModel = new(typeof(Enumeration), ["First"]);
    private static readonly Type EmptyClassType = typeof(EmptyClass);
    private static readonly Action<TsType> IsEmptyTypeModel = IsTsType(new TsModel(EmptyClassType));
    private static readonly TsModel WithStructModel = new TsModel(typeof(WithStruct)) .WithProperty("aStruct", "StructRecord");
    private static readonly TsModel StructModel = new TsModel(typeof(StructRecord))
        .WithProperty("first", "string").WithProperty("second", "string");

    private readonly TsTranslator translator = new(JsonAttributor);

    [Fact]
    public void Empty_class_has_no_properties()
    {
        translator.AddType(EmptyClassType);
        Assert.Collection(translator.TsTypes, IsEmptyTypeModel);
    }

    [Fact]
    public void Translate_property_names_to_lowerCamelCase()
    {
        var theType = typeof(WithCamelCase);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType)
                .WithProperty("upperCase", "boolean")
                .WithProperty("lowerCase", "boolean"))
            );
    }

    [Fact]
    public void Class_with_builtin_type_properties()
    {
        var theType = typeof(WithPrimitives);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType)
                .WithProperty("aBoolean", "boolean")
                .WithProperty("anInteger", "integer")
                .WithProperty("anUnsignedInteger", "uint32")
                .WithProperty("aDouble", "double")
                .WithProperty("aDecimal", "decimal")
                .WithProperty("aString", "string")
                .WithProperty("aDateTime", "utcDateTime")
                .WithProperty("anObject", "unknown")
            ));
    }

    [Fact]
    public void Nested_types_generate_a_model_each()
    {
        var theType = typeof(WithStructuredProperty);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType).WithProperty("empty", "EmptyClass")),
            IsEmptyTypeModel
            );
    }

    [Fact]
    public void Shows_nullable_properties()
    {
        var theType = typeof(WithNullables);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType)
                .WithProperty("aBoolean?", "boolean")
                .WithProperty("isNullable?", "EmptyClass")
                .WithProperty("nullableArray?", "string[]")
                .WithProperty("nullableDate?", "utcDateTime")
                .WithProperty("nullableString?", "string")
            ),
            IsEmptyTypeModel
        );
    }
    
    [Fact]
    public void Structs_are_nested_too()
    {
        var theType = typeof(WithStruct);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(WithStructModel),
            IsTsType(StructModel)
        );
    }

    [Fact]
    public void Handles_subclasses()
    {
        var theType = typeof(ADerivedClass);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes,
            IsTsType(WithStructModel),
            IsTsType(StructModel),
            IsTsType(new TsModel(theType, WithStructModel).WithProperty("fromSubclass", "string"))
        );
    }
    [Fact]
    public void Converts_collections()
    {
        var theType = typeof(WithCollections);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes,
            IsTsType(new TsModel(theType)
                .WithProperty("aList", "EmptyClass[]")
                .WithProperty("anArray", "EmptyClass[]")),
            IsEmptyTypeModel);
    }

    [Fact]
    public void Converts_dictionaries_to_unknown()
    {
        var theType = typeof(WithDictionary);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes,
            IsTsType(new TsModel(theType)
                .WithProperty("anIGenericDictionary", "unknown")
                .WithProperty("anIDictionary", "unknown")
                .WithProperty("anIImmutableDictionary", "unknown")
                .WithProperty("anIReadonlyDictionary", "unknown")
                .WithProperty("aGenericDictionary", "unknown")
                .WithProperty("anImmutableDictionary", "unknown")
                .WithProperty("aReadonlyDictionary", "unknown")
            ));
    }

    [Fact]
    public void Converts_recursive_types()
    {
        var theType = typeof(Recursive);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes,
            IsTsType(new TsModel(theType).WithProperty("aRecursive?", "Recursive"))
        );
    }

    [Fact]
    public void Converts_enumerations()
    {
        var theType = typeof(WithEnumeration);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes,
            IsTsType(new TsModel(theType)
                .WithProperty("anEnumeration", "Enumeration")
                .WithProperty("maybeEnumeration?", "Enumeration")
                .WithProperty("enumerations", "Enumeration[]")),
            IsTsType(EnumerationModel)
        );
    }

    public static Action<TsType> IsTsType(TsType tsType) => actual => Assert.Equal(tsType, actual);

    private class EmptyClass {}

    private class WithCamelCase
    {
        public bool UpperCase => true;
        public bool lowerCase => true;
    }
    private class WithPrimitives
    {
        public bool ABoolean => false;
        public int AnInteger => -1;
        public uint AnUnsignedInteger => 1;
        public double ADouble => 1.0;
        public decimal ADecimal => 2.2m;
        public string AString => "a string";
        public DateTime ADateTime => DateTime.Now;
        public object AnObject => "an object";
    }

    private class WithStructuredProperty
    {
        public EmptyClass Empty => new();
    }

    private class WithNullables
    {
        public bool? ABoolean => false;
        public string? NullableString => null;
        public DateTime? NullableDate => null;
        public EmptyClass? IsNullable => null;
        public string[]? NullableArray => null;
    }

    private record struct StructRecord(string First, string Second);
    
    private class WithStruct
    {
        public StructRecord AStruct => new("first", "second");
    }

    private class ADerivedClass : WithStruct
    {
        public string FromSubclass => "from subclass";
    }

    private class WithCollections
    {
        public List<EmptyClass> AList => new();
        public EmptyClass[] AnArray => new [] { new EmptyClass() };
    }

    private class WithDictionary
    {
        public IDictionary<int, int> AnIGenericDictionary => new Dictionary<int, int>();
        public IDictionary AnIDictionary => new Dictionary<int, int>();
        public IImmutableDictionary<int, int> AnIImmutableDictionary => ImmutableDictionary<int, int>.Empty;
        public IReadOnlyDictionary<int, int> AnIReadonlyDictionary => ImmutableDictionary<int, int>.Empty;
        public Dictionary<int, int> AGenericDictionary => new();
        public ImmutableDictionary<int, int> AnImmutableDictionary => ImmutableDictionary<int, int>.Empty;
        public ReadOnlyDictionary<int, int> AReadonlyDictionary => ReadOnlyDictionary<int, int>.Empty;
    }

    private class Recursive
    {
        public Recursive? ARecursive => null;
    }

    internal enum Enumeration
    {
        First
    }

    private class WithEnumeration
    {
        public Enumeration AnEnumeration => Enumeration.First;
        public Enumeration? MaybeEnumeration => Enumeration.First;
        public List<Enumeration> Enumerations => new();
    }
}