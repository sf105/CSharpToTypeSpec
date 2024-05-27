using CSharpToTypeSpec;

namespace CSharpToTypeSpecTests;

public class TsTypesTests
{
    [Fact]
    public void Renders_a_Model_as_TypeSpec_code()
    {
        var model = new TsModel(typeof(TargetType)).WithProperty("name", "type");
        Assert.Equal("model TargetType {\n  name: type\n}", model.ToString());
    }

    [Fact]
    public void Renders_an_Enum_as_TypeSpec_code()
    {
        var model = new TsEnum(typeof(TargetEnum), ["First", "Second"]);
        Assert.Equal("enum TargetEnum {\n  First,\n  Second\n}", model.ToString());
    }

}

internal class TargetType
{
}

internal enum TargetEnum
{
    
}