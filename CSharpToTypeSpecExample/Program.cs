using CSharpToTypeSpec;
using CSharpToTypeSpecExample;

// This should generate the same contents as ExampleModel.tsp

var typeOverrides = new Dictionary<string, string> { {"aDifferentType", "decimal"} };
TsTranslator.WriteAsTypeSpec(
    typeof(ExampleClass), "example", new TsSystemJsonAttributor(), Console.Out,
    typeOverrides);


