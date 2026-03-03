using System.Globalization;
using System.Text.RegularExpressions;
using Quartz.Domain;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Exceptions.Parsing;
using Quartz.Domain.Lexing;
using Quartz.Domain.Parsing;
using Quartz.Shared;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Lexing.Token;

namespace Quartz.Application.Parsing;

public class Parser
{
	private static Dictionary<string, string> Brackets { get; } = new()
	{
		{ Definitions.Brackets.OpenParen, Definitions.Brackets.CloseParen },
		{ Definitions.Brackets.OpenBrace, Definitions.Brackets.CloseBrace },
		{ Definitions.Brackets.OpenAngle, Definitions.Brackets.CloseAngle },
		{ Definitions.Brackets.OpenBracket, Definitions.Brackets.CloseBracket },
	};

	public List<Node> Parse(Token[] tokens)
	{
		Walker walker = new(tokens);
		return [.. ProgramParse(walker)];
	}

	private IEnumerable<Node> ProgramParse(Walker walker)
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

		if (token1.Represents(Types.Separator, Definitions.Separators.Semicolon))
		{
			walker.Index++;
			return new BlockNode([], token1.RangePosition);
		}

		if (token1.Represents(Types.Keyword, Definitions.Keywords.If)) return IfStatementParse(walker);

		if (token1.Represents(Types.Keyword, Definitions.Keywords.While)) return WhileStatementParse(walker);
		if (token1.Represents(Types.Keyword, Definitions.Keywords.For)) return ForStatementParse(walker);

		if (token1.Represents(Types.Bracket, Definitions.Brackets.OpenBrace)) return BlockParse(walker);

