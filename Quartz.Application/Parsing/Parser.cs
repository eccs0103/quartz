using System.Globalization;
using System.Text.Json;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Lexing;
using Quartz.Domain.Parsing;
using static Quartz.Domain.Lexing.Token;

namespace Quartz.Application.Parsing;

public class Parser
{
	private static Dictionary<string, string> Brackets { get; } = new()
	{
		{ "(", ")" },
		{ "{", "}" },
		{ "<", ">" },
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
		}
	}

	private Node StatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token1.Represents(Types.Separator, ";"))
		{
			walker.Index++;
			return new BlockNode([], token1.RangePosition);
		}

		if (token1.Represents(Types.Keyword, "if")) return IfStatementParse(walker);

		if (token1.Represents(Types.Keyword, "while")) return WhileStatementParse(walker);
		// if (token1.Represents(Types.Keyword, "for")) return ForStatementParse(walker);

		if (token1.Represents(Types.Bracket, "{")) return BlockParse(walker);

		Node statement = SimpleStatementParse(walker);
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(";", ~walker.RangePosition.End);
		if (!token2.Represents(Types.Separator, ";")) throw new ExpectedIssue(";", token2.RangePosition);
		walker.Index++;
		return statement;
	}

	private Node SimpleStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token1.Represents(Types.Keyword, "break")) return BreakStatementParse(walker);
		if (token1.Represents(Types.Keyword, "continue")) return ContinueStatementParse(walker);

		if (!token1.Represents(Types.Identifier)) return ExpressionParse(walker);

		if (!walker.Peek(out Token? token2, 1)) return ExpressionParse(walker);

		if (token2.Represents(Types.Identifier)) return DeclarationParse(walker);

		if (token2.Represents(Types.Operator, ":")) return AssignmentParse(walker);

		return ExpressionParse(walker);
	}

	private Node WhileStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, "while")) throw new ExpectedIssue("while", walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, "(")) throw new ExpectedIssue("(", ~token1.RangePosition.End);
		Node condition = ExpressionParse(walker.GetSubwalker("(", ")"));
		walker.Index++;

		Node body = StatementParse(walker);
		return new WhileStatementNode(condition, body, token1.RangePosition >> body.RangePosition);
	}

	/* private Node ForStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, "for")) throw new ExpectedIssue("for", walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, "(")) throw new ExpectedIssue("(", ~token1.RangePosition.End);

		// Парсим сложную "шапку" цикла
		Walker subwalker = walker.GetSubwalker("(", ")");

		// 1. item (identifier)
		if (!subwalker.Peek(out Token? tokenItem) || !tokenItem.Represents(Types.Identifier)) throw new ExpectedIssue("variable identifier for for-loop", ~subwalker.RangePosition.Begin);
		IdentifierNode item = new(tokenItem.Value, tokenItem.RangePosition);
		subwalker.Index++;

		// 2. Type (identifier)
		if (!subwalker.Peek(out Token? tokenType) || !tokenType.Represents(Types.Identifier)) throw new ExpectedIssue("type identifier for for-loop", ~item.RangePosition.End);
		IdentifierNode type = new(tokenType.Value, tokenType.RangePosition);
		subwalker.Index++;

		// 3. in (keyword)
		if (!subwalker.Peek(out Token? tokenIn) || !tokenIn.Represents(Types.Keyword, "in")) throw new ExpectedIssue("in", ~type.RangePosition.End);
		subwalker.Index++;

		// 4. iterator (expression)
		Node iterator = ExpressionParse(subwalker);

		// Проверяем, что в скобках больше ничего нет
		if (subwalker.InRange) throw new UnexpectedIssue("extra tokens in for-loop header", subwalker.RangePosition);

		// Продвигаем главный walker
		walker.Index++;

		// 5. body (statement)
		Node body = StatementParse(walker);
		return new ForStatementNode(item, type, iterator, body, token1.RangePosition >> body.RangePosition);
	} */

	private Node BreakStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, "break")) throw new ExpectedIssue("break", walker.RangePosition);
		walker.Index++;
		return new BreakStatementNode(token.RangePosition);
	}

	private Node ContinueStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, "continue")) throw new ExpectedIssue("continue", walker.RangePosition);
		walker.Index++;
		return new ContinueStatementNode(token.RangePosition);
	}

	private IfStatementNode IfStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, "if")) throw new ExpectedIssue("if", walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, "(")) throw new ExpectedIssue("(", ~token1.RangePosition.End);
		Node condition = ExpressionParse(walker.GetSubwalker("(", ")"));
		walker.Index++;

		Node then = StatementParse(walker);
		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Keyword, "else")) return new IfStatementNode(condition, then, null, token1.RangePosition >> then.RangePosition);
		walker.Index++;

		Node @else = StatementParse(walker);
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

	private IdentifierNode TypeParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("type identifier", walker.RangePosition);
		if (!token1.Represents(Types.Identifier)) throw new ExpectedIssue("type identifier", walker.RangePosition);
		
		IdentifierNode type = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (walker.Peek(out Token? token2) && token2.Represents(Types.Bracket, "<"))
		{
			string open = token2.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
			Walker subwalker = walker.GetSubwalker(open, close);
			IEnumerable<IdentifierNode> generics = [.. GenericArgumentsParse(subwalker)];
			if (!walker.Peek(out Token? token3)) throw new ExpectedIssue(close, ~type.RangePosition.End);
			walker.Index++;
			type = new GenericNode(type, generics, type.RangePosition >> token3.RangePosition);
		}

		if (walker.Peek(out Token? token4) && token4.Represents(Types.Operator, "?"))
		{
			type = new GenericNode(new IdentifierNode("Nullable", type.RangePosition), [type], type.RangePosition >> token4.RangePosition);
			walker.Index++;
		}

		return type;
	}

	private IEnumerable<IdentifierNode> GenericArgumentsParse(Walker walker)
	{
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return TypeParse(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, ",")) break;
			walker.Index++;
		}
	}

	private DeclarationNode DeclarationParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for variable name", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		IdentifierNode type = TypeParse(walker);

		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Bracket, "("))
		{
			return new DeclarationNode(type, identifier, null, identifier.RangePosition >> type.RangePosition);
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
		return LogicalOrParse(walker);
	}

	private Node LogicalOrParse(Walker walker)
	{
		Node left = LogicalAndParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, "|"))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = LogicalAndParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node LogicalAndParse(Walker walker)
	{
		Node left = ComparisonParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, "&"))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = ComparisonParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ComparisonParse(Walker walker)
	{
		Node left = AdditiveParse(walker);
		while (walker.Peek(out Token? token) && (token.Represents(Types.Operator, "!=", "=", "<=", ">=") || token.Represents(Types.Bracket, "<", ">")))
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
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, "+", "-", "!")) return PrimaryParse(walker);
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
			if (token.Represents("null"))
			{
				walker.Index++;
				return new ValueNode("Null", null, token.RangePosition);
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
