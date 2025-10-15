using System.Diagnostics.CodeAnalysis;
using System.Text;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Scope(string name, Scope? parent = null)
{
	private readonly Dictionary<string, Property> Properties = [];
	public readonly string Name = name;
	private readonly Scope? Parent = parent;
	private readonly string Path = DeterminePath(parent, name);

	private static string DeterminePath(Scope? parent, string name)
	{
		if (parent == null) return name;
		return $"{parent.Path}.{name}";
	}

	public override string ToString()
	{
		return Path;
	}

	public Property Register(string name, Property property, Range<Position> range)
	{
		if (!Properties.TryAdd(name, property)) throw new AlreadyExistsIssue($"Identifier '{name}' in {this}", range);
		return property;
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Property? property)
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

	public Property Read(string name, Range<Position> range)
	{
		if (!TryRead(name, out Property? property)) throw new NotExistIssue($"Identifier '{name}' in {this}", range);
		return property;
	}

	public void Write(string name, string tag, object value, Range<Position> range)
	{
		Property property = Read(name, range);
		if (!property.Mutable) throw new NotMutableIssue($"Identifier '{name}' in {this}", range);
		if (property.Tag != tag) throw new TypeMismatchIssue(tag, property.Tag, range);
		property.Value = value;
	}
}
