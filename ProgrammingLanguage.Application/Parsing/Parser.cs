using System.Globalization;
using System.Text.Json;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Lexing;
using static ProgrammingLanguage.Application.Lexing.Token;

namespace ProgrammingLanguage.Application.Parsing;

internal class Parser
{
	private static readonly Dictionary<string, string> Brackets = new()
	{
		{ "(", ")" },
	};

	public List<Node> Parse(Token[] tokens)
	{
		List<Node> trees = [];
		Walker walker = new(tokens);
		while (walker.InRange)
		{
			Node tree = StatementParse(walker);
			if (!walker.GetToken(out Token token)) throw new ExpectedIssue(";", ~walker.RangePosition.End);
			if (!token.Represents(Types.Separator, ";")) throw new ExpectedIssue(";", token.RangePosition);
			trees.Add(tree);
			walker.Index++;
		}
		return trees;
	}

	private Node StatementParse(Walker walker)
	{
		if (!walker.GetToken(out Token token)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);
		if (token.Represents(Types.Keyword, "datum")) return DeclarationParse(walker);
		if (!token.Represents(Types.Identifier)) return ExpressionParse(walker);
		walker.Index++;
		bool isNotColon = !walker.GetToken(out Token next) || !next.Represents(Types.Operator, ":");
		walker.Index--;
		if (isNotColon) return ExpressionParse(walker);
		return AssignmentParse(walker);
	}

	private DeclarationNode DeclarationParse(Walker walker)
	{
		walker.GetToken(out Token token1);
		walker.Index++;

		if (!walker.GetToken(out Token token2)) throw new ExpectedIssue("identifier", ~token1.RangePosition.End);
		if (!token2.Represents(Types.Identifier)) throw new ExpectedIssue("identifier", token2.RangePosition);
		IdentifierNode identifier = new(token2.Value, token2.RangePosition);
		walker.Index++;

		if (!walker.GetToken(out Token token3) || !token3.Represents(Types.Operator, ":"))
		{
			ValueNode nullable = ValueNode.NullableAt("Number", token2.RangePosition);
			return new DeclarationNode(identifier, nullable, token1.RangePosition >> identifier.RangePosition);
		}

		walker.Index++;
		Node value = ExpressionParse(walker);

		return new DeclarationNode(identifier, value, token1.RangePosition >> value.RangePosition);
	}

	private AssignmentNode AssignmentParse(Walker walker)
	{
		if (!walker.GetToken(out Token token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (!walker.GetToken(out Token token2) || !token2.Represents(Types.Operator, ":")) throw new ExpectedIssue(":", ~token1.RangePosition.End);

		walker.Index++;
		Node value = ExpressionParse(walker);

		return new AssignmentNode(identifier, value, identifier.RangePosition >> value.RangePosition);
	}

	private Node ExpressionParse(Walker walker)
	{
		return AdditiveParse(walker);
	}

	private Node AdditiveParse(Walker walker)
	{
		Node left = MultiplicativeParse(walker);
		while (walker.GetToken(out Token token))
		{
			if (!token.Represents(Types.Operator, "+", "-")) break;
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = MultiplicativeParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node MultiplicativeParse(Walker walker)
	{
		Node left = UnaryParse(walker);
		while (walker.GetToken(out Token token))
		{
			if (!token.Represents(Types.Operator, "*", "/")) break;
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = UnaryParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node UnaryParse(Walker walker)
	{
		if (!walker.GetToken(out Token token) || !token.Represents(Types.Operator, "+", "-")) return PrimaryParse(walker);
		IdentifierNode @operator = new(token.Value, token.RangePosition);
		walker.Index++;
		Node target = PrimaryParse(walker);
		return new UnaryOperatorNode(@operator, target, token.RangePosition >> target.RangePosition);
	}

	private Node PrimaryParse(Walker walker)
	{
		if (!walker.GetToken(out Token token)) throw new ExpectedIssue("expression", ~walker.RangePosition.Begin);
		switch (token.Type)
		{
		case Types.Number:
		{
			double value = Convert.ToDouble(token.Value, CultureInfo.GetCultureInfo("en-US"));
			ValueNode number = new("Number", value, token.RangePosition);
			walker.Index++;
			return number;
		}
		case Types.String:
		{
			string value = JsonSerializer.Deserialize<string>(token.Value)!;
			ValueNode @string = new("String", value, token.RangePosition);
			walker.Index++;
			return @string;
		}
		case Types.Identifier:
		{
			IdentifierNode identifier = new(token.Value, token.RangePosition);
			walker.Index++;
			if (!walker.GetToken(out Token subtoken) || !subtoken.Represents(Types.Bracket, "(")) return identifier;
			string open = subtoken.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, subtoken.RangePosition);

			IEnumerable<Node> arguments = ArgumentsParse(walker.GetSubwalker(open, close));
			walker.Index++;
			return new InvokationNode(identifier, arguments, identifier.RangePosition >> arguments.LastOrDefault(identifier).RangePosition);
		}
		case Types.Keyword:
		{
			if (token.Represents("null"))
			{
				walker.Index++;
				return ValueNode.NullableAt("Number", token.RangePosition);
			}
			if (token.Represents("true"))
			{
				walker.Index++;
				return new ValueNode("Boolean", true, token.RangePosition);
			}
			if (token.Represents("false"))
			{
				walker.Index++;
				return new ValueNode("Boolean", false, token.RangePosition);
			}
			throw new UnexpectedIssue($"keyword '{token.Value}'", token.RangePosition);
		}
		case Types.Bracket:
		{
			if (token.Represents("("))
			{
				string open = token.Value;
				if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
				Node expression = ExpressionParse(walker.GetSubwalker(open, close));
				walker.Index++;
				return expression;
			}
			throw new UnexpectedIssue($"bracket '{token.Value}'", token.RangePosition);
		}
		default: throw new UnexpectedIssue($"token '{token.Value}'", token.RangePosition);
		}
	}

	private IEnumerable<Node> ArgumentsParse(Walker walker)
	{
		List<Node> arguments = [];
		if (!walker.InRange) return arguments.AsEnumerable();
		while (true)
		{
			arguments.Add(ExpressionParse(walker));
			if (!walker.GetToken(out Token token) || !token.Represents(Types.Separator, ",")) break;
			walker.Index++;
		}
		return arguments.AsEnumerable();
	}
}
