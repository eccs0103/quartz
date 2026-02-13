using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Scope
{
	private Dictionary<string, Symbol> Symbols { get; } = [];
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

	public bool TryRegister(string name, Symbol symbol)
	{
		return Symbols.TryAdd(name, symbol);
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Symbol? symbol)
	{
		if (Symbols.TryGetValue(name, out symbol)) return true;
		if (Parent != null) return Parent.TryRead(name, out symbol);
		symbol = default;
		return false;
	}

	public Symbol Read(string name, Range<Position> range)
	{
		if (!TryRead(name, out Symbol? symbol)) throw new NotExistIssue($"Identifier '{name}' in {this}", range);
		return symbol;
	}

	// TODO: Fix deepness
	public T? Find<T>(Predicate<T> predicate) where T : Symbol
	{
		return Symbols.Values.OfType<T>().FirstOrDefault(predicate.Invoke);
	}
}
