using System.Reflection;
using System.Text.Json.Serialization;

namespace CSharpToTypeSpec;

public class TsSystemJsonAttributor : IJsonAttributor
{
    public string? JsonPropertyName(MemberInfo member) => member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

    public bool IsJsonIgnored(MemberInfo member) => member.GetCustomAttribute<JsonIgnoreAttribute>() != null;
    public bool IsNullableProperty(MemberInfo member) => false;

    public static readonly IJsonAttributor JsonAttributor = new TsSystemJsonAttributor();
}

// A version for NewtonSoft
// public class NewtonSoftJsonAttributor : IJsonAttributor
// {
//     public string? JsonPropertyName(MemberInfo member) => member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
//     public bool IsJsonIgnored(MemberInfo member) => member.GetCustomAttribute<JsonIgnoreAttribute>() != null;
//     public bool IsNullableProperty(MemberInfo member)
//     {
//         var attribute = member.GetCustomAttribute<JsonPropertyAttribute>();
//         return attribute != null 
//                && (attribute.DefaultValueHandling == DefaultValueHandling.Ignore 
//                    || attribute.NullValueHandling == NullValueHandling.Ignore);
//     }
// }