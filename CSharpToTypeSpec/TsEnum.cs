namespace CSharpToTypeSpec;

public sealed class TsEnum(Type theType, string[] values) : TsType(theType)
{
    public override bool Equals(object? obj) =>
        obj is TsEnum theEnum && theEnum.ToString().Equals(ToString()); // Brutal but it works

    public override int GetHashCode() => HashCode.Combine(values, theType);

    public override string ToString() =>
        "enum " + base.ToString() + " {\n  "
        + string.Join(",\n  ", values)
        + "\n}";
}