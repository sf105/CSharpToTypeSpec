namespace CSharpToTypeSpec;

public sealed class TsModel(Type theType, TsModel? extends = null) : TsType(theType)
{
    private readonly SortedDictionary<string, string> properties = new();

    public void AddProperty(TsModelDescription description) => WithProperty(description.Name, description.TypeName);

    public TsModel WithProperty(string name, string typeName)
    {
        properties.Add(name, typeName);
        return this;
    }

    public override bool Equals(object? obj) =>
        obj is TsModel model && model.ToString().Equals(ToString()); // Brutal but it works

    public override int GetHashCode() => HashCode.Combine(properties, theType);

    public override string ToString() =>
        "model " + base.ToString() + ExtendsString + " {\n  "
        + string.Join(",\n  ", properties.Select(pair => pair.Key + ": " + pair.Value))
        + "\n}";

    private string ExtendsString => extends == null ? "" : " extends " + extends.theType.Name;
}

public record TsModelDescription(string Name, string TypeName); 

