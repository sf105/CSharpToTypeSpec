using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpToTypeSpec;
using static CSharpToTypeSpec.TsSystemJsonAttributor;
using static CSharpToTypeSpecTests.TsTranslatorTests;

namespace CSharpToTypeSpecTests;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class TsTranslatorOverrideTests
{
    private static readonly Dictionary<string, string> Overrides = new()
    {
        {"anIntegerProperty", "notAnInteger"},
        { "aDictionary", "a dictionary type"},
        {"aJsonName", "aJsonType"}
    };
    private readonly TsTranslator translator = new(JsonAttributor, Overrides);

    [Fact]
    public void Overrides_type_for_property()
    {
        var theType = typeof(WithProperties);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType)
                .WithProperty("anIntegerProperty", "notAnInteger")
                .WithProperty("aDictionary", "a dictionary type")
            )
        );
    }

    [Fact]
    public void Overrides_json_names()
    {
        var theType = typeof(WithJsonAttributes);
        translator.AddType(theType);
        Assert.Collection(translator.TsTypes, 
            IsTsType(new TsModel(theType).WithProperty("aJsonName", "aJsonType"))
        );
    }

    private class WithProperties
    {
        public int AnIntegerProperty => 0;
        public IDictionary ADictionary => new Dictionary<int, int>();

    }

    private class WithJsonAttributes
    {
        [JsonPropertyName("aJsonName")]
        public string UnusedName => "unused name";
        
        [JsonIgnore]
        public int AnIntegerProperty => 0;
    }
}