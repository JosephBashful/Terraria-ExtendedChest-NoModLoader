using Mono.Cecil;

if (args.Length != 3)
    throw new ArgumentException("Usage: ResourceExtractor <assembly> <embedded-resource-name> <output-file>");

string assemblyPath = Path.GetFullPath(args[0]);
string resourceName = args[1];
string outputPath = Path.GetFullPath(args[2]);

using var module = ModuleDefinition.ReadModule(assemblyPath);
var resource = module.Resources
    .OfType<EmbeddedResource>()
    .SingleOrDefault(item => item.Name == resourceName)
    ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
using var source = resource.GetResourceStream();
using var destination = File.Create(outputPath);
source.CopyTo(destination);
Console.WriteLine($"Extracted local compile reference: {outputPath}");

