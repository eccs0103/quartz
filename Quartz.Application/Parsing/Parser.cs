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
		return [.. ParseProgram(walker)];
	}

	private IEnumerable<Node> ParseProgram(Walker walker)
	{
		while (walker.InRange)
		{
			Node statement = ParseStatement(walker);
			yield return statement;
		}
	}

	private Node ParseStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token1.Represents(Types.Separator, Definitions.Separators.Semicolon))
		{
			walker.Index++;
			return new BlockNode([], token1.RangePosition);
		}

		if (token1.Represents(Types.Keyword, Definitions.Keywords.If)) return ParseIfStatement(walker);

		if (token1.Represents(Types.Keyword, Definitions.Keywords.While)) return ParseWhileStatement(walker);
		if (token1.Represents(Types.Keyword, Definitions.Keywords.For)) return ParseForStatement(walker);

		if (token1.Represents(Types.Bracket, Definitions.Brackets.OpenBrace)) return ParseBlock(walker);

		Node statement = ParseSimpleStatement(walker);
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(Definitions.Separators.Semicolon, ~walker.RangePosition.End);
		if (!token2.Represents(Types.Separator, Definitions.Separators.Semicolon)) throw new ExpectedIssue(Definitions.Separators.Semicolon, token2.RangePosition);
		walker.Index++;

		return statement;
	}

	private Node ParseSimpleStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token1)) throw new ExpectedIssue("statement", ~walker.RangePosition.Begin);

		if (token1.Represents(Types.Keyword, Definitions.Keywords.Break)) return ParseBreakStatement(walker);
		if (token1.Represents(Types.Keyword, Definitions.Keywords.Continue)) return ParseContinueStatement(walker);

		if (token1.Represents(Types.Identifier) && walker.Peek(out Token? token2, 1) && token2.Represents(Types.Identifier)) return ParseDeclaration(walker);

		if (walker.Attempt(() => ParseAssignment(walker), out AssignmentNode? assignment)) return assignment;

		return ParseExpression(walker);
	}

	private AssignmentNode ParseAssignment(Walker walker)
	{
		Node target = ParsePrimary(walker);
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, Definitions.Operators.Colon)) throw new ExpectedIssue(Definitions.Operators.Colon, ~target.RangePosition.End);
		walker.Index++;

		Node value = ParseExpression(walker);
		return new AssignmentNode(target, value, target.RangePosition >> value.RangePosition);
	}

	private WhileStatementNode ParseWhileStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, Definitions.Keywords.While)) throw new ExpectedIssue(Definitions.Keywords.While, walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) throw new ExpectedIssue(Definitions.Brackets.OpenParen, ~token1.RangePosition.End);
		string open = token2.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
		Node condition = ParseExpression(walker.GetSubwalker(open, close));
		walker.Index++;

		Node body = ParseStatement(walker);
		return new WhileStatementNode(condition, body, token1.RangePosition >> body.RangePosition);
	}

	private ForStatementNode ParseForStatement(Walker walker)
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

		IdentifierNode type = ParseType(subwalker);
		if (!subwalker.Peek(out Token? token4) || !token4.Represents(Types.Keyword, Definitions.Keywords.In)) throw new ExpectedIssue(Definitions.Keywords.In, ~type.RangePosition.End);
		subwalker.Index++;

		Node iterator = ParseExpression(subwalker);
		// if (subwalker.InRange) throw new UnexpectedIssue("extra tokens in for-loop header", subwalker.RangePosition);
		walker.Index++;

		Node body = ParseStatement(walker);
		return new ForStatementNode(item, type, iterator, body, token1.RangePosition >> body.RangePosition);
	}

	private BreakStatementNode ParseBreakStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, Definitions.Keywords.Break)) throw new ExpectedIssue(Definitions.Keywords.Break, walker.RangePosition);
		walker.Index++;

		return new BreakStatementNode(token.RangePosition);
	}

	private ContinueStatementNode ParseContinueStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Keyword, Definitions.Keywords.Continue)) throw new ExpectedIssue(Definitions.Keywords.Continue, walker.RangePosition);
		walker.Index++;

		return new ContinueStatementNode(token.RangePosition);
	}

	private IfStatementNode ParseIfStatement(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Keyword, Definitions.Keywords.If)) throw new ExpectedIssue(Definitions.Keywords.If, walker.RangePosition);
		walker.Index++;

		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) throw new ExpectedIssue(Definitions.Brackets.OpenParen, ~token1.RangePosition.End);
		string open = token2.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
		Node condition = ParseExpression(walker.GetSubwalker(open, close));
		walker.Index++;

		Node then = ParseStatement(walker);
		if (!walker.Peek(out Token? token3) || !token3.Represents(Types.Keyword, Definitions.Keywords.Else)) return new IfStatementNode(condition, then, null, token1.RangePosition >> then.RangePosition);
		walker.Index++;

		Node @else = ParseStatement(walker);
		return new IfStatementNode(condition, then, @else, token1.RangePosition >> @else.RangePosition);
	}

	private BlockNode ParseBlock(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, Definitions.Brackets.OpenBrace)) throw new ExpectedIssue(Definitions.Brackets.OpenBrace, walker.RangePosition);
		string open = token1.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
		Walker subwalker = walker.GetSubwalker(open, close);
		IEnumerable<Node> statements = [.. ParseProgram(subwalker)];
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(close, ~walker.RangePosition.End);
		walker.Index++;
		
		return new BlockNode(statements, token1.RangePosition >> token2.RangePosition);
	}

	private DeclarationNode ParseDeclaration(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for variable name", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token1.Value, token1.RangePosition);
		walker.Index++;

		IdentifierNode type = ParseType(walker);
		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) return new DeclarationNode(type, identifier, null, identifier.RangePosition >> type.RangePosition);

		string open = token2.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
		Node value = ParseExpression(walker.GetSubwalker(open, close));
		if (!walker.Peek(out Token? token3)) throw new ExpectedIssue(close, ~type.RangePosition.End);
		walker.Index++;

		return new DeclarationNode(type, identifier, value, identifier.RangePosition >> token3.RangePosition);
	}

	private IdentifierNode ParseType(Walker walker)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Identifier)) throw new ExpectedIssue("identifier for type", walker.RangePosition);
		IdentifierNode type = new(token1.Value, token1.RangePosition);
		walker.Index++;

		if (walker.Peek(out Token? token2) && token2.Represents(Types.Operator, Definitions.Brackets.OpenAngle))
		{
			string open = token2.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token2.RangePosition);
			walker.Index++;

			IEnumerable<IdentifierNode> generics = [.. ParseGenerics(walker)];
			if (!walker.Peek(out Token? token3)) throw new ExpectedIssue(close, ~type.RangePosition.End);
			walker.Index++;

			type = new TemplateNode(type, generics, type.RangePosition >> token3.RangePosition);
		}

		if (walker.Peek(out Token? token4) && token4.Represents(Types.Operator, Definitions.Operators.Question))
		{
			type = new TemplateNode(new IdentifierNode(Definitions.Types.Nullable, type.RangePosition), [type], type.RangePosition >> token4.RangePosition);
			walker.Index++;
		}

		return type;
	}

	private TemplateNode ParseTemplate(Walker walker, IdentifierNode identifier)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Operator, Definitions.Brackets.OpenAngle)) throw new ExpectedIssue(Definitions.Brackets.OpenAngle, walker.RangePosition);
		walker.Index++;

		IEnumerable<IdentifierNode> generics = [.. ParseGenerics(walker)];
		if (!walker.Peek(out Token? token2) || !token2.Represents(Types.Operator, Definitions.Brackets.CloseAngle)) throw new ExpectedIssue(Definitions.Brackets.CloseAngle, ~walker.RangePosition.End);
		walker.Index++;

		return new TemplateNode(identifier, generics, identifier.RangePosition >> token2.RangePosition);
	}

	private IEnumerable<IdentifierNode> ParseGenerics(Walker walker)
	{
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return ParseType(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, Definitions.Separators.Comma)) break;
			walker.Index++;
		}
	}

	private Node ParseExpression(Walker walker)
	{
		return ParseDisjunction(walker);
	}

	private Node ParseDisjunction(Walker walker)
	{
		Node left = ParseConjunction(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Or))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;

			Node right = ParseConjunction(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ParseConjunction(Walker walker)
	{
		Node left = ParseRelation(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.And))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;

			Node right = ParseRelation(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ParseRelation(Walker walker)
	{
		Node left = ParseAdditive(walker);
		while (walker.Peek(out Token? token) && (token.Represents(Types.Operator, Definitions.Operators.NotEqual, Definitions.Operators.Equal, Definitions.Operators.LessOrEqual, Definitions.Operators.GreaterOrEqual, Definitions.Brackets.OpenAngle, Definitions.Brackets.CloseAngle)))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;

			Node right = ParseAdditive(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ParseAdditive(Walker walker)
	{
		Node left = ParseMultiplicative(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Plus, Definitions.Operators.Minus))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;

			Node right = ParseMultiplicative(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ParseMultiplicative(Walker walker)
	{
		Node left = ParsePrefix(walker);
		while (walker.Peek(out Token? token) && token.Represents(Types.Operator, Definitions.Operators.Multiply, Definitions.Operators.Divide))
		{
			IdentifierNode @operator = new(token.Value, token.RangePosition);
			walker.Index++;

			Node right = ParsePrefix(walker);
			left = new BinaryOperatorNode(@operator, left, right, left.RangePosition >> right.RangePosition);
		}
		return left;
	}

	private Node ParsePrefix(Walker walker)
	{
		if (!walker.Peek(out Token? token) || !token.Represents(Types.Operator, Definitions.Operators.Plus, Definitions.Operators.Minus, Definitions.Operators.Not)) return ParsePrimary(walker);
		IdentifierNode @operator = new(token.Value, token.RangePosition);
		walker.Index++;

		Node target = ParsePrimary(walker);
		return new UnaryOperatorNode(@operator, target, token.RangePosition >> target.RangePosition);
	}

	private Node ParseAtomic(Walker walker)
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
		case Types.Identifier: return ParseDesignator(walker);
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
				Node expression = ParseExpression(walker.GetSubwalker(open, close));
				walker.Index++;

				return expression;
			}
			if (token.Represents(Definitions.Brackets.OpenBracket))
			{
				string open = token.Value;
				if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
				IEnumerable<Node> elements = [.. ParseArguments(walker.GetSubwalker(open, close))];
				if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~token.RangePosition.End);
				walker.Index++;

				return new ArrayNode(elements, token.RangePosition >> closeToken.RangePosition);
			}
			throw new UnexpectedIssue(token.Value, token.RangePosition);
		}
		default: throw new UnexpectedIssue(token.Value, token.RangePosition);
		}
	}

	private Node ParsePrimary(Walker walker)
	{
		Node node = ParseAtomic(walker);
		while (walker.Peek(out Token? token) && (token.Represents(Types.Operator, Definitions.Operators.Dot) || token.Represents(Types.Bracket, Definitions.Brackets.OpenParen) || token.Represents(Types.Bracket, Definitions.Brackets.OpenBracket)))
			node = ParsePostfix(walker, node);
		return node;
	}

	private Node ParsePostfix(Walker walker, Node node)
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
			string open = token.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
			IEnumerable<Node> arguments = [.. ParseArguments(walker.GetSubwalker(open, close))];
			if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~node.RangePosition.End);
			walker.Index++;

			return new InvocationNode(node, arguments, node.RangePosition >> closeToken.RangePosition);
		}
		if (token.Represents(Types.Bracket, Definitions.Brackets.OpenBracket))
		{
			string open = token.Value;
			if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token.RangePosition);
			Node index = ParseExpression(walker.GetSubwalker(open, close));
			if (!walker.Peek(out Token? closeToken)) throw new ExpectedIssue(close, ~node.RangePosition.End);
			walker.Index++;

			return new IndexNode(node, index, node.RangePosition >> closeToken.RangePosition);
		}
		return node;
	}

	private Node ParseDesignator(Walker walker)
	{
		if (!walker.Peek(out Token? token)) throw new ExpectedIssue("identifier expression", ~walker.RangePosition.Begin);
		IdentifierNode identifier = new(token.Value, token.RangePosition);
		walker.Index++;

		if (walker.Attempt(() => ParseTemplate(walker, identifier), out TemplateNode? template)) return template;

		return ParseInvocation(walker, identifier);
	}

	private Node ParseInvocation(Walker walker, IdentifierNode identifier)
	{
		if (!walker.Peek(out Token? token1) || !token1.Represents(Types.Bracket, Definitions.Brackets.OpenParen)) return identifier;
		string open = token1.Value;
		if (!Brackets.TryGetValue(open, out string? close)) throw new UnmatchedBracketIssue(open, token1.RangePosition);
		IEnumerable<Node> arguments = [.. ParseArguments(walker.GetSubwalker(open, close))];
		if (!walker.Peek(out Token? token2)) throw new ExpectedIssue(close, ~identifier.RangePosition.End);
		walker.Index++;

		return new InvocationNode(identifier, arguments, identifier.RangePosition >> token2.RangePosition);
	}

	private IEnumerable<Node> ParseArguments(Walker walker)
	{
		if (!walker.InRange) yield break;
		while (true)
		{
			yield return ParseExpression(walker);
			if (!walker.Peek(out Token? token) || !token.Represents(Types.Separator, Definitions.Separators.Comma)) break;
			walker.Index++;
		}
	}
}
