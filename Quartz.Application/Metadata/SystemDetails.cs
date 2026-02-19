using System.Collections;
using System.Reflection;
using System.Text;
using Quartz.Application.Evaluating;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Metadata;

public static class SystemDetails
{
	private const string Indent = "\t";
	private const string NewLine = "\n";

	public static string Generate()
	{
		StringBuilder builder = new();
		Scope globalScope = GetGlobalScope();
		IEnumerable<Variable> variables = GetVariables(globalScope).OrderBy(GetVariableName);

		List<Class> classes = [];
		List<Template> templates = [];
		Class? workspaceClass = null;

		foreach (Variable variable in variables)
		{
			if (IsType(variable, out Class? classType))
			{
				if (IsWorkspace(classType))
				{
					workspaceClass = classType;
				}
				else
				{
					classes.Add(classType);
				}
			}
			else if (IsTemplate(variable, out Template? template))
			{
				templates.Add(template);
			}
		}

		foreach (Class type in classes)
		{
			builder.Append(FormatClass(type));
			builder.Append(NewLine);
		}

		foreach (Template template in templates)
		{
			builder.Append(FormatTemplate(template));
			builder.Append(NewLine);
		}

		// Print Workspace last
		if (workspaceClass is not null)
		{
			builder.Append(FormatClass(workspaceClass, "workspace"));
		}

		return builder.ToString();
	}

	private static string FormatClass(Class type, string? displayName = null)
	{
		StringBuilder content = new();
		string name = displayName ?? type.Name;
		Class? baseClass = GetBaseClass(type);
		string baseName = baseClass?.Name ?? TypeConstants.Any;

		content.Append(name);

		if (baseClass is not null || type.Name != TypeConstants.Any)
		{
			string displayBaseName = baseName == TypeConstants.Workspace ? "workspace" : baseName;
			content.Append($" from {displayBaseName}");
		}

		content.AppendLine(" {");

		Scope scope = GetScope(type);
		IEnumerable<Variable> variables = GetVariables(scope);
		HashSet<string> signatures = [];

		foreach (Variable variable in variables)
		{
			if (IsOperator(variable, out Operator? op))
			{
				Scope opScope = GetScope(op);
				foreach (Variable opVar in GetVariables(opScope))
				{
					if (IsOperation(opVar, out Operation? operation))
					{
						string formatted = FormatOperation(op.Name, operation, type.Name);
						if (signatures.Add(formatted))
						{
							content.AppendLine($"{Indent}{formatted};");
						}
					}
				}
			}
			else if (!variable.Name.StartsWith(TypeConstants.Type) && !variable.Name.StartsWith(TypeConstants.Template))
			{
				content.AppendLine($"{Indent}{variable.Name} {variable.Tag};");
			}
		}

		content.AppendLine("}");
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

	private static string FormatOperation(string name, Operation operation, string className)
	{
		List<string> parameters = operation.Parameters.ToList();

		if (className != TypeConstants.Workspace && parameters.Count > 0)
		{
			parameters.RemoveAt(0);
		}

		string args = string.Join(", ", parameters.Select(FormatParameter));
		return $"{name}({args}) {operation.Result}";
	}

	private static string FormatParameter(string type)
	{
		string typeName = type == TypeConstants.Workspace ? "workspace" : type;
		return $"other {typeName}";
	}

	private static Scope GetGlobalScope()
	{
		PropertyInfo? property = typeof(RuntimeBuilder).GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Static);
		return (Scope?)property?.GetValue(null) ?? throw new InvalidOperationException("Could not access Global Scope");
	}

	private static Scope GetScope(object container)
	{
		PropertyInfo? property = typeof(Container).GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Instance);
		return (Scope?)property?.GetValue(container) ?? throw new InvalidOperationException($"Could not access Scope for {container}");
	}

	private static IEnumerable<Variable> GetVariables(Scope scope)
	{
		PropertyInfo? property = typeof(Scope).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
		IDictionary? variables = (IDictionary?)property?.GetValue(scope);
		
		if (variables is null)
		{
			// Fallback to field if property is not available (e.g. if implementation details change slightly or property is not auto-implemented in expected way)
			FieldInfo? field = typeof(Scope).GetField("Variables", BindingFlags.NonPublic | BindingFlags.Instance)
				?? typeof(Scope).GetField("<Variables>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			
			if (field is not null)
			{
				variables = (IDictionary?)field.GetValue(scope);
			}
		}

		if (variables is null)
		{
			yield break;
		}

		foreach (DictionaryEntry entry in variables)
		{
			if (entry.Value is Variable variable)
			{
				yield return variable;
			}
		}
	}

	private static Class? GetBaseClass(Class type)
	{
		if (type.Name == TypeConstants.Any) return null;

		FieldInfo[] fields = typeof(Class).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo field in fields)
		{
			if (field.FieldType == typeof(Class))
			{
				Class? potentialBase = (Class?)field.GetValue(type);
				// Ensure we aren't picking up something else, though Class only has one Class field usually (base)
				if (potentialBase != null && potentialBase != type)
				{
					return potentialBase;
				}
			}
		}
		
		return null;
	}

	private static IEnumerable<string> GetGenerics(Template template)
	{
		FieldInfo[] fields = typeof(Template).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			if (typeof(IEnumerable<string>).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
			{
				return (IEnumerable<string>?)field.GetValue(template) ?? Enumerable.Empty<string>();
			}
		}
		
		PropertyInfo? property = typeof(Template).GetProperty("Generics");
		if (property != null)
		{
			return (IEnumerable<string>?)property.GetValue(template) ?? Enumerable.Empty<string>();
		}

		return Enumerable.Empty<string>();
	}

	private static string GetVariableName(Variable variable) => variable.Name;

	private static bool IsType(Variable variable, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Class? classType)
	{
		if (variable.Value.Tag == TypeConstants.Type && variable.Value.Content is Class c)
		{
			classType = c;
			return true;
		}
		classType = null;
		return false;
	}

	private static bool IsTemplate(Variable variable, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Template? template)
	{
		if (variable.Value.Tag == TypeConstants.Template && variable.Value.Content is Template t)
		{
			template = t;
			return true;
		}
		template = null;
		return false;
	}

	private static bool IsWorkspace(Class classType)
	{
		return classType.Name == TypeConstants.Workspace;
	}

	private static bool IsOperator(Variable variable, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Operator? op)
	{
		if (variable.Value.Tag == TypeConstants.Function && variable.Value.Content is Operator o)
		{
			op = o;
			return true;
		}
		op = null;
		return false;
	}

	private static bool IsOperation(Variable variable, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Operation? operation)
	{
		if (variable.Value.Tag == TypeConstants.Function && variable.Value.Content is Operation o)
		{
			operation = o;
			return true;
		}
		operation = null;
		return false;
	}
}
