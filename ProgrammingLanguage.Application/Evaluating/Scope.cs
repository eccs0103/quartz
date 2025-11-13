using System.Diagnostics.CodeAnalysis;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Scope
{
	private readonly Dictionary<string, Symbol> Symbols = [];
	public readonly string Name;
	private readonly Scope? Parent;
	private readonly string Path;

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
		return Path;
	}

	public Symbol Register(string name, Symbol symbol, Range<Position> range)
	{
		if (!Symbols.TryAdd(name, symbol)) throw new AlreadyExistsIssue($"Identifier '{name}' in {this}", range);
		return symbol;
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Symbol? symbol)
	{
		Scope? current = this;
		while (current != null)
		{
			if (current.Symbols.TryGetValue(name, out symbol)) return true;
			current = current.Parent;
		}
		symbol = null;
		return false;
	}

	public Symbol Read(string name, Range<Position> range)
	{
		if (!TryRead(name, out Symbol? symbol)) throw new NotExistIssue($"Identifier '{name}' in {this}", range);
		return symbol;
	}

	public void Write(string name, string tag, object value, Range<Position> range)
	{
		Symbol symbol = Read(name, range);
		if (symbol is not Datum datum) throw new NotMutableIssue($"Identifier '{name}' is not a variable in {this}", range);
		if (!datum.Mutable) throw new NotMutableIssue($"Identifier '{name}' is constant in {this}", range);
		if (datum.Tag != tag) throw new TypeMismatchIssue(tag, datum.Tag, range);
		datum.Value = value;
	}
}
