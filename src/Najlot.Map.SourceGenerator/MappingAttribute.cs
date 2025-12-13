namespace Najlot.Map.SourceGenerator;

/// <summary>
/// Attribute to mark partial classes or methods for automatic mapping code generation.
/// The source generator will create mapping implementations for marked types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MappingAttribute : Attribute
{
}
