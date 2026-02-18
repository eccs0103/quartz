using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

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

	public bool TryRegister(string name, Value value, bool mutable = false)
	{
		return TryRegister(name, value.Tag, value, mutable);
	}

	public bool TryRegister(string name, string typeTag, Value value, bool mutable = false)
	{
		return Variables.TryAdd(name, new Variable(name, typeTag, value, mutable));
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Variable? variable)
	{
		if (Variables.TryGetValue(name, out Variable? value))
		{
			variable = value;
			return true;
		}
		if (Parent != null) return Parent.TryRead(name, out variable);
		variable = null;
		return false;
	}

	public bool TryRead<T>(string name, [NotNullWhen(true)] out T? content)
		where T : notnull
	{
		if (TryRead(name, out Variable? variable) && variable.Value is Value<T> typed)
		{
			content = typed.Content;
			return true;
		}
		content = default;
		return false;
	}

	public Variable Read(string name, Range<Position> range)
	{
		if (TryRead(name, out Variable? variable)) return variable;
		throw new NotExistIssue($"Variable '{name}' in {this}", range);
	}

	public T Read<T>(string name, Range<Position> range)
		where T : notnull
	{
		if (TryRead(name, out T? content)) return content;
		throw new NotExistIssue($"{typeof(T).Name} '{name}' in {this}", range);
	}

	public IEnumerable<T> Scan<T>()
		where T : notnull
	{
		foreach (Variable variable in Variables.Values)
		{
			if (variable.Value is Value<T> typed) yield return typed.Content;
		}
		if (Parent != null)
		{
			foreach (T result in Parent.Scan<T>()) yield return result;
		}
	}
}
