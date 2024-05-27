using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Local
namespace CSharpToTypeSpecExample;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class ExampleClass
{ 
    [JsonPropertyName("aJsonName")]
    public int AnIntProperty => 5;
    public string AStringProperty => "a string";
    public bool? AMaybeBoolProperty => null;
    public int ADifferentType => 0;
}