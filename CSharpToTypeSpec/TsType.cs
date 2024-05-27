namespace CSharpToTypeSpec;

public class TsType
{
    protected readonly Type theType;

    protected TsType(Type theType)
    {
        this.theType = theType;
    }

    public bool HasType(Type aType) => theType == aType;

    public override string ToString() => theType.Name;
}