		Node statement = SimpleStatementParse(walker);
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(Definitions.Separators.Semicolon, ~walker.RangePosition.End);
		if (!token2.Represents(Types.Separator, Definitions.Separators.Semicolon)) throw new ExpectedIssue(Definitions.Separators.Semicolon, token2.RangePosition);
		walker.Index++;
		return statement;
	}

	private Node SimpleStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token1.Represents(Types.Keyword, Definitions.Keywords.Break)) return BreakStatementParse(walker);
		if (token1.Represents(Types.Keyword, Definitions.Keywords.Continue)) return ContinueStatementParse(walker);

		if (token1.Represents(Types.Identifier) && walker.Peek(out Token? token2, 1) && token2.Represents(Types.Identifier)) return DeclarationParse(walker);

		if (walker.Attempt(() => AssignmentParse(walker), out AssignmentNode? assignment)) return assignment;

		return ExpressionParse(walker);
	}

	private AssignmentNode AssignmentParse(Walker walker)
	{
		Node target = PrimaryParse(walker);
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, Definitions.Operators.Colon)) throw new ExpectedIssue(Definitions.Operators.Colon, ~target.RangePosition.End);
		walker.Index++;
		Node value = ExpressionParse(walker);
		return new AssignmentNode(target, value, target.RangePosition >> value.RangePosition);
	}

	private WhileStatementNode WhileStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, Definitions.Keywords.While)) throw new ExpectedIssue(Definitions.Keywords.While, walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) throw new ExpectedIssue(Definitions.Brackets.OpenParen, ~token1.RangePosition.End);
		Node condition = ExpressionParse(walker.GetSubwalker(Definitions.Brackets.OpenParen, Definitions.Brackets.CloseParen));
		walker.Index++;

		Node body = StatementParse(walker);
		return new WhileStatementNode(condition, body, token1.RangePosition >> body.RangePosition);
	}

	private ForStatementNode ForStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, Definitions.Keywords.For)) throw new ExpectedIssue(Definitions.Keywords.For, walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) throw new ExpectedIssue(Definitions.Brackets.OpenParen, ~token1.RangePosition.End);
		string open = token2.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
		Walker subwalker = walker.GetSubwalker(open, close);
		if (!subwalker.Peek(out Token? token3) || !token3.Represents(Types.Identifier)) throw new ExpectedIssue("variable for for-loop", ~subwalker.RangePosition.Begin);
		IdentifierNode item = new(token3.Value, token3.RangePosition);
		subwalker.Index++;

		IdentifierNode type = TypeParse(subwalker);
		if (!subwalker.Peek(out Token? token4) || !token4.Represents(Types.Keyword, Definitions.Keywords.In)) throw new ExpectedIssue(Definitions.Keywords.In, ~type.RangePosition.End);
		subwalker.Index++;

		Node iterator = ExpressionParse(subwalker);
		// if (subwalker.InRange) throw new UnexpectedIssue("extra tokens in for-loop header", subwalker.RangePosition);
		walker.Index++;

		Node body = StatementParse(walker);
		return new ForStatementNode(item, type, iterator, body, token1.RangePosition >> body.RangePosition);
	}

	private BreakStatementNode BreakStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, Definitions.Keywords.Break)) throw new ExpectedIssue(Definitions.Keywords.Break, walker.RangePosition);
		walker.Index++;
		return new BreakStatementNode(token.RangePosition);
	}

	private ContinueStatementNode ContinueStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, Definitions.Keywords.Continue)) throw new ExpectedIssue(Definitions.Keywords.Continue, walker.RangePosition);
		walker.Index++;
		return new ContinueStatementNode(token.RangePosition);
	}

	private IfStatementNode IfStatementParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, Definitions.Keywords.If)) throw new ExpectedIssue(Definitions.Keywords.If, walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) throw new ExpectedIssue(Definitions.Brackets.OpenParen, ~token1.RangePosition.End);
		Node condition = ExpressionParse(walker.GetSubwalker(Definitions.Brackets.OpenParen, Definitions.Brackets.CloseParen));
		walker.Index++;

		Node then = StatementParse(walker);
		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Keyword, Definitions.Keywords.Else)) return new IfStatementNode(condition, then, null, token1.RangePosition >> then.RangePosition);
		walker.Index++;

		Node @else = StatementParse(walker);
		return new IfStatementNode(condition, then, @else, token1.RangePosition >> @else.RangePosition);
	}

	private BlockNode BlockParse(Walker walker)
	{
		const string open = Definitions.Brackets.OpenBrace;
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, open)) throw new ExpectedIssue(open, walker.RangePosition);
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
		Walker subwalker = walker.GetSubwalker(open, close);
		IEnumerable<Node> statements = [.. ProgramParse(subwalker)];
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(close, ~walker.RangePosition.End);
		walker.Index++;
		return new BlockNode(statements, token1.RangePosition >> token2.RangePosition);
	}

	private DeclarationNode DeclarationParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for variable name", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		IdentifierNode type = TypeParse(walker);
		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) return new DeclarationNode(type, identifier, null, identifier.RangePosition >> type.RangePosition);

		string open = token2.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
		Node value = ExpressionParse(walker.GetSubwalker(open, close));
		if (!walker.Peek(out Token? token3)) throw new ExpectedIssue(close, ~type.RangePosition.End);
		walker.Index++;
		return new DeclarationNode(type, identifier, value, identifier.RangePosition >> token3.RangePosition);
	}

	private IdentifierNode TypeParse(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for type", walker.RangePosition);
		IdentifierNode type = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (walker.Peek(out Token? token2) && token2.Represents(Types.Operator, Definitions.Brackets.OpenAngle))
		{
			string open = token2.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
			walker.Index++;
			IEnumerable<IdentifierNode> generics = [.. GenericsParse(walker)];
			if (!walker.Peek(out Token? token3)) throw new ExpectedIssue(close, ~type.RangePosition.End);
			walker.Index++;
			type = new GenericNode(type, generics, type.RangePosition >> token3.RangePosition);
		}

		if (walker.Peek(out Token? token4) && token4.Represents(Types.Operator, Definitions.Operators.Question))
		{
			type = new GenericNode(new IdentifierNode(Definitions.Types.Nullable, type.RangePosition), [type], type.RangePosition >> token4.RangePosition);
			walker.Index++;
		}

		return type;
	}

	private GenericNode? TemplateParse(Walker walker, IdentifierNode identifier)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, Definitions.Brackets.OpenAngle)) return null;
		if (walker.Attempt(() =>
		{
			walker.Index++;
			IEnumerable<IdentifierNode> generics = [.. GenericsParse(walker)];
			if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Operator, Definitions.Brackets.CloseAngle)) throw new ExpectedIssue(Definitions.Brackets.CloseAngle, ~walker.RangePosition.End);
			walker.Index++;
			return new GenericNode(identifier, generics, identifier.RangePosition >> token2.RangePosition);
		}, out GenericNode? generic)) return generic;
		return null;
	}

	private IEnumerable<IdentifierNode> GenericsParse(Walker walker)
	{
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return TypeParse(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, Definitions.Separators.Comma)) break;
			walker.Index++;
		}
	}

	private Node ExpressionParse(Walker walker)
	{
		return DisjunctionParse(walker);
	}

	private Node DisjunctionParse(Walker walker)
	{
		Node left = ConjunctionParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Or))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = ConjunctionParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ConjunctionParse(Walker walker)
	{
		Node left = RelationParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.And))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = RelationParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node RelationParse(Walker walker)
	{
		Node left = AdditiveParse(walker);
		while (walker.Peek(out Token? token) && (token.Represents(Types.Operator, Definitions.Operators.NotEqual, Definitions.Operators.Equal, Definitions.Operators.LessOrEqual, Definitions.Operators.GreaterOrEqual, Definitions.Brackets.OpenAngle, Definitions.Brackets.CloseAngle)))
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
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Plus, Definitions.Operators.Minus))
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
		Node left = PrefixParse(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Multiply, Definitions.Operators.Divide))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;
			Node right = PrefixParse(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node PrefixParse(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, Definitions.Operators.Plus, Definitions.Operators.Minus, Definitions.Operators.Not)) return PrimaryParse(walker);
		IdentifierNode @operator = new(token.Value, token.RangePosition);
		walker.Index++;
		Node target = PrimaryParse(walker);
		return new UnaryOperatorNode(@operator, target, token.RangePosition >> target.RangePosition);
	}

	private Node AtomicParse(Walker walker)
	{
		if (!walker.Peek(out Token? token)) throw new ExpectedIssue("expression", ~walker.RangePosition.Begin);
		switch (token.Type)
		{
		case Types.Number:
		{
			double value = double.Parse(token.Value, CultureInfo.InvariantCulture);
			ValueNode number = new(Definitions.Types.Number, value, token.RangePosition);
			walker.Index++;
			return number;
		}
		case Types.String:
		{
			string value = Regex.Unescape(token.Value[1..^1]);
			ValueNode @string = new(Definitions.Types.String, value, token.RangePosition);
			walker.Index++;
			return @string;
		}
		case Types.Character:
		{
			char value = Regex.Unescape(token.Value[1..^1]).Single();
			ValueNode character = new(Definitions.Types.Character, value, token.RangePosition);
			walker.Index++;
			return character;
		}
		case Types.Identifier: return DesignatorParse(walker);
		case Types.Keyword:
		{
			if (token.Represents(Definitions.Keywords.True, Definitions.Keywords.False))
			{
				walker.Index++;
				return new ValueNode(Definitions.Types.Boolean, bool.Parse(token.Value), token.RangePosition);
			}
			if (token.Represents(Definitions.Keywords.Null))
			{
				walker.Index++;
				return new ValueNode(Definitions.Types.Null, null, token.RangePosition);
			}
			throw new UnexpectedIssue(token.Value, token.RangePosition);
		}
		case Types.Bracket:
		{
			if (token.Represents(Definitions.Brackets.OpenParen))
			{
				string open = token.Value;
				if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
				Node expression = ExpressionParse(walker.GetSubwalker(open, close));
				walker.Index++;
				return expression;
			}
			if (token.Represents(Definitions.Brackets.OpenBracket))
			{
				string open = token.Value;
				if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
				IEnumerable<Node> elements = [.. ArgumentsParse(walker.GetSubwalker(open, close))];
				if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~token.RangePosition.End);
				walker.Index++;
				return new ArrayNode(elements, token.RangePosition >> closeToken.RangePosition);
			}
			throw new UnexpectedIssue(token.Value, token.RangePosition);
		}
		default: throw new UnexpectedIssue(token.Value, token.RangePosition);
		}
	}

	private Node PrimaryParse(Walker walker)
	{
		Node node = AtomicParse(walker);
		while (walker.Peek(out Token? token) && (token.Represents(Types.Operator, Definitions.Operators.Dot) || token.Represents(Types.Bracket, Definitions.Brackets.OpenParen) || token.Represents(Types.Bracket, Definitions.Brackets.OpenBracket)))
			node = PostfixParse(walker, node);
		return node;
	}

	private Node PostfixParse(Walker walker, Node node)
	{
		if (!walker.Peek(out Token? token)) return node;
		if (token.Represents(Types.Operator, Definitions.Operators.Dot))
		{
			walker.Index++;
			if (!walker.Peek(out Token? idToken) || !idToken.Represents(Types.Identifier)) throw new ExpectedIssue("member name", ~token.RangePosition.End);
			IdentifierNode member = new(idToken.Value, idToken.RangePosition);
			walker.Index++;
			return new FieldNode(node, member, node.RangePosition >> member.RangePosition);
		}
		if (token.Represents(Types.Bracket, Definitions.Brackets.OpenParen))
		{
			const string open = Definitions.Brackets.OpenParen;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
			IEnumerable<Node> arguments = [.. ArgumentsParse(walker.GetSubwalker(open, close))];
			if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~node.RangePosition.End);
			walker.Index++;
			return new InvocationNode(node, arguments, node.RangePosition >> closeToken.RangePosition);
		}
		if (token.Represents(Types.Bracket, Definitions.Brackets.OpenBracket))
		{
			const string open = Definitions.Brackets.OpenBracket;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
			Node index = ExpressionParse(walker.GetSubwalker(open, close));
			if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~node.RangePosition.End);
			walker.Index++;
			return new IndexNode(node, index, node.RangePosition >> closeToken.RangePosition);
		}
		return node;
	}

	private Node DesignatorParse(Walker walker)
	{
		if (!walker.Peek(out Token? token)) throw new ExpectedIssue("identifier expression", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token.Value, token.RangePosition);
		walker.Index++;

		GenericNode? generic = TemplateParse(walker, identifier);
		if (generic != null) return generic;

		return InvocationParse(walker, identifier);
	}

	private Node InvocationParse(Walker walker, IdentifierNode identifier)
	{
		const string open = Definitions.Brackets.OpenParen;
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, open)) return identifier;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
		IEnumerable<Node> arguments = [.. ArgumentsParse(walker.GetSubwalker(open, close))];
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(close, ~identifier.RangePosition.End);
		walker.Index++;
		return new InvocationNode(identifier, arguments, identifier.RangePosition >> token2.RangePosition);
	}

	private IEnumerable<Node> ArgumentsParse(Walker walker)
	{
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return ExpressionParse(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, Definitions.Separators.Comma)) break;
			walker.Index++;
		}
	}
}
