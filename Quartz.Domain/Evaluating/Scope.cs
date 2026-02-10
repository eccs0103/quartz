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

		if (ParseGeneric(name, out string? templateName, out string[]? arguments))
		{
			if (TryRead(templateName, out Symbol? templateSymbol) && templateSymbol is Template template)
			{
				Class[] classes = new Class[arguments.Length];
				for (int i = 0; i < arguments.Length; i++)
				{
					if (!TryRead(arguments[i], out Symbol? argSymbol) || argSymbol is not Class argClass)
					{
						symbol = null;
						return false;
					}
					classes[i] = argClass;
				}

				symbol = template.Instantiate(name, classes);
				Register(name, symbol, ~Position.Zero);
				return true;
			}
		}

		symbol = null;
		return false;
	}

	private static bool ParseGeneric(string input, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out string[]? arguments)
	{
		int bracketStart = input.IndexOf('<');
		int bracketEnd = input.LastIndexOf('>');

		if (bracketStart == -1 || bracketEnd == -1 || bracketEnd != input.Length - 1)
		{
			name = null;
			arguments = null;
			return false;
		}

		name = input[..bracketStart];
		string content = input.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
		
		List<string> args = [];
		int balance = 0;
		int last = 0;
		for (int i = 0; i < content.Length; i++)
		{
			if (content[i] == '<')
			{
				balance++;
			}
			else if (content[i] == '>')
			{
				balance--;
			}
			else if (content[i] == ',' && balance == 0)
			{
				args.Add(content[last..i].Trim());
				last = i + 1;
			}
		}
		args.Add(content[last..].Trim());

		arguments = [.. args];
		return true;
	}

	public Symbol Read(string name, Range<Position> range)
	{
		if (!TryRead(name, out Symbol? symbol)) throw new NotExistIssue($"Identifier '{name}' in {this}", range);
		return symbol;
	}

	public T? Find<T>(Predicate<T> predicate) where T : Symbol
	{
		return Symbols.Values.OfType<T>().FirstOrDefault(symbol => predicate(symbol));
	}
}
