using System.Runtime.CompilerServices;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Structure(string name, Type equivalent, Scope location) : Property(name, "Type", equivalent)
{
	public Type Equivalent => Unsafe.As<Type>(Value);
	private readonly Scope Scope = new(name, location);

	public Property RegisterConstant(string name, string tag, object value, Range<Position> range)
	{
		Property constant = new(name, tag, value);
		return Scope.Register(name, constant, range);
	}

	public Property RegisterVariable(string name, string tag, object value, Range<Position> range)
	{
		Property variable = new(name, tag, value, MutableOptions);
		return Scope.Register(name, variable, range);
	}

	public Property ReadProperty(string name, Range<Position> range)
	{
		return Scope.Resolve(name, range);
	}

	public void WriteProperty(string name, string tag, object value, Range<Position> range)
	{
		Scope.Write(name, tag, value, range);
	}

	public Operation RegisterOperation(string name, IEnumerable<string> parameters, string result, OperationContent function, Range<Position> range)
	{
		if (!Scope.TryResolve(name, out Property? property))
		{
			property = new OverloadSet(name, Scope);
			Scope.Register(name, property, range);
		}
		if (property is not OverloadSet set) throw new AlreadyExistsIssue($"Identifier '{name}' in {Scope}", range);
		return set.RegisterOperation(parameters, result, function, range);
	}

	public Operation ReadOperation(string name, IEnumerable<string> parameters, Range<Position> range)
	{
		Property property = Scope.Resolve(name, range);
		if (property is not OverloadSet set) throw new NotExistIssue($"Overload set '{name}' in {Scope}", range);
		return set.ReadOperation(parameters, range);
	}
}
