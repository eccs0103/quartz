using System.Globalization;
using System.Text.Json;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Lexing;
using static ProgrammingLanguage.Application.Lexing.Token;

namespace ProgrammingLanguage.Application.Parsing;

public class Parser
{
	private static readonly Dictionary<string, string> Brackets = new()
	{
		{ "(", ")" },
	};

	public List<Node> Parse(Token[] tokens)
	{
		List<Node> trees = [];
		Walker walker = new(tokens, new(0, Convert.ToUInt32(tokens.Length)));
		while (walker.InRange)
		{
			Node tree = StatementParse(walker);
			if (!walker.GetToken(out Token token) || !token.Represents(Types.Separator, ";")) throw new Issue("Expected ';'", walker.RangePosition.End);
			trees.Add(tree);
			walker.Index++;
		}
		return trees;
	}

	private Node StatementParse(Walker walker)
	{
		if (!walker.GetToken(out Token token)) throw new Issue("Expected statement", walker.RangePosition.Begin);
		if (token.Represents(Types.Keyword, "datum")) return DeclarationParse(walker);
		return ExpressionParse(walker);
	}

	private DeclarationNode DeclarationParse(Walker walker)
	{
		walker.GetToken(out Token token1);
		walker.Index++;

		if (!walker.GetToken(out Token token2)) throw new Issue("Expected identifier", token1.RangePosition.End);
		if (!token2.Represents(Types.Identifier)) throw new Issue("Expected identifier", token2.RangePosition.Begin);
		IdentifierNode identifier = new(token2.Value, token2.RangePosition);
		walker.Index++;

		if (!walker.GetToken(out Token token) || !token.Represents(Types.Operator, ":"))
		{
			ValueNode nullable = ValueNode.NullAt(token2.RangePosition);
			return new DeclarationNode(identifier, nullable, new(token1.RangePosition.Begin, identifier.RangePosition.End));
		}

		walker.Index++;
		Node value = ExpressionParse(walker);
		return new DeclarationNode(identifier, value, new(token1.RangePosition.Begin, value.RangePosition.End));
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
			walker.Index++;
			Node right = MultiplicativeParse(walker);
			left = new BinaryOperatorNode(token.Value, left, right, new(left.RangePosition.Begin, right.RangePosition.End));
		}
		return left;
	}

	private Node MultiplicativeParse(Walker walker)
	{
		Node left = UnaryParse(walker);
		while (walker.GetToken(out Token token))
		{
			if (!token.Represents(Types.Operator, "*", "/")) break;
			walker.Index++;
			Node right = UnaryParse(walker);
			left = new BinaryOperatorNode(token.Value, left, right, new(left.RangePosition.Begin, right.RangePosition.End));
		}
		return left;
	}

	private Node UnaryParse(Walker walker)
	{
		if (!walker.GetToken(out Token token)) throw new Issue("Expected expression", walker.RangePosition.Begin);
		if (token.Represents(Types.Operator, "+", "-"))
		{
			walker.Index++;
			Node target = UnaryParse(walker);
			return new UnaryOperatorNode(token.Value, target, new(token.RangePosition.Begin, target.RangePosition.End));
		}
		return PrimaryParse(walker);
	}

	private Node PrimaryParse(Walker walker)
	{
		if (!walker.GetToken(out Token token)) throw new Issue("Expected expression", walker.RangePosition.Begin);
		switch (token.Type)
		{
		case Types.Number:
		{
			ValueNode value = new(Convert.ToDouble(token.Value, CultureInfo.GetCultureInfo("en-US")), token.RangePosition);
			walker.Index++;
			return value;
		}
		case Types.String:
		{
			ValueNode @string = new(JsonSerializer.Deserialize<string>(token.Value) ?? throw new Issue("Unable to parse string", token.RangePosition.Begin), token.RangePosition);
			walker.Index++;
			return @string;
		}
		case Types.Identifier:
		{
			IdentifierNode identifier = new(token.Value, token.RangePosition);
			walker.Index++;
			if (!walker.GetToken(out Token subtoken) || !subtoken.Represents(Types.Bracket, "(")) return identifier;
			string open = subtoken.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new Issue($"Unmatched bracket {open}", walker.RangePosition.Begin);
			IEnumerable<Node> arguments = ArgumentsParse(walker.GetSubwalker(open, close));
			walker.Index++;
			return new InvokationNode(identifier, arguments, new(identifier.RangePosition.Begin, arguments.LastOrDefault(identifier).RangePosition.End));
		}
		case Types.Keyword:
		{
			if (token.Represents("null"))
			{
				walker.Index++;
				return ValueNode.NullAt(token.RangePosition);
			}
			if (token.Represents("true"))
			{
				walker.Index++;
				return new ValueNode(true, token.RangePosition);
			}
			if (token.Represents("false"))
			{
				walker.Index++;
				return new ValueNode(false, token.RangePosition);
			}
			throw new Issue($"Unexpected keyword '{token.Value}'", token.RangePosition.Begin);
		}
		case Types.Bracket:
		{
			if (token.Represents("("))
			{
				string open = token.Value;
				if (!Brackets.TryGetValue(open, out string? close)) throw new Issue($"Unmatched bracket {open}", walker.RangePosition.Begin);
				Node expression = ExpressionParse(walker.GetSubwalker(open, close));
				walker.Index++;
				return expression;
			}
			throw new Issue($"Unexpected bracket '{token.Value}'", token.RangePosition.Begin);
		}
		default: throw new Issue($"Unexpected token '{token.Value}'", token.RangePosition.Begin);
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
