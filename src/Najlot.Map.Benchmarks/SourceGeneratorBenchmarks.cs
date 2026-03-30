using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Najlot.Map.SourceGenerator;
using System.Collections.Immutable;
using System.Text;

namespace Najlot.Map.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(BenchmarksConfig))]
public class SourceGeneratorBenchmarks
{
	private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);
	private static readonly ImmutableArray<MetadataReference> MetadataReferences = CreateMetadataReferences();

	private GeneratorBenchmarkScenario _scenario = null!;
	private GeneratorDriver _incrementalDriver = null!;

	[Params(10, 50)]
	public int MapperCount { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		_scenario = GeneratorBenchmarkScenario.Create(MapperCount, ParseOptions, MetadataReferences);
	}

	[IterationSetup(Target = nameof(IncrementalEditLatency))]
	public void SetupIncrementalDriver()
	{
		_incrementalDriver = CreateDriver()
			.RunGeneratorsAndUpdateCompilation(_scenario.OriginalCompilation, out _, out var diagnostics);

		EnsureNoDiagnostics(diagnostics);
	}

	[Benchmark(Description = "Full generator throughput")]
	public int FullGenerationThroughput()
	{
		var driver = CreateDriver();
		driver = driver.RunGeneratorsAndUpdateCompilation(_scenario.OriginalCompilation, out var outputCompilation, out var diagnostics);

		EnsureNoDiagnostics(diagnostics);
		return outputCompilation.SyntaxTrees.Count() - _scenario.OriginalCompilation.SyntaxTrees.Count();
	}

	[Benchmark(Description = "Incremental edit latency")]
	public int IncrementalEditLatency()
	{
		_ = _incrementalDriver.RunGeneratorsAndUpdateCompilation(_scenario.EditedCompilation, out var outputCompilation, out var diagnostics);

		EnsureNoDiagnostics(diagnostics);
		return outputCompilation.SyntaxTrees.Count() - _scenario.EditedCompilation.SyntaxTrees.Count();
	}

	private static GeneratorDriver CreateDriver()
	{
		return CSharpGeneratorDriver.Create(
			new ISourceGenerator[]
			{
				new MappingGenerator().AsSourceGenerator(),
				new RegistrationGenerator().AsSourceGenerator()
			},
			parseOptions: ParseOptions);
	}

	private static ImmutableArray<MetadataReference> CreateMetadataReferences()
	{
		var references = new List<MetadataReference>();
		var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedAssemblies)
		{
			foreach (var assemblyPath in trustedAssemblies.Split(Path.PathSeparator))
			{
				AddReference(references, seenPaths, assemblyPath);
			}
		}

		AddReference(references, seenPaths, typeof(object).Assembly.Location);
		AddReference(references, seenPaths, typeof(Map).Assembly.Location);
		AddReference(references, seenPaths, typeof(MappingGenerator).Assembly.Location);

		return references.ToImmutableArray();
	}

	private static void AddReference(ICollection<MetadataReference> references, ISet<string> seenPaths, string assemblyPath)
	{
		if (string.IsNullOrWhiteSpace(assemblyPath) || !seenPaths.Add(assemblyPath))
		{
			return;
		}

		references.Add(MetadataReference.CreateFromFile(assemblyPath));
	}

	private static void EnsureNoDiagnostics(ImmutableArray<Diagnostic> diagnostics)
	{
		var relevantDiagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		if (relevantDiagnostics.Length == 0)
		{
			return;
		}

		throw new InvalidOperationException(string.Join(Environment.NewLine, relevantDiagnostics.Select(d => d.ToString())));
	}

	private sealed class GeneratorBenchmarkScenario
	{
		private GeneratorBenchmarkScenario(CSharpCompilation originalCompilation, CSharpCompilation editedCompilation)
		{
			OriginalCompilation = originalCompilation;
			EditedCompilation = editedCompilation;
		}

		public CSharpCompilation OriginalCompilation { get; }
		public CSharpCompilation EditedCompilation { get; }

		public static GeneratorBenchmarkScenario Create(
			int mapperCount,
			CSharpParseOptions parseOptions,
			ImmutableArray<MetadataReference> metadataReferences)
		{
			var syntaxTrees = new List<SyntaxTree>(mapperCount);
			SyntaxTree? originalEditedTree = null;
			SyntaxTree? updatedEditedTree = null;

			for (var index = 0; index < mapperCount; index++)
			{
				var source = CreateMapperSource(index, edited: false);
				var tree = CSharpSyntaxTree.ParseText(source, parseOptions, path: $"Mapper{index}.cs");
				syntaxTrees.Add(tree);

				if (index == 0)
				{
					originalEditedTree = tree;
					updatedEditedTree = CSharpSyntaxTree.ParseText(CreateMapperSource(index, edited: true), parseOptions, path: $"Mapper{index}.cs");
				}
			}

			var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
			var originalCompilation = CSharpCompilation.Create(
				assemblyName: "Najlot.Map.SourceGenerator.BenchmarkScenario",
				syntaxTrees: syntaxTrees,
				references: metadataReferences,
				options: compilationOptions);

			var editedCompilation = originalCompilation.ReplaceSyntaxTree(originalEditedTree!, updatedEditedTree!);
			return new GeneratorBenchmarkScenario(originalCompilation, editedCompilation);
		}

		private static string CreateMapperSource(int index, bool edited)
		{
			var methodName = edited ? "MapEntityRenamed" : "MapEntity";
			var sb = new StringBuilder();
			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine("using Najlot.Map;");
			sb.AppendLine("using Najlot.Map.Attributes;");
			sb.AppendLine();
			sb.AppendLine("namespace GeneratorBenchmark;");
			sb.AppendLine();
			sb.AppendLine($"public enum SourceStatus{index} {{ Draft, Active, Archived }}");
			sb.AppendLine($"public enum TargetStatus{index} {{ Draft, Active, Archived }}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class SourceAddress{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public string Street { get; set; } = string.Empty;");
			sb.AppendLine("    public string City { get; set; } = string.Empty;");
			sb.AppendLine("    public string ZipCode { get; set; } = string.Empty;");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class TargetAddress{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public string Street { get; set; } = string.Empty;");
			sb.AppendLine("    public string City { get; set; } = string.Empty;");
			sb.AppendLine("    public string ZipCode { get; set; } = string.Empty;");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class SourceItem{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public int Id { get; set; }");
			sb.AppendLine("    public string Name { get; set; } = string.Empty;");
			sb.AppendLine("    public decimal Price { get; set; }");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class TargetItem{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public int Id { get; set; }");
			sb.AppendLine("    public string Name { get; set; } = string.Empty;");
			sb.AppendLine("    public decimal Price { get; set; }");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class SourceEntity{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public int Id { get; set; }");
			sb.AppendLine("    public string Name { get; set; } = string.Empty;");
			sb.AppendLine("    public string Description { get; set; } = string.Empty;");
			sb.AppendLine("    public DateTimeOffset CreatedAt { get; set; }");
			sb.AppendLine($"    public SourceStatus{index} Status {{ get; set; }}");
			sb.AppendLine($"    public SourceAddress{index}? Address {{ get; set; }}");
			sb.AppendLine($"    public List<SourceItem{index}> Items {{ get; set; }} = [];");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine($"public sealed class TargetEntity{index}");
			sb.AppendLine("{");
			sb.AppendLine("    public int Id { get; set; }");
			sb.AppendLine("    public string Name { get; set; } = string.Empty;");
			sb.AppendLine("    public string Description { get; set; } = string.Empty;");
			sb.AppendLine("    public DateTime CreatedAt { get; set; }");
			sb.AppendLine($"    public TargetStatus{index} Status {{ get; set; }}");
			sb.AppendLine($"    public TargetAddress{index}? Address {{ get; set; }}");
			sb.AppendLine($"    public List<TargetItem{index}> Items {{ get; set; }} = [];");
			sb.AppendLine("    public string AuditStamp { get; set; } = string.Empty;");
			sb.AppendLine("}");
			sb.AppendLine();
			sb.AppendLine("[Mapping]");
			sb.AppendLine($"public partial class Mapper{index}");
			sb.AppendLine("{");
			sb.AppendLine("    [MapIgnoreProperty(nameof(to.AuditStamp))]");
			sb.AppendLine($"    public partial void {methodName}(IMap map, SourceEntity{index} from, TargetEntity{index} to);");
			sb.AppendLine($"    public partial TargetEntity{index} CreateEntity(IMap map, SourceEntity{index} from);");
			sb.AppendLine($"    public partial TargetStatus{index} MapStatus(SourceStatus{index} from);");
			sb.AppendLine();
			sb.AppendLine("    private static DateTime MapCreatedAt(DateTimeOffset from) => from.UtcDateTime;");
			sb.AppendLine("}");
			return sb.ToString();
		}
	}

}