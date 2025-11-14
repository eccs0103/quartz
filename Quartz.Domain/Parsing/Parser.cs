using System.Globalization;
using System.Text.Json;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Lexing;
using static Quartz.Domain.Lexing.Token;

namespace Quartz.Domain.Parsing;

public class Parser
{
	private static Dictionary<string, string> Brackets { get; } = new()
	{
		{ "(", ")" },
		{ "{", "}" },
	};

	public List<Node> Parse(Token[] tokens)
	{
		Walker walker = new(tokens);
		return [.. StatementsParse(walker)];
	}

	private IEnumerable<Node> StatementsParse(Walker walker)
	{
		while (walker.InRange)
		{
			Node statement = StatementParse(walker);
			yield return statement;
			if (!walker.Peek(out Token? token)) throw new ExpectedIssue(";", ~walker.RangePosition.End);
			if (!token.Represents(Types.Separator, ";")) throw new ExpectedIssue(";", token.RangePosition);
			walker.Index++;
		}
	}

	private Node StatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token.Represents(Types.Keyword, "if")) return IfStatementParse(walker);

		if (token.Represents(Types.Bracket, "{")) return BlockParse(walker);

		if (!token.Represents(Types.Identifier)) return ExpressionParse(walker);

		if (!walker.Peek(out Token? token2, 1)) return ExpressionParse(walker);

		if (token2.Represents(Types.Identifier)) return DeclarationParse(walker);

		if (token2.Represents(Types.Operator, ":")) return AssignmentParse(walker);

		return ExpressionParse(walker);
	}

	private IfStatementNode IfStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, "if")) throw new ExpectedIssue("if", walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, "(")) throw new ExpectedIssue("(", ~token1.RangePosition.End);
		Node condition = ExpressionParse(walker.GetSubwalker("(", ")"));
		walker.Index++;

		BlockNode then = BlockParse(walker);
		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Keyword, "else")) return new IfStatementNode(condition, then, null, token1.RangePosition >> then.RangePosition);
		walker.Index++;

		Node @else;
		if (!walker.Peek(out Token? token4) || !token4.Represents(Types.Keyword, "if"))
		{
			@else = BlockParse(walker);
			return new IfStatementNode(condition, then, @else, token1.RangePosition >> @else.RangePosition);
		}

		@else = IfStatementParse(walker);
		return new IfStatementNode(condition, then, @else, token1.RangePosition >> @else.RangePosition);
	}

	private BlockNode BlockParse(Walker walker)
	{
		const string open = "{";
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, open)) throw new ExpectedIssue(open, walker.RangePosition);
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
		Walker subwalker = walker.GetSubwalker(open, close);
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(close, ~walker.RangePosition.End);
		IEnumerable<Node> statements = [.. StatementsParse(subwalker)];
		walker.Index++;
		return new BlockNode(statements, token1.RangePosition >> token2.RangePosition);
	}

	private DeclarationNode DeclarationParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for variable name", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Identifier)) throw new ExpectedIssue("type identifier", ~identifier.RangePosition.End);
		IdentifierNode type = new(token2.Value, token2.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Bracket, "("))
		{
			ValueNode nullable = new ValueNode(type.Name, null, type.RangePosition);
			return new DeclarationNode(type, identifier, nullable, identifier.RangePosition >> type.RangePosition);
		}

		string open = token3.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token3.RangePosition);
		Node value = ExpressionParse(walker.GetSubwalker(open, close));
		if (!walker.Peek(out Token? token4)) throw new ExpectedIssue(close, ~type.RangePosition.End);
		walker.Index++;
		return new DeclarationNode(type, identifier, value, identifier.RangePosition >> token4.RangePosition);
	}

	private AssignmentNode AssignmentParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Operator, ":")) throw new ExpectedIssue(":", ~token1.RangePosition.End);
		walker.Index++;

		Node value = ExpressionParse(walker);
		return new AssignmentNode(identifier, value, identifier.RangePosition >> value.RangePosition);
	}

	private Node ExpressionParse(Walker walker)
	{
		return ComparisonParse(walker);
	}

	private Node ComparisonParse(Walker walker)
	{
		Node left = AdditiveParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, "=", "<", ">", "<=", ">="))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = AdditiveParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node AdditiveParse(Walker walker)
	{
		Node left = MultiplicativeParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, "+", "-"))
		{
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
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, "*", "/"))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = UnaryParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node UnaryParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, "+", "-")) return PrimaryParse(walker);
		IdentifierNode @operator = new(token.Value, token.RangePosition);
		walker.Index++;
		Node target = PrimaryParse(walker);
		return new UnaryOperatorNode(@operator, target, token.RangePosition >> target.RangePosition);
	}

	private Node PrimaryParse(Walker walker)
	{
		if (!walker.Peek(out Token? token)) throw new ExpectedIssue("expression", ~walker.RangePosition.Begin);
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

			const string open = "(";
			if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, open)) return identifier;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
			IEnumerable<Node> arguments = [.. ArgumentsParse(walker.GetSubwalker(open, close))];
			if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, close)) throw new ExpectedIssue(close, ~identifier.RangePosition.End);
			walker.Index++;
			return new InvokationNode(identifier, arguments, identifier.RangePosition >> token2.RangePosition);
		}
		case Types.Keyword:
		{
			if (token.Represents("true", "false"))
			{
				walker.Index++;
				return new ValueNode("Boolean", bool.Parse(token.Value), token.RangePosition);
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
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return ExpressionParse(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, ",")) break;
			walker.Index++;
		}
	}
}
