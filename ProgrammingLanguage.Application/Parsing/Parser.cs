using System.Globalization;
using System.Text.Json;
using ProgrammingLanguage.Application.Lexing;
using ProgrammingLanguage.Shared.Exceptions;

namespace ProgrammingLanguage.Application.Parsing;

public class Parser
{
	private static readonly Dictionary<string, string> Brackets = new()
	{
		{  @"(", @")" },
	};
	private Node Degree1OperatorsParse(in Walker walker)
	{
		Node left = Degree2OperatorsParse(walker);
		while (walker.InRange)
		{
			Token token = walker.Token;
			if (!token.Match(Token.Types.Operator, ":")) break;
			walker.Index++;
			Node right = Degree2OperatorsParse(walker);
			left = new BinaryOperatorNode(token.Value, left, right, new(left.RangePosition.Begin, right.RangePosition.End));
		}
		return left;
	}
	private Node Degree2OperatorsParse(in Walker walker)
	{
		Node left = Degree3OperatorsParse(walker);
		while (walker.InRange)
		{
			Token token = walker.Token;
			if (!token.Match(Token.Types.Operator, "+", "-")) break;
			walker.Index++;
			Node right = Degree3OperatorsParse(walker);
			left = new BinaryOperatorNode(token.Value, left, right, new(left.RangePosition.Begin, right.RangePosition.End));
		}
		return left;
	}
	private Node Degree3OperatorsParse(in Walker walker)
	{
		Node left = VerticesParse(walker);
		while (walker.InRange)
		{
			Token token = walker.Token;
			if (!token.Match(Token.Types.Operator, "*", "/")) break;
			walker.Index++;
			Node right = VerticesParse(walker);
			left = new BinaryOperatorNode(token.Value, left, right, new(left.RangePosition.Begin, right.RangePosition.End));
		}
		return left;
	}
	private Node[] ArgumentsParse(in Walker walker)
	{
		List<Node> arguments = [];
		while (true)
		{
			arguments.Add(Degree1OperatorsParse(walker));

			if (!walker.InRange) break;
			Token token = walker.Token;
			if (!token.Match(Token.Types.Separator, ",")) throw new Issue($"Expected ','", token.RangePosition.Begin);
			walker.Index++;
		}
		return [.. arguments];
	}
	private static Walker GetSubwalker(in Walker walker, in string bracket)
	{
		if (!Brackets.TryGetValue(bracket, out string? pair)) throw new Issue($"Unable to get pair of {bracket}", walker.RangePosition.Begin);
		uint counter = 1;
		uint begin = walker.Index + 1;
		for (walker.Index++; walker.Index < walker.RangeIndex.End; walker.Index++)
		{
			if (!walker.InRange) continue;
			Token token = walker.Token;
			if (token.Match(Token.Types.Bracket, bracket)) counter++;
			else if (token.Match(Token.Types.Bracket, pair)) counter--;
			if (counter != 0) continue;
			uint end = walker.Index;
			walker.Index = begin;
			return walker.GetSubwalker(begin, end);
		}
		throw new Issue($"Expected '{pair}'", walker.RangePosition.End);
	}
	private Node VerticesParse(in Walker walker)
	{
		if (!walker.InRange) throw new Issue($"Expected expression", walker.RangePosition.Begin);
		Token token = walker.Token;
		switch (token.Type)
		{
		case Token.Types.Number:
		{
			ValueNode value = new(Convert.ToDouble(token.Value, CultureInfo.GetCultureInfo("en-US")), token.RangePosition);
			walker.Index++;
			return value;
		}
		case Token.Types.String:
		{
			ValueNode path = new(JsonSerializer.Deserialize<string>(token.Value) ?? throw new Issue($"Unable to parse string", token.RangePosition.Begin), token.RangePosition);
			walker.Index++;
			return path;
		}
		case Token.Types.Identifier:
		{
			IdentifierNode identifier = new(token.Value, token.RangePosition);
			walker.Index++;
			if (!walker.InRange) return identifier;
			Token subtoken = walker.Token;
			if (!subtoken.Match(Token.Types.Bracket, "(")) return identifier;
			Node[] arguments = ArgumentsParse(GetSubwalker(walker, subtoken.Value));
			walker.Index++;
			return new InvokationNode(identifier, arguments, new(identifier.RangePosition.Begin, arguments.LastOrDefault(identifier).RangePosition.End));
		}
		case Token.Types.Keyword:
		{
			switch (token.Value)
			{
			case "data":
			{
				walker.Index++;
				Node target = VerticesParse(walker);
				return new UnaryOperatorNode(token.Value, target, new(token.RangePosition.Begin, target.RangePosition.End));
			}
			case "null":
			{
				walker.Index++;
				return new ValueNode(null, token.RangePosition);
			}
			case "import":
			{
				walker.Index++;
				Node target = VerticesParse(walker);
				return new UnaryOperatorNode(token.Value, target, new(token.RangePosition.Begin, target.RangePosition.End));
			}
			default: throw new Issue($"Unidentified keyword '{token.Value}'", token.RangePosition.Begin);
			}
		}
		case Token.Types.Operator:
		{
			if (token.Match("+", "-"))
			{
				walker.Index++;
				Node target = VerticesParse(walker);
				return new UnaryOperatorNode(token.Value, target, new(token.RangePosition.Begin, target.RangePosition.End));
			}
			else throw new Issue($"Unidentified operator '{token.Value}'", token.RangePosition.Begin);
		}
		case Token.Types.Bracket:
		{
			Node node = Degree1OperatorsParse(GetSubwalker(walker, token.Value));
			walker.Index++;
			return node;
		}
		default: throw new Issue($"Unidentified token '{token.Value}'", token.RangePosition.Begin);
		}
	}
	public List<Node> Parse(in Token[] tokens)
	{
		List<Node> trees = [];
		Walker walker = new(tokens, new(0, Convert.ToUInt32(tokens.Length)));
		while (walker.Index < tokens.Length)
		{
			Node tree = Degree1OperatorsParse(walker);
			if (!walker.InRange || !walker.Token.Match(Token.Types.Separator, ";")) throw new Issue($"Expected ';'", walker.RangePosition.End);
			trees.Add(tree);
			walker.Index++;
		}
		return trees;
	}
}
