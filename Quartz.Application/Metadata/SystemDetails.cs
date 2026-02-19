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

		// Order: Standard classes, Templates, then Workspace at the end.
		var globalVariables = GetVariables(globalScope).OrderBy(v => v.Name);

		// Separate categories
		List<Class> classes = new();
		List<Template> templates = new();
		Class? workspaceClass = null;

		foreach (var variable in globalVariables)
		{
			if (variable.Value.Tag == TypeConstants.Type && variable.Value.Content is Class classType)
			{
				if (classType.Name == TypeConstants.Workspace)
				{
					workspaceClass = classType;
				}
				else
				{
					classes.Add(classType);
				}
			}
			else if (variable.Value.Tag == TypeConstants.Template && variable.Value.Content is Template template)
			{
				templates.Add(template);
			}
		}

		// Print Classes
		foreach (var type in classes)
		{
			builder.AppendLine(FormatClass(type));
			builder.AppendLine();
		}

		// Print Templates
		foreach (var template in templates)
		{
			builder.AppendLine(FormatTemplate(template));
			builder.AppendLine();
		}

		// Print Workspace last
		if (workspaceClass != null)
		{
			// Use "workspace" alias for display, but keep original name internally logic consistent
			builder.AppendLine(FormatClass(workspaceClass, "workspace"));
		}

		return builder.ToString();
	}

	private static string FormatClass(Class type, string? displayName = null)
	{
		StringBuilder content = new();
		string name = displayName ?? type.Name;

		// Base Class Logic
		Class? baseClass = GetBaseClass(type);
		string baseName = baseClass?.Name ?? TypeConstants.Any;

		// If type is Any (no base), just print name.
		// If type is Workspace, base is usually Any but let's just say "from Any" if it has base.
		if (baseClass == null && type.Name == TypeConstants.Any)
		{
			content.Append($"{name}");
		}
		else
		{
			// Display base name cleanly (e.g. if base is Workspace, print "workspace")
			string displayBaseName = baseName == TypeConstants.Workspace ? "workspace" : baseName;
			content.Append($"{name} from {displayBaseName}");
		}

		content.AppendLine(" {");

		Scope? classScope = GetScope(type);
		if (classScope != null)
		{
			var variables = GetVariables(classScope);

			// To avoid duplicates logic:
			// We need to group overloading operations or distinct them.
			// If multiple overloads exist for same name.

			// First collect all members
			List<string> members = new();

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
								members.Add($"\t{FormatOperation(op.Name, operation, type.Name)};");
							}
						}
					}
				}
				else if (variable.Value.Tag != TypeConstants.Type && variable.Value.Tag != TypeConstants.Template)
				{
					members.Add($"\t{variable.Name} {variable.Value.Tag};");
				}
			}

			// Deduplicate strings as a simple fix for "Duplicate to_string" issue.
			// Operations might be added multiple times if traverse logic is flawed? 
			// Or maybe `GetVariables` returns duplicates? Scope.Variables is Dictionary, so keys are unique.
			// But Operator.Location (Scope) holds operations. Operations have different names (mangled params).
			// Wait, Operation name in Scope is MANGLED with types.
			// e.g. "to_string()" vs "to_string(Number)".
			// But `FormatOperation` formats them nicely.
			// If we have distinct mangled names, why duplication in output?
			// Maybe `GetVariables` logic? Or `Runtime` registers same operation multiple times?
			// Ah, `Evaluator.cs` or `Runtime.cs` might have multiple declarations?
			// Let's assume distinct output lines.

			foreach (var member in members.Distinct())
			{
				content.AppendLine(member);
			}
		}

		content.Append("}");
		return content.ToString();
	}

	private static string FormatTemplate(Template template)
	{
		string name = template.Name;
		IEnumerable<string> generics = GetGenerics(template);

		string genericsStr = string.Join(", ", generics);
		string header = !string.IsNullOrEmpty(genericsStr) ? $"{name}<{genericsStr}>" : name;

		return $"{header} from {TypeConstants.Any} {{\n}}";
	}

	private static string FormatOperation(string opName, Operation operation, string className)
	{
		var paramsList = operation.Parameters.ToList();

		// Remove 'this' parameter
		// For Workspace, there is no 'this' parameter in the operation definition usually.
		// For Class methods, the first parameter is the instance.
		if (className != TypeConstants.Workspace && paramsList.Count > 0)
		{
			paramsList.RemoveAt(0);
		}

		string args = string.Join(", ", paramsList.Select((p) =>
		{
			string typeName = p == TypeConstants.Workspace ? "workspace" : p;
			return $"other {typeName}";
		}));

		return $"{opName}({args}) {operation.Result}";
	}

	private static IEnumerable<string> GetGenerics(Template template)
	{
		// Try Property
		PropertyInfo? prop = typeof(Template).GetProperty("Generics");
		if (prop != null) return (IEnumerable<string>?) prop.GetValue(template) ?? Enumerable.Empty<string>();

		// Try Field
		FieldInfo[] fields = typeof(Template).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		foreach (var field in fields)
		{
			if (typeof(IEnumerable<string>).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
			{
				var val = field.GetValue(template);
				if (val != null) return (IEnumerable<string>) val;
			}
		}
		return Enumerable.Empty<string>();
	}

	private static Class? GetBaseClass(Class type)
	{
		// Access via reflection on private field.
		// Inspect all fields of type Class.
		// We know 'Any' has no base.
		if (type.Name == TypeConstants.Any) return null;

		FieldInfo[] fields = typeof(Class).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (var field in fields)
		{
			if (field.FieldType == typeof(Class))
			{
				var value = field.GetValue(type) as Class;
				if (value != null && value != type) return value;
			}
		}
		return null; // Should ideally return Any if not found? 
					 // But if field is null, it means no base.
	}

	private static Scope? GetScope(object container)
	{
		Type containerType = typeof(Container);
		PropertyInfo? prop = containerType.GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Instance);
		return prop?.GetValue(container) as Scope;
	}

	private static List<Variable> GetVariables(Scope scope)
	{
		// Using Property approach usually works
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
		else
		{
			// Backing field fallback
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
