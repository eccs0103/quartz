using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Quartz.Application.Evaluating;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Application.Metadata;

public static class SystemDetails
{
	public static string Generate(Runtime runtime)
	{
		StringBuilder builder = new();

		List<Class> types = RuntimeBuilder.Workspace.Scan<Class>().ToList();
		List<Template> templates = RuntimeBuilder.Workspace.Scan<Template>().ToList();
		Class workspace = types.FirstOrDefault(type => type.Name == Types.Workspace) ?? throw new InvalidOperationException($"Class '{Types.Workspace}' not found.");
		types.Remove(workspace);

		foreach (Class type in types)
		{
			builder.AppendLine(FormatClass(type));
			builder.AppendLine();
		}

		foreach (Template template in templates)
		{
			builder.AppendLine(FormatTemplate(template));
			builder.AppendLine();
		}

		builder.Append(FormatClass(workspace, "workspace"));

		return builder.ToString();
	}

	private static string FormatClass(Class type, string? alias = null)
	{
		StringBuilder builder = new();

		string name = alias ?? type.Name;
		builder.Append(name);

		if (type.Name != Types.Any)
		{
			Class typeBase = GetBase(type);
			builder.Append($" from {typeBase.Name}");
		}

		builder.AppendLine(" {");

		Scope scope = GetScope(type);
		foreach (Variable variable in GetVariables(scope))
		{
			if (IsOperator(variable, out Operator? @operator))
			{
				AppendOperatorSignatures(builder, @operator, type.Name);
				continue;
			}
			if (variable.Tag is Types.Type or Types.Template) continue;
			builder.AppendLine($"\t{variable.Name} {variable.Tag};");
		}

		builder.Append('}');
		return builder.ToString();
	}

	private static void AppendOperatorSignatures(StringBuilder builder, Operator @operator, string tag)
	{
		HashSet<string> signatures = [];
		Scope scope = GetScope(@operator);
		foreach (Variable variable in GetVariables(scope))
		{
			if (!IsOperation(variable, out Operation? operation)) continue;
			string formatted = FormatOperation(@operator.Name, operation, tag);
			if (!signatures.Add(formatted)) continue;
			builder.AppendLine($"\t{formatted};");
		}
	}

	private static string FormatTemplate(Template template)
	{
		IEnumerable<string> generics = GetGenerics(template);
		List<Class> arguments = [];
		foreach (string generic in generics)
		{
			Class type = new(generic, new Scope(generic), null);
			arguments.Add(type);
		}
		string name = Mangler.Generics(template.Name, generics);
		Class assembled = template.Assemble(template.Name, arguments, ~Position.Zero);
		return FormatClass(assembled, name);
	}

	private static string FormatOperation(string name, Operation operation, string type)
	{
		List<string> parameters = operation.Parameters.ToList();
		if (type != Types.Workspace && parameters.Count > 0) parameters.RemoveAt(0);
		string signature = string.Join(", ", parameters.Select(FormatParameter));
		return $"{name}({signature}) {operation.Result}";
	}

	private static string FormatParameter(string type, int index)
	{
		return $"arg_{index + 1} {type}";
	}

	private static Scope GetScope(Container container)
	{
		return GetPropertyValue<Scope>(container, typeof(Container), "Location");
	}

	private static IEnumerable<Variable> GetVariables(Scope scope)
	{
		IDictionary variables = GetPropertyValue<IDictionary>(scope, typeof(Scope), "Variables");
		foreach (DictionaryEntry entry in variables)
		{
			if (entry.Value is not Variable variable) throw new InvalidOperationException($"Variable expected, got {entry.Value?.GetType().Name}.");
			yield return variable;
		}
	}

	private static Class GetBase(Class type)
	{
		return GetFieldValue<Class>(type, typeof(Class), "<base>P");
	}

	private static IEnumerable<string> GetGenerics(Template template)
	{
		return GetFieldValue<IEnumerable<string>>(template, typeof(Template), "<generics>P");
	}

	private static T GetPropertyValue<T>(object instance, Type type, string propertyName)
	{
		PropertyInfo property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException($"Property '{propertyName}' not found on {type.Name}.");
		return (T?) property.GetValue(instance) ?? throw new InvalidOperationException($"Property '{propertyName}' is null on {type.Name}.");
	}

	private static T GetFieldValue<T>(object instance, Type type, string fieldName)
	{
		FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {type.Name}.");
		return (T?) field.GetValue(instance) ?? throw new InvalidOperationException($"Field '{fieldName}' is null on {type.Name}.");
	}

	private static bool IsOperator(Variable variable, [NotNullWhen(true)] out Operator? @operator)
	{
		if (variable.Tag == Types.Function && variable.Value.Content is Operator content)
		{
			@operator = content;
			return true;
		}
		@operator = null;
		return false;
	}

	private static bool IsOperation(Variable variable, [NotNullWhen(true)] out Operation? operation)
	{
		if (variable.Tag == Types.Function && variable.Value.Content is Operation content)
		{
			operation = content;
			return true;
		}
		operation = null;
		return false;
	}
}
