using System.Diagnostics.CodeAnalysis;
using System.Text;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Scope(string name, Scope? parent = null)
{
	private readonly Dictionary<string, Property> Properties = [];
	public readonly string Name = name;
	public readonly Scope? Parent = parent;

	public override string ToString()
	{
		StringBuilder builder = new();
		Scope? current = this;
		while (true)
		{
			builder.Insert(0, current.Name);
			current = current.Parent;
			if (current == null) break;
			builder.Insert(0, ".");
		}
		return builder.ToString();
	}

	public Property Register(string name, Property property, Range<Position> range)
	{
		if (!Properties.TryAdd(name, property)) throw new AlreadyExistsIssue($"Identifier '{name}' in {this}", range);
		return property;
	}

	public bool TryResolve(string name, [NotNullWhen(true)] out Property? property)
	{
		Scope? current = this;
		while (current != null)
		{
			if (current.Properties.TryGetValue(name, out property)) return true;
			current = current.Parent;
		}
		property = null;
		return false;
	}

	public Property Resolve(string name, Range<Position> range)
	{
		if (!TryResolve(name, out Property? property)) throw new NotExistIssue($"Identifier '{name}' in {this}", range);
		return property;
	}

	public void Write(string name, string tag, object value, Range<Position> range)
	{
		Property property = Resolve(name, range);
		if (!property.Mutable) throw new NotMutableIssue($"Identifier '{name}' in {this}", range);
		if (property.Tag != tag) throw new TypeMismatchIssue(tag, property.Tag, range);
		property.Value = value;
	}
}
