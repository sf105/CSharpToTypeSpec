using CSharpToTypeSpec;
using CSharpToTypeSpecExample;

// This should generate the same contents as ExampleModel.tsp
TsTranslator.WriteAsTypeSpec(
    typeof(ExampleClass), "example", new TsSystemJsonAttributor(), Console.Out,
    new Dictionary<string, string> { {"aDifferentType", "decimal"} });


