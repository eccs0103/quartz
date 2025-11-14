using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class Class(string name, Scope location) : Symbol(name)
{
	public override void Assign(ValueNode value, Range<Position> range)
	{
		throw new NotMutableIssue($"Class '{Name}'", range);
	}

	public Datum RegisterConstant(string name, string tag, object value, Range<Position> range)
	{
		Datum constant = new(name, tag, value, false);
		location.Register(name, constant, range);
		return constant;
	}

	public Datum RegisterVariable(string name, string tag, object value, Range<Position> range)
	{
		Datum variable = new(name, tag, value, true);
		location.Register(name, variable, range);
		return variable;
	}

	public Datum ReadProperty(string name, Range<Position> range)
	{
		Symbol symbol = location.Read(name, range);
		if (symbol is not Datum datum) throw new NotExistIssue($"Datum '{name}' in {location}", range);
		return datum;
	}

	public Operator RegisterOperator(string name, Range<Position> range)
	{
		Operator @operator = new(name, location);
		location.Register(name, @operator, range);
		return @operator;
	}

	public Operator ReadOperator(string name, Range<Position> range)
	{
		Symbol symbol = location.Read(name, range);
		if (symbol is not Operator @operator) throw new NotExistIssue($"Operator '{name}' in {location}", range);
		return @operator;
	}
}
