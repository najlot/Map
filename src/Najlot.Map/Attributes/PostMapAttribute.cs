namespace Najlot.Map.Attributes;

/// <summary>
/// Specifies a method to be invoked after a mapping operation completes.
/// </summary>
/// <remarks>Apply this attribute to a mapping method to designate a post-mapping callback. The specified method
/// can be used to perform additional processing after the main mapping logic has executed.</remarks>
/// <param name="methodName">The name of the method to call after the mapping process finishes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PostMapAttribute(string methodName) : Attribute
{
	/// <summary>
	/// Gets the name of the callback method to invoke after mapping.
	/// </summary>
	public string MethodName { get; } = methodName;
}
