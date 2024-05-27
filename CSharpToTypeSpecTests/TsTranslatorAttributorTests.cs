using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpToTypeSpec;
using static CSharpToTypeSpecTests.TsTranslatorTests;
using static CSharpToTypeSpec.TsSystemJsonAttributor;

namespace CSharpToTypeSpecTests;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class TsTranslatorAttributorTests
{
    private static readonly TsModel WithJsonAttributesModel = 
        new TsModel(typeof(WithJsonAttributes))
            .WithProperty("forEnum", "Enumeration")
            .WithProperty("usedName", "string");

    private readonly TsTranslator translator = new(JsonAttributor);

    [Fact]
    public void Uses_json_field_attributes()
    {
        translator.AddType(typeof(WithJsonAttributes));
        Assert.Collection(translator.TsTypes, 
            IsTsType(WithJsonAttributesModel),
            IsTsType(EnumerationModel));
    }
    
    
    private class WithJsonAttributes
    {
        [JsonPropertyName("usedName")]
        public string UnusedName => "unused name";
        
        [JsonIgnore]
        public string Ignored => "ignored";

        [JsonPropertyName("forEnum")]
        public Enumeration AnEnum  => Enumeration.First;
    }
}