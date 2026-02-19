using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Quartz.Application.Evaluating;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Metadata;

public static class SystemDetails
{
	public static string Generate(Runtime runtime)
	{
		StringBuilder builder = new();
		Scope globalScope = GetGlobalScope(runtime);
		IEnumerable<Variable> variables = GetVariables(globalScope).OrderBy(variable => variable.Name);

		List<Class> classes = [];
		List<Template> templates = [];
		Class? workspace = null;

		foreach (Variable variable in variables)
		{
			if (IsType(variable, out Class? type))
			{
				if (type.Name == TypeConstants.Workspace)
				{
					workspace = type;
					continue;
				}

				classes.Add(type);
				continue;
			}

			if (IsTemplate(variable, out Template? template))
			{
				templates.Add(template);
				continue;
			}
		}

		foreach (Class type in classes)
		{
			builder.Append(FormatClass(type));
			builder.Append(Environment.NewLine);
		}

		foreach (Template template in templates)
		{
			builder.Append(FormatTemplate(template));
			builder.Append(Environment.NewLine);
		}

		if (workspace != null)
		{
			builder.Append(FormatClass(workspace, "workspace"));
		}

		return builder.ToString();
	}

	private static string FormatClass(Class type, string? alias = null)
	{
		StringBuilder builder = new();
		string name = alias ?? type.Name;
		Class? typeBase = GetBaseClass(type);
		string @base = typeBase?.Name ?? TypeConstants.Any;
		builder.Append(name);
		if (typeBase != null || type.Name != TypeConstants.Any)
		{
			builder.Append(" from ");
			builder.Append(@base == TypeConstants.Workspace ? "workspace" : @base);
		}
		builder.AppendLine(" {");
		Scope scope = GetScope(type);
		IEnumerable<Variable> variables = GetVariables(scope);
		HashSet<string> signatures = [];
		foreach (Variable variable in variables)
		{
			if (IsOperator(variable, out Operator? щзукфещк))
			{
				Scope opScope = GetScope(щзукфещк);
				foreach (Variable opVar in GetVariables(opScope))
				{
					if (!IsOperation(opVar, out Operation? operation)) continue;
					string formatted = FormatOperation(щзукфещк.Name, operation, type.Name);
					if (!signatures.Add(formatted)) continue;
					builder.Append('\t');
					builder.Append(formatted);
					builder.AppendLine(";");
				}
				continue;
			}
			if (!variable.Name.StartsWith(TypeConstants.Type) && !variable.Name.StartsWith(TypeConstants.Template))
			{
				builder.Append('\t');
				builder.Append(variable.Name);
				builder.Append(' ');
				builder.Append(variable.Tag);
				builder.AppendLine(";");
			}
		}

		builder.AppendLine("}");
		return builder.ToString();
	}

	private static string FormatTemplate(Template template)
	{
		StringBuilder builder = new();
		string name = template.Name;
		IEnumerable<string> generics = GetGenerics(template);

		builder.Append(name);

		using IEnumerator<string> iterator = generics.GetEnumerator();
		if (iterator.MoveNext())
		{
			builder.Append('<');
			builder.Append(iterator.Current);
			while (iterator.MoveNext())
			{
				builder.Append(", ");
				builder.Append(iterator.Current);
			}
			builder.Append('>');
		}

		builder.Append(" from ");
		builder.Append(TypeConstants.Any);
		builder.AppendLine(" {");
		builder.AppendLine("}");

		return builder.ToString();
	}

	private static string FormatOperation(string name, Operation operation, string type)
	{
		List<string> parameters = operation.Parameters.ToList();
		if (type != TypeConstants.Workspace && parameters.Count > 0) parameters.RemoveAt(0);
		string args = string.Join(", ", parameters.Select(FormatParameter));
		return $"{name}({args}) {operation.Result}";
	}

	private static string FormatParameter(string type)
	{
		return $"other {(type == TypeConstants.Workspace ? "workspace" : type)}";
	}

	private static Scope GetGlobalScope(Runtime runtime)
	{
		PropertyInfo? property = typeof(Runtime).GetProperty("Builder", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not find Builder property on Runtime.");
		object? builder = property.GetValue(runtime) ?? throw new InvalidOperationException("Runtime Builder is null.");

		Type typeBuilder = builder.GetType();
		PropertyInfo? location = typeBuilder.GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Static);

		if (location == null)
		{
			FieldInfo? fieldLocation = typeBuilder.GetField("Location", BindingFlags.NonPublic | BindingFlags.Static);
			return (Scope?) fieldLocation?.GetValue(null) ?? throw new InvalidOperationException("Could not find Location on Builder.");
		}

		return (Scope?) location.GetValue(null) ?? throw new InvalidOperationException("Global Scope is null.");
	}

	private static Scope GetScope(object container)
	{
		PropertyInfo? property = typeof(Container).GetProperty("Location", BindingFlags.NonPublic | BindingFlags.Instance);
		return (Scope?) property?.GetValue(container) ?? throw new InvalidOperationException($"Could not access Scope for {container}");
	}

	private static IEnumerable<Variable> GetVariables(Scope scope)
	{
		PropertyInfo? property = typeof(Scope).GetProperty("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
		IDictionary? variables = (IDictionary?) property?.GetValue(scope);

		if (variables == null)
		{
			FieldInfo? field = typeof(Scope).GetField("Variables", BindingFlags.NonPublic | BindingFlags.Instance);
			if (field == null) yield break;
			variables = (IDictionary?) field.GetValue(scope);
		}

		if (variables == null) yield break;

		foreach (DictionaryEntry entry in variables)
		{
			if (entry.Value is not Variable variable) continue;
			yield return variable;
		}
	}

	private static Class? GetBaseClass(Class type)
	{
		if (type.Name == TypeConstants.Any) return null;

		FieldInfo[] fields = typeof(Class).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo field in fields)
		{
			if (field.FieldType != typeof(Class)) continue;
			Class? @base = (Class?) field.GetValue(type);
			if (@base != null && @base != type) return @base;
		}

		return null;
	}

	private static IEnumerable<string> GetGenerics(Template template)
	{
		FieldInfo[] fields = typeof(Template).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			if (!typeof(IEnumerable<string>).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(string)) continue;
			return (IEnumerable<string>?) field.GetValue(template) ?? Enumerable.Empty<string>();
		}

		PropertyInfo? property = typeof(Template).GetProperty("Generics");
		if (property != null)
		{
			return (IEnumerable<string>?) property.GetValue(template) ?? Enumerable.Empty<string>();
		}

		return Enumerable.Empty<string>();
	}

	private static bool IsType(Variable variable, [NotNullWhen(true)] out Class? type)
	{
		Value value = variable.Value;
		if (value.Tag == TypeConstants.Type && value.Content is Class type2)
		{
			type = type2;
			return true;
		}
		type = null;
		return false;
	}

	private static bool IsTemplate(Variable variable, [NotNullWhen(true)] out Template? template)
	{
		Value value = variable.Value;
		if (value.Tag == TypeConstants.Template && value.Content is Template template2)
		{
			template = template2;
			return true;
		}
		template = null;
		return false;
	}

	private static bool IsOperator(Variable variable, [NotNullWhen(true)] out Operator? @operator)
	{
		Value value = variable.Value;
		if (value.Tag == TypeConstants.Function && value.Content is Operator operator2)
		{
			@operator = operator2;
			return true;
		}
		@operator = null;
		return false;
	}

	private static bool IsOperation(Variable variable, [NotNullWhen(true)] out Operation? operation)
	{
		Value value = variable.Value;
		if (value.Tag == TypeConstants.Function && value.Content is Operation operation2)
		{
			operation = operation2;
			return true;
		}
		operation = null;
		return false;
	}
}
