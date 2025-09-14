using ProgrammingLanguage.Application.Parsing;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Evaluator
{
	private readonly Registry Memory = new();
	private readonly ValueResolver Valuator;
	private readonly IdentifierResolver Nominator;

	public Evaluator()
	{
		Valuator = new(Memory);
		Nominator = new(Memory);

		Valuator.Nominator = Nominator;
		Nominator.Valuator = Valuator;

		Memory.TryDeclareType("Type", typeof(Type), out _);
		Memory.TryDeclareType("Number", typeof(double), out _);
		Memory.TryDeclareType("Boolean", typeof(bool), out _);
		Memory.TryDeclareType("String", typeof(string), out _);
		Memory.TryDeclareConstant("Number", "pi", PI, out _);
		Memory.TryDeclareConstant("Number", "e", E, out _);
	}

	public void Evaluate(IEnumerable<Node> trees)
	{
		foreach (Node tree in trees) tree.Accept(Valuator);
	}
}
