using Najlot.Map.Attributes;
using Najlot.Map.Exceptions;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Najlot.Map;

public partial class Map
{
	private bool HasIgnoreNethodAttribute(Delegate @delegate)
	{
		return @delegate.Method.CustomAttributes.Any(a => a.AttributeType == typeof(MapIgnoreMethodAttribute));
	}

	public void Validate()
	{
		var sb = new StringBuilder();

		foreach (var @delegate in _mapDelegates)
		{
			if (HasIgnoreNethodAttribute(@delegate))
			{
				continue;
			}

			CheckDelegate(@delegate, sb);
		}

		foreach (var @delegate in _mapFactoryDelegates)
		{
			if (HasIgnoreNethodAttribute(@delegate))
			{
				continue;
			}

			CheckDelegate(@delegate, sb);
		}

		if (sb.Length > 0)
		{
			throw new MapMissPropertiesException(sb.ToString());
		}
	}

	private void CheckDelegate(Delegate @delegate, StringBuilder sb)
	{
		var method = @delegate.Method;
		var parameters = method.GetParameters();
		ParameterInfo sourceParameter;
		ParameterInfo targetParameter;

		if (method.ReturnType == typeof(void))
		{
			if (parameters.Length == 2)
			{
				sourceParameter = parameters[0];
				targetParameter = parameters[1];
			}
			else if (parameters.Length == 3)
			{
				sourceParameter = parameters[1];
				targetParameter = parameters[2];
			}
			else
			{
				throw new InvalidOperationException($"Method {method.Name} has {parameters.Length} parameters.");
			}
		}
		else
		{
			if (parameters.Length == 1)
			{
				sourceParameter = parameters[0];
			}
			else if (parameters.Length == 2)
			{
				sourceParameter = parameters[1];
			}
			else
			{
				throw new InvalidOperationException($"Method {method.Name} has {parameters.Length} parameters.");
			}

			targetParameter = method.ReturnParameter;
		}

		var sourceType = sourceParameter.ParameterType;
		var targetType = targetParameter.ParameterType;

		var assignedProperties = GetCalledTargetProperties(method, sourceType, targetType, new HashSet<MethodInfo>()).ToArray();
		var ignoredProperties = GetIgnoredProperties(method, sourceType, targetType, new HashSet<MethodInfo>()).ToArray();

		var unmappedProperties = targetType
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
			.Where(p => p.CanWrite && p.SetMethod != null && p.SetMethod.IsPublic)
			.Select(p => p.Name)
			.Where(p => !(assignedProperties.Contains(p) || ignoredProperties.Contains(p)))
			.ToArray();

		if (unmappedProperties.Length > 0)
		{
			var declaringTypeName = method.DeclaringType?.Name;
			if (!string.IsNullOrEmpty(declaringTypeName)) declaringTypeName += ".";

			var sourceProperties = sourceType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
				.Select(p => p.Name)
				.ToArray();

			sb.AppendLine($"Method {declaringTypeName}.{method.Name}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.FullName} {p.Name}"))}) does not map the following properties:");

			foreach (var property in unmappedProperties)
			{
				sb.AppendLine($"\t{property}");
			}

			sb.AppendLine();
			sb.AppendLine("Suggestion:");

			var targetAssignment = string.IsNullOrEmpty(targetParameter.Name) ? "" : targetParameter.Name + ".";
			var suffix = string.IsNullOrEmpty(targetParameter.Name) ? "," : ";";
			foreach (var property in unmappedProperties.Where(p => sourceProperties.Contains(p)))
			{
				sb.AppendLine($"\t{targetAssignment}{property} = {sourceParameter.Name}.{property}{suffix}");
			}

			foreach (var property in unmappedProperties.Where(p => !sourceProperties.Contains(p)))
			{
				if (method.ReturnType == typeof(void))
				{
					sb.AppendLine($"\t[MapIgnoreProperty(nameof({targetParameter.Name}.{property}))]");
				}
				else
				{
					sb.AppendLine($"\t[MapIgnoreProperty(nameof({targetType.FullName}.{property}))]");
				}
			}

			sb.AppendLine();
		}
	}

	private IEnumerable<string> GetIgnoredProperties(MethodInfo method, Type sourceType, Type targetType, HashSet<MethodInfo> visitedMethods)
	{
		// Avoid infinite recursion
		if (!visitedMethods.Add(method))
		{
			yield break;
		}

		// Get directly ignored properties from this method
		var directIgnored = method.CustomAttributes
			.Where(a => a.AttributeType == typeof(MapIgnorePropertyAttribute))
			.Select(a => a.ConstructorArguments[0].Value?.ToString())
			.Where(p => p is not null)
			.Cast<string>();

		foreach (var prop in directIgnored)
		{
			yield return prop;
		}

		// Get ignored properties from called mapping methods
		foreach (var calledMethod in GetCalledMappingMethods(method, sourceType, targetType))
		{
			foreach (var prop in GetIgnoredProperties(calledMethod, sourceType, targetType, visitedMethods))
			{
				yield return prop;
			}
		}
	}

	private IEnumerable<string> GetCalledTargetProperties(MethodInfo method, Type sourceType, Type targetType, HashSet<MethodInfo> visitedMethods)
	{
		// Avoid infinite recursion
		if (!visitedMethods.Add(method))
		{
			yield break;
		}

		// Get directly assigned properties from this method
		foreach (var calledMethod in GetAssignedProperties(method))
		{
			if (calledMethod.DeclaringType == targetType)
			{
				yield return calledMethod.Name.Substring(4);
			}
		}

		// Get assigned properties from called mapping methods
		foreach (var calledMethod in GetCalledMappingMethods(method, sourceType, targetType))
		{
			foreach (var prop in GetCalledTargetProperties(calledMethod, sourceType, targetType, visitedMethods))
			{
				yield return prop;
			}
		}
	}

	private IEnumerable<MethodInfo> GetCalledMappingMethods(MethodInfo method, Type sourceType, Type targetType)
	{
		var body = method.GetMethodBody();
		Module module = method.Module;

		byte[]? il = body?.GetILAsByteArray();

		if (il is null)
		{
			yield break;
		}

		var operandType = OperandType.InlineNone;

		for (int position = 0; position < il.Length; position += GetOperandSize(operandType))
		{
			var opCode = ReadOpCode(il, ref position);
			operandType = opCode.OperandType;

			if (operandType == OperandType.InlineMethod)
			{
				int metadataToken = BitConverter.ToInt32(il, position);
				var calledMethod = module.ResolveMethod(metadataToken) as MethodInfo;
				
				if (calledMethod is not null && IsMappingMethod(calledMethod, sourceType, targetType))
				{
					yield return calledMethod;
				}
			}
		}
	}

	private bool IsMappingMethod(MethodInfo method, Type sourceType, Type targetType)
	{
		// Check if this method is registered as a mapping delegate
		foreach (var @delegate in _mapDelegates)
		{
			if (@delegate.Method == method)
			{
				return true;
			}
		}

		// Check if the method signature matches a mapping method pattern
		// For void methods: (source, target) or (IMap, source, target)
		// For non-void methods: (source) => target or (IMap, source) => target
		var parameters = method.GetParameters();
		
		if (method.ReturnType == typeof(void))
		{
			if (parameters.Length == 2)
			{
				return parameters[0].ParameterType == sourceType && parameters[1].ParameterType == targetType;
			}
			else if (parameters.Length == 3)
			{
				return parameters[0].ParameterType == typeof(IMap) &&
				       parameters[1].ParameterType == sourceType && 
				       parameters[2].ParameterType == targetType;
			}
		}
		else if (method.ReturnType == targetType)
		{
			if (parameters.Length == 1)
			{
				return parameters[0].ParameterType == sourceType;
			}
			else if (parameters.Length == 2)
			{
				return parameters[0].ParameterType == typeof(IMap) &&
				       parameters[1].ParameterType == sourceType;
			}
		}

		return false;
	}

	private IEnumerable<MethodInfo> GetAssignedProperties(MethodInfo method)
	{
		var body = method.GetMethodBody();
		Module module = method.Module;

		byte[]? il = body?.GetILAsByteArray();

		if (il is null)
		{
			return [];
		}

		return GetProperties(module, il);
	}

	private IEnumerable<MethodInfo> GetProperties(Module module, byte[] il)
	{
		var operandType = OperandType.InlineNone;

		for (int position = 0; position < il.Length; position += GetOperandSize(operandType))
		{
			var opCode = ReadOpCode(il, ref position);
			operandType = opCode.OperandType;

			if (operandType == OperandType.InlineMethod)
			{
				int metadataToken = BitConverter.ToInt32(il, position);
				var calledMethod = module.ResolveMethod(metadataToken) as MethodInfo;
				if (calledMethod is not null
					&& calledMethod.IsSpecialName
					&& calledMethod.Name.StartsWith("set_"))
				{
					yield return calledMethod;
				}
			}
		}
	}

	private static int GetOperandSize(OperandType operandType) => operandType switch
	{
		OperandType.InlineNone => 0,
		OperandType.ShortInlineBrTarget => 1,
		OperandType.ShortInlineI => 1,
		OperandType.ShortInlineVar => 1,
		OperandType.InlineVar => 2,
		OperandType.InlineBrTarget => 4,
		OperandType.InlineField => 4,
		OperandType.InlineI => 4,
		OperandType.InlineMethod => 4,
		OperandType.InlineSig => 4,
		OperandType.InlineString => 4,
		OperandType.InlineSwitch => 4,
		OperandType.InlineTok => 4,
		OperandType.InlineType => 4,
		OperandType.ShortInlineR => 4,
		OperandType.InlineI8 => 8,
		OperandType.InlineR => 8,
		_ => throw new InvalidOperationException($"Unknown operand type: {operandType}")
	};

	private static OpCode ReadOpCode(byte[] il, ref int position)
	{
		byte code = il[position++];

		if (code == 0xFE)
		{
			byte secondByte = il[position++];
			return TwoByteOpCodes[secondByte];
		}

		return SingleByteOpCodes[code];
	}

	private static readonly OpCode[] SingleByteOpCodes = new OpCode[256];
	private static readonly OpCode[] TwoByteOpCodes = new OpCode[256];

	static Map()
	{
		foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (field.GetValue(null) is OpCode opCode)
			{
				var c1 = OpCodes.Call;
				var c2 = OpCodes.Callvirt;

				if (opCode == c1 || opCode == c2)
				{
					Debug.WriteLine(opCode.Name);
				}

				ushort value = (ushort)opCode.Value;
				if (value < 0x100)
				{
					SingleByteOpCodes[value] = opCode;
				}
				else if ((value & 0xFF00) == 0xFE00)
				{
					TwoByteOpCodes[value & 0xFF] = opCode;
				}
			}
		}
	}
}