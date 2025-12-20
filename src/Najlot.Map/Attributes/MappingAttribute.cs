namespace Najlot.Map.Attributes;

/// <summary>
/// Attribute to mark partial classes or methods for automatic mapping code generation.
/// The source generator will create mapping implementations for marked types, if the SourceGenerator added to the project.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MappingAttribute : Attribute
{
}
