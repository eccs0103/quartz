using System.Collections;
using System.Reflection;
using System.Text;
using Quartz.Application.Evaluating;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Metadata;

public static class SystemDetails
{
	public static string Generate()
	{
		StringBuilder builder = new();

		Type runtimeBuilderType = typeof(RuntimeBuilder);
		PropertyInfo? locationProperty = runtimeBuilderType.GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Static);

		if (locationProperty == null) return "Error: Could not find Location property on RuntimeBuilder.";
		if (locationProperty.GetValue(null) is not Scope globalScope) return "Error: Could not get value of Location property.";

		// Get all variables in the global scope
		var globalVariables = GetVariables(globalScope).OrderBy(v => v.Name);

		foreach (var variable in globalVariables)
		{
			if (variable.Value.Tag == TypeConstants.Type && variable.Value.Content is Class classType)
			{
				// Workspace name handling
				string name = classType.Name;
				if (name == TypeConstants.Workspace) name = "workspace";

				builder.AppendLine(FormatClass(classType, name));
				builder.AppendLine();
			}
			else if (variable.Value.Tag == TypeConstants.Template && variable.Value.Content is Template template)
			{
				builder.AppendLine(FormatTemplate(template));
				builder.AppendLine();
			}
		}

		return builder.ToString();
	}

	private static string FormatClass(Class type, string displayName)
	{
		StringBuilder content = new();

		// Get Base Class
		Class? baseClass = GetBaseClass(type);
		string baseName = baseClass?.Name ?? "Any";

		if (type.Name == "Any")
		{
			content.Append($"{displayName}");
		}
		else
		{
			if (baseName == TypeConstants.Workspace) baseName = "workspace";
			content.Append($"{displayName} from {baseName}");
		}

		content.AppendLine(" {");

		// Inspect the internal Scope of the Class
		Scope? classScope = GetScope(type);
		if (classScope != null)
		{
			var variables = GetVariables(classScope);

			foreach (var variable in variables)
			{
				if (variable.Value.Tag == TypeConstants.Function && variable.Value.Content is Operator op)
				{
					Scope? opScope = GetScope(op);
					if (opScope != null)
					{
						var opVariables = GetVariables(opScope);
						foreach (var opVar in opVariables)
						{
							if (opVar.Value.Tag == TypeConstants.Function && opVar.Value.Content is Operation operation)
							{
								content.AppendLine($"\t{FormatOperation(op.Name, operation, type.Name)};");
							}
						}
					}
				}
				else if (variable.Value.Tag != TypeConstants.Type && variable.Value.Tag != TypeConstants.Template)
				{
					content.AppendLine($"\t{variable.Name} {variable.Value.Tag};");
				}
			}
		}

		content.Append("}");
		return content.ToString();
	}

	private static string FormatTemplate(Template template)
	{
		string name = template.Name;
		IEnumerable<string> generics = Enumerable.Empty<string>();

		// Scan for generic parameters field
		// Get all instance fields including private ones
		FieldInfo[] fields = typeof(Template).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		foreach (var field in fields)
		{
			// The generics parameter is passed as IEnumerable<string>
			// We look for a field that is compatible with IEnumerable<string> and is NOT string (Name, Path)
			// Name is string (property backing field is string).
			if (typeof(IEnumerable<string>).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
			{
				var val = field.GetValue(template);
				if (val != null)
				{
					generics = (IEnumerable<string>) val;
					break;
				}
			}
		}

		string genericsStr = string.Join(", ", generics);
		if (!string.IsNullOrEmpty(genericsStr))
		{
			return $"{name}<{genericsStr}> from Any {{\n}}";
		}
		return $"{name} from Any {{\n}}";
	}

	private static string FormatOperation(string opName, Operation operation, string className)
	{
		var paramsList = operation.Parameters.ToList();

		// Remove 'this' parameter which corresponds to the class itself
		// workspace operations don't have 'this'
		if (className != TypeConstants.Workspace && paramsList.Count > 0)
		{
			paramsList.RemoveAt(0);
		}

		string args = string.Join(", ", paramsList.Select((p) =>
		{
			string typeName = p;
			if (typeName == TypeConstants.Workspace) typeName = "workspace";

			return $"other {typeName}";
		}));

		return $"{opName}({args}) {operation.Result}";
	}

	private static Class? GetBaseClass(Class type)
	{
		FieldInfo[] fields = typeof(Class).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (var field in fields)
		{
			if (field.FieldType == typeof(Class))
			{
				var value = field.GetValue(type) as Class;
				// Base class field should hold different instance or null.
				// Just in case, check reference equality
				if (value != type) return value;
			}
		}
		return null;
	}

	private static Scope? GetScope(object container)
	{
		Type containerType = typeof(Container);
		PropertyInfo? prop = containerType.GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Instance);
		return prop?.GetValue(container) as Scope;
	}

	private static List<Variable> GetVariables(Scope scope)
	{
		PropertyInfo? prop = typeof(Scope).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
		var dict = prop?.GetValue(scope) as IDictionary;

		List<Variable> result = new();
		if (dict != null)
		{
			foreach (var val in dict.Values)
			{
				if (val is Variable v) result.Add(v);
			}
		}

		// Fallback for fields
		if (dict == null)
		{
			FieldInfo? field = typeof(Scope).GetField("Variables", BindingFlags.NonPublic | BindingFlags.Instance)
				?? typeof(Scope).GetField("<Variables>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

			if (field != null)
			{
				dict = field.GetValue(scope) as IDictionary;
				if (dict != null)
				{
					foreach (var val in dict.Values)
					{
						if (val is Variable v) result.Add(v);
					}
				}
			}
		}
		return result;
	}
}
