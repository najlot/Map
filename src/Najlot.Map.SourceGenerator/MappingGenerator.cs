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

        // Find all partial methods in the class with signature: partial void MapFrom(IMap map, TSource from, TTarget to)
        var partialMethods = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsPartialDefinition 
                     && m.Name == "MapFrom" 
                     && m.ReturnsVoid
                     && m.Parameters.Length == 3)
            .ToList();

        if (partialMethods.Count == 0)
        {
            return; // No partial methods to implement
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

        // Generate implementation for each partial method
        foreach (var method in partialMethods)
        {
            GenerateMapFromImplementation(method, sb, indent);
        }

        sb.AppendLine($"{indent}}}");

        if (namespaceName is not null)
        {
            sb.AppendLine("}");
        }

        var fileName = $"{fullClassName.Replace("<", "_").Replace(">", "_").Replace(".", "_")}.g.cs";
        context.AddSource(fileName, sb.ToString());
    }

    private static void GenerateMapFromImplementation(IMethodSymbol method, StringBuilder sb, string indent)
    {
        var parameters = method.Parameters;
        if (parameters.Length != 3)
        {
            return;
        }

        // First parameter should be IMap
        var mapParam = parameters[0];
        var sourceParam = parameters[1];
        var targetParam = parameters[2];

        var sourceType = sourceParam.Type;
        var targetType = targetParam.Type;
        var containingType = method.ContainingType;

        // Get ignored properties from MapIgnoreProperty attributes
        var ignoredProperties = new HashSet<string>();
        foreach (var attr in method.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "MapIgnorePropertyAttribute" || 
                attr.AttributeClass?.Name == "MapIgnoreProperty")
            {
                if (attr.ConstructorArguments.Length > 0 && 
                    attr.ConstructorArguments[0].Value is string propertyName)
                {
                    ignoredProperties.Add(propertyName);
                }
            }
        }

        // Get properties from source and target types
        var sourceProperties = sourceType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null)
            .ToList();

        var targetProperties = targetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null)
            .ToList();

        // Generate method signature
        var accessibility = GetAccessibilityModifier(method.DeclaredAccessibility);
        var mapTypeName = mapParam.Type.ToDisplayString();
        var sourceTypeName = sourceType.ToDisplayString();
        var targetTypeName = targetType.ToDisplayString();

        sb.AppendLine($"{indent}    {accessibility} partial void MapFrom({mapTypeName} {mapParam.Name}, {sourceTypeName} {sourceParam.Name}, {targetTypeName} {targetParam.Name})");
        sb.AppendLine($"{indent}    {{");

        // Generate mapping logic for each matching property
        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(tp => tp.Name == sourceProp.Name);
            if (targetProp is null)
            {
                continue;
            }

            // Check if this property should be ignored
            if (ignoredProperties.Contains(targetProp.Name))
            {
                continue;
            }

            GeneratePropertyMapping(sourceProp, targetProp, sourceParam.Name, targetParam.Name, mapParam.Name, containingType, sb, indent);
        }

        sb.AppendLine($"{indent}    }}");
        sb.AppendLine();
    }

    private static void GeneratePropertyMapping(
        IPropertySymbol sourceProperty,
        IPropertySymbol targetProperty,
        string sourceParamName,
        string targetParamName,
        string mapParamName,
        INamedTypeSymbol containingType,
        StringBuilder sb,
        string indent)
    {
        var sourcePropertyType = sourceProperty.Type;
        var targetPropertyType = targetProperty.Type;

        // Check if the types can be directly assigned
        if (CanDirectlyAssign(sourcePropertyType, targetPropertyType))
        {
            // Direct assignment for simple types
            sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {sourceParamName}.{sourceProperty.Name};");
        }
        else
        {
            // Check for custom mapping method in the containing class
            var customMapMethod = FindCustomMappingMethod(containingType, sourcePropertyType, targetPropertyType);
            if (customMapMethod is not null)
            {
                // Use custom mapping method
                sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {customMapMethod.Name}({sourceParamName}.{sourceProperty.Name});");
            }
            else if (IsCollectionType(sourcePropertyType) && IsCollectionType(targetPropertyType))
            {
                // For collections, use IMap.From().ToList() or ToArray()
                var sourceElementType = GetCollectionElementType(sourcePropertyType);
                var targetElementType = GetCollectionElementType(targetPropertyType);
                
                if (sourceElementType is not null && targetElementType is not null)
                {
                    var targetTypeName = targetPropertyType.ToDisplayString();
                    
                    // Determine which method to call based on the target type
                    if (targetTypeName.Contains("List<"))
                    {
                        sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {mapParamName}.From<{sourceElementType.ToDisplayString()}>({sourceParamName}.{sourceProperty.Name}).ToList<{targetElementType.ToDisplayString()}>();");
                    }
                    else if (targetTypeName.Contains("[]"))
                    {
                        sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {mapParamName}.From<{sourceElementType.ToDisplayString()}>({sourceParamName}.{sourceProperty.Name}).ToArray<{targetElementType.ToDisplayString()}>();");
                    }
                    else
                    {
                        // Default to To<T>() for IEnumerable<T>
                        sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {mapParamName}.From<{sourceElementType.ToDisplayString()}>({sourceParamName}.{sourceProperty.Name}).To<{targetElementType.ToDisplayString()}>();");
                    }
                }
            }
            else
            {
                // For single objects, check if target is a value type or can be assigned
                if (targetPropertyType.IsValueType)
                {
                    // For value types, we need to create and assign
                    sb.AppendLine($"{indent}        {targetParamName}.{targetProperty.Name} = {mapParamName}.From({sourceParamName}.{sourceProperty.Name}).To<{targetPropertyType.ToDisplayString()}>();");
                }
                else
                {
                    // For reference types (objects), initialize if null and then map
                    var targetTypeDisplayString = targetPropertyType.ToDisplayString();
                    // Remove nullable annotation for instantiation
                    var targetTypeForInstantiation = targetTypeDisplayString.TrimEnd('?');
                    
                    sb.AppendLine($"{indent}        if ({targetParamName}.{targetProperty.Name} == null)");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            {targetParamName}.{targetProperty.Name} = new {targetTypeForInstantiation}();");
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}        {mapParamName}.From({sourceParamName}.{sourceProperty.Name}).To({targetParamName}.{targetProperty.Name});");
                }
            }
        }
    }

    private static IMethodSymbol? FindCustomMappingMethod(INamedTypeSymbol containingType, ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Look for a method with signature: TTarget MapFrom(TSource source)
        var methods = containingType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.Name == "MapFrom" 
                     && !m.IsPartialDefinition
                     && m.Parameters.Length == 1
                     && !m.ReturnsVoid);

        foreach (var method in methods)
        {
            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, sourceType) &&
                SymbolEqualityComparer.Default.Equals(method.ReturnType, targetType))
            {
                return method;
            }
        }

        return null;
    }

    private static bool CanDirectlyAssign(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        // Check if types are the same
        if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
        {
            return true;
        }

        // Check if source type is implicitly convertible to target type
        // For now, we'll keep it simple and only allow exact matches or built-in conversions
        return sourceType.SpecialType != SpecialType.None && sourceType.SpecialType == targetType.SpecialType;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        // Check if type implements IEnumerable<T>
        if (type is INamedTypeSymbol namedType)
        {
            // Check for List<T>, IEnumerable<T>, ICollection<T>, etc.
            if (namedType.Name == "List" || namedType.Name == "IEnumerable" || namedType.Name == "ICollection" || namedType.Name == "IList")
            {
                return namedType.IsGenericType;
            }

            // Check if it implements IEnumerable<T>
            foreach (var @interface in namedType.AllInterfaces)
            {
                if (@interface.Name == "IEnumerable" && @interface.IsGenericType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            // For List<T>, IEnumerable<T>, etc., get the T
            if (namedType.TypeArguments.Length > 0)
            {
                return namedType.TypeArguments[0];
            }

            // Check interfaces for IEnumerable<T>
            foreach (var @interface in namedType.AllInterfaces)
            {
                if (@interface.Name == "IEnumerable" && @interface.IsGenericType && @interface.TypeArguments.Length > 0)
                {
                    return @interface.TypeArguments[0];
                }
            }
        }

        return null;
    }

    private static void GenerateMappingForMethod(IMethodSymbol methodSymbol, SourceProductionContext context)
    {
        var containingClass = methodSymbol.ContainingType;
        if (!containingClass.IsPartial())
        {
            return; // Method must be in a partial class
        }

        // Check if method is partial
        if (!methodSymbol.IsPartialDefinition)
        {
            return; // Only partial methods are supported
        }

        var parameters = methodSymbol.Parameters;

        // Handle two cases:
        // 1. void MapFrom(IMap map, TSource from, TTarget to) - 3 parameters
        // 2. TTarget MapToTarget(TSource source) - 1 parameter with return type
        
        if (parameters.Length == 3 && methodSymbol.ReturnsVoid && methodSymbol.Name == "MapFrom")
        {
            // This is a method-level [Mapping] attribute on a void MapFrom method
            // Generate it the same way as class-level mappings
            GenerateMappingForMethodWithThreeParams(methodSymbol, context);
        }
        else if (parameters.Length == 1 && !methodSymbol.ReturnsVoid)
        {
            // This is a method-level [Mapping] attribute on a method with return type
            GenerateMappingForMethodWithOneParam(methodSymbol, context);
        }
    }

    private static void GenerateMappingForMethodWithThreeParams(IMethodSymbol methodSymbol, SourceProductionContext context)
    {
        var containingClass = methodSymbol.ContainingType;
        var namespaceName = containingClass.ContainingNamespace.IsGlobalNamespace
            ? null
            : containingClass.ContainingNamespace.ToDisplayString();

        var className = containingClass.Name;
        var fullClassName = containingClass.ToDisplayString();

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

        GenerateMapFromImplementation(methodSymbol, sb, indent);

        sb.AppendLine($"{indent}}}");

        if (namespaceName is not null)
        {
            sb.AppendLine("}");
        }

        var fileName = $"{fullClassName.Replace("<", "_").Replace(">", "_").Replace(".", "_")}_{methodSymbol.Name}_{methodSymbol.Parameters[1].Type.Name}_{methodSymbol.Parameters[2].Type.Name}.g.cs";
        context.AddSource(fileName, sb.ToString());
    }

    private static void GenerateMappingForMethodWithOneParam(IMethodSymbol methodSymbol, SourceProductionContext context)
    {
        var containingClass = methodSymbol.ContainingType;
        var namespaceName = containingClass.ContainingNamespace.IsGlobalNamespace
            ? null
            : containingClass.ContainingNamespace.ToDisplayString();

        var className = containingClass.Name;
        var methodName = methodSymbol.Name;

        var parameters = methodSymbol.Parameters;
        var sourceType = parameters[0].Type;
        var targetType = methodSymbol.ReturnType;

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
