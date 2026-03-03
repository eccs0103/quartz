using System.Diagnostics.CodeAnalysis;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public class Scope
{
	private Dictionary<string, Variable> Variables { get; } = [];
	public string Name { get; }
	private Scope? Parent { get; }
	private string Path { get; }

	private Scope(string name, Scope? parent)
	{
		Name = name;
		Parent = parent;
		Path = DeterminePath(parent, name);
	}

	public Scope(string name) : this(name, null)
	{
	}

	public Scope GetSubscope(string name)
	{
		return new Scope(name, this);
	}

	private static string DeterminePath(Scope? parent, string name)
	{
		if (parent == null) return name;
		return $"{parent.Path}.{name}";
	}

	public override string ToString()
	{
		return $"<{Path}>";
	}

	public bool TryRegister(string name, string tag, Value value, bool mutable = false)
	{
		return Variables.TryAdd(name, new Variable(name, tag, value, mutable));
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Variable? variable, bool deep = true)
	{
		if (Variables.TryGetValue(name, out variable)) return true;
		if (deep && Parent != null && Parent.TryRead(name, out variable)) return true;
		if (Mangler.IsGeneric(name, out string? template, out IEnumerable<string>? parameters) && TryRead(template, out Template? definition, deep))
		{
			List<Class> arguments = [];
			foreach (string parameter in parameters)
			{
				if (!TryRead(parameter, out Class? generic, deep))
				{
					variable = null;
					return false;
				}
				arguments.Add(generic);
			}
			Class type = definition.Assemble(name, arguments, ~Position.Zero);
			if (TryRegister(name, Types.Type, new Value<Class>(Types.Type, type)) && TryRead(name, out variable, false)) return true;
			variable = null;
			return false;
		}
		variable = null;
		return false;
	}

	public bool TryRead<T>(string name, [NotNullWhen(true)] out T? content, bool deep = true)
		where T : notnull
	{
		if (TryRead(name, out Variable? variable, deep) && variable.Value is Value<T> typed)
		{
			content = typed.Content;
			return true;
		}
		content = default;
		return false;
	}

	public IEnumerable<T> Scan<T>()
		where T : notnull
	{
		foreach (Variable variable in Variables.Values)
		{
			if (variable.Value is Value<T> typed) yield return typed.Content;
		}
		if (Parent == null) yield break;
		foreach (T result in Parent.Scan<T>())
		{
			yield return result;
		}
	}
}
