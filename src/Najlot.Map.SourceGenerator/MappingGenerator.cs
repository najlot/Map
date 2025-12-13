using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Najlot.Map.SourceGenerator;

/// <summary>
/// Incremental source generator for automatic mapping code generation.
/// Uses caching for optimal performance.
/// </summary>
[Generator]
public class MappingGenerator : IIncrementalGenerator
{
    private const string AttributeName = "Najlot.Map.SourceGenerator.MappingAttribute";
    private const string AttributeShortName = "MappingAttribute";
    private const string AttributeNameWithoutSuffix = "Mapping";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MappingAttribute.g.cs",
            AttributeSourceCode));

        // Find all partial classes and methods with the Mapping attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source code
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Check for partial class declarations with attributes
        if (node is ClassDeclarationSyntax classDeclaration
            && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
            && classDeclaration.AttributeLists.Count > 0)
        {
            return true;
        }

        // Check for method declarations with attributes in partial classes
        if (node is MethodDeclarationSyntax methodDeclaration
            && methodDeclaration.AttributeLists.Count > 0
            && methodDeclaration.Parent is ClassDeclarationSyntax parentClass
            && parentClass.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return true;
        }

        return false;
    }

    private static MappingTarget? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                    if (symbol is IMethodSymbol attributeSymbol)
                    {
                        var attributeTypeName = attributeSymbol.ContainingType.ToDisplayString();
                        if (IsMappingAttribute(attributeTypeName))
                        {
                            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                            if (classSymbol is not null)
                            {
                                return new MappingTarget(classSymbol, MappingTargetKind.Class);
                            }
                        }
                    }
                }
            }
        }
        else if (context.Node is MethodDeclarationSyntax methodDeclaration)
        {
            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                    if (symbol is IMethodSymbol attributeSymbol)
                    {
                        var attributeTypeName = attributeSymbol.ContainingType.ToDisplayString();
                        if (IsMappingAttribute(attributeTypeName))
                        {
                            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                            if (methodSymbol is not null)
                            {
                                return new MappingTarget(methodSymbol, MappingTargetKind.Method);
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    private static bool IsMappingAttribute(string attributeTypeName)
    {
        return attributeTypeName == AttributeName
            || attributeTypeName.EndsWith("." + AttributeShortName)
            || attributeTypeName.EndsWith("." + AttributeNameWithoutSuffix);
    }

    private static void Execute(Compilation compilation, ImmutableArray<MappingTarget?> targets, SourceProductionContext context)
    {
        if (targets.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctTargets = targets.Where(t => t is not null).Distinct();

        foreach (var target in distinctTargets)
        {
            if (target!.Kind == MappingTargetKind.Class && target.Symbol is INamedTypeSymbol classSymbol)
            {
                GenerateMappingForClass(classSymbol, context);
            }
            else if (target.Kind == MappingTargetKind.Method && target.Symbol is IMethodSymbol methodSymbol)
            {
                GenerateMappingForMethod(methodSymbol, context);
            }
        }
    }

    private static void GenerateMappingForClass(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : classSymbol.ContainingNamespace.ToDisplayString();

        var className = classSymbol.Name;
        var fullClassName = classSymbol.ToDisplayString();

        // Find properties to map
        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null && p.GetMethod is not null)
            .ToList();

        if (properties.Count == 0)
        {
            return; // No mappable properties
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (namespaceName is not null)
        {
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
        }

        var indent = namespaceName is not null ? "    " : "";

        sb.AppendLine($"{indent}partial class {className}");
        sb.AppendLine($"{indent}{{");

        // Generate a simple Map method
        sb.AppendLine($"{indent}    /// <summary>");
        sb.AppendLine($"{indent}    /// Maps properties from another instance of {className}.");
        sb.AppendLine($"{indent}    /// </summary>");
        sb.AppendLine($"{indent}    public void MapFrom({className} source)");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        if (source == null)");
        sb.AppendLine($"{indent}        {{");
        sb.AppendLine($"{indent}            throw new System.ArgumentNullException(nameof(source));");
        sb.AppendLine($"{indent}        }}");
        sb.AppendLine();

        foreach (var property in properties)
        {
            sb.AppendLine($"{indent}        this.{property.Name} = source.{property.Name};");
        }

        sb.AppendLine($"{indent}    }}");

        sb.AppendLine($"{indent}}}");

        if (namespaceName is not null)
        {
            sb.AppendLine("}");
        }

        var fileName = $"{fullClassName.Replace("<", "_").Replace(">", "_").Replace(".", "_")}.g.cs";
        context.AddSource(fileName, sb.ToString());
    }

    private static void GenerateMappingForMethod(IMethodSymbol methodSymbol, SourceProductionContext context)
    {
        var containingClass = methodSymbol.ContainingType;
        if (!containingClass.IsPartial())
        {
            return; // Method must be in a partial class
        }

        var namespaceName = containingClass.ContainingNamespace.IsGlobalNamespace
            ? null
            : containingClass.ContainingNamespace.ToDisplayString();

        var className = containingClass.Name;
        var methodName = methodSymbol.Name;

        // Check if method is partial
        if (!methodSymbol.IsPartialDefinition)
        {
            return; // Only partial methods are supported
        }

        // Get method signature
        var parameters = methodSymbol.Parameters;
        if (parameters.Length != 1)
        {
            return; // Only methods with single parameter are supported
        }

        var sourceType = parameters[0].Type;
        var targetType = methodSymbol.ReturnType;

        if (targetType.SpecialType == SpecialType.System_Void)
        {
            return; // Must return a type
        }

        var sourceProperties = sourceType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null)
            .ToList();

        var targetProperties = targetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null && p.GetMethod is not null)
            .ToList();

        // Find matching properties
        var matchingProperties = new List<(IPropertySymbol Source, IPropertySymbol Target)>();
        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(tp => tp.Name == sourceProp.Name);
            if (targetProp is not null)
            {
                matchingProperties.Add((sourceProp, targetProp));
            }
        }

        if (matchingProperties.Count == 0)
        {
            return; // No matching properties
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (namespaceName is not null)
        {
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
        }

        var indent = namespaceName is not null ? "    " : "";

        sb.AppendLine($"{indent}partial class {className}");
        sb.AppendLine($"{indent}{{");

        // Generate method implementation
        var returnTypeName = targetType.ToDisplayString();
        var paramTypeName = sourceType.ToDisplayString();
        var paramName = parameters[0].Name;
        var accessibility = GetAccessibilityModifier(methodSymbol.DeclaredAccessibility);

        sb.AppendLine($"{indent}    {accessibility} partial {returnTypeName} {methodName}({paramTypeName} {paramName})");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        var result = new {returnTypeName}();");
        sb.AppendLine();

        foreach (var (sourceProp, targetProp) in matchingProperties)
        {
            sb.AppendLine($"{indent}        result.{targetProp.Name} = {paramName}.{sourceProp.Name};");
        }

        sb.AppendLine();
        sb.AppendLine($"{indent}        return result;");
        sb.AppendLine($"{indent}    }}");

        sb.AppendLine($"{indent}}}");

        if (namespaceName is not null)
        {
            sb.AppendLine("}");
        }

        var fileName = $"{containingClass.ToDisplayString().Replace("<", "_").Replace(">", "_").Replace(".", "_")}_{methodName}.g.cs";
        context.AddSource(fileName, sb.ToString());
    }

    private static string GetAccessibilityModifier(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };
    }

    private const string AttributeSourceCode = @"// <auto-generated/>
namespace Najlot.Map.SourceGenerator
{
    /// <summary>
    /// Attribute to mark partial classes or methods for automatic mapping code generation.
    /// The source generator will create mapping implementations for marked types.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MappingAttribute : System.Attribute
    {
    }
}
";
}

internal record MappingTarget(ISymbol Symbol, MappingTargetKind Kind);

internal enum MappingTargetKind
{
    Class,
    Method
}

internal static class TypeSymbolExtensions
{
    public static bool IsPartial(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(d => d.Modifiers.Any(SyntaxKind.PartialKeyword));
    }
}
