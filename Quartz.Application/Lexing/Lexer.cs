using System.Text;
using System.Text.RegularExpressions;
using Quartz.Domain.Exceptions;
using Quartz.Domain.Exceptions.Lexing;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;
using Quartz.Domain.Lexing;
using static Quartz.Domain.Lexing.Token;

namespace Quartz.Application.Lexing;

public partial class Lexer
{
	private static Dictionary<Regex, Types?> Patterns { get; } = new()
	{
		{ StringPattern(), Types.String },
		{ CharacterPattern(), Types.Character },
		{ WhitespacePattern(), null },
		{ CommentPattern(), null },
		{ NumberPattern(), Types.Number },
		{ OperatorPattern(), Types.Operator },
		{ IdentifierPattern(), Types.Identifier },
		{ BracketsPattern(), Types.Bracket },
		{ SeparatorPattern(), Types.Separator },
	};
	private static HashSet<string> Keywords { get; } =
	[
		"true",
		"false",
		"null",
		"if",
		"else",
		"while",
		"for",
		"in",
		"continue",
		"break"
	];

	private static void FixKeyword(ref Types type, string value)
	{
		if (type != Types.Identifier || !Keywords.Contains(value)) return;
		type = Types.Keyword;
	}

	[GeneratedRegex(@"\G\s+", RegexOptions.Compiled)]
	private static partial Regex WhitespacePattern();

	[GeneratedRegex(@"\G//[^\r\n]*", RegexOptions.Compiled)]
	private static partial Regex CommentPattern();

	[GeneratedRegex(@"\G\d+(\.\d+)?", RegexOptions.Compiled)]
	private static partial Regex NumberPattern();

	[GeneratedRegex(@"\G""([^""\\]|\\.)*""", RegexOptions.Compiled)]
	private static partial Regex StringPattern();

	[GeneratedRegex(@"\G'([^'\\]|\\.)'", RegexOptions.Compiled)]
	private static partial Regex CharacterPattern();

	[GeneratedRegex(@"\G(>=?|<=?|!=|=|\+|-|\*|/|:|\?|&|\||!|\.)", RegexOptions.Compiled)]
	private static partial Regex OperatorPattern();

	[GeneratedRegex(@"\G[A-z]\w*", RegexOptions.Compiled)]
	private static partial Regex IdentifierPattern();

	[GeneratedRegex(@"\G[(){}]", RegexOptions.Compiled)]
	private static partial Regex BracketsPattern();

	[GeneratedRegex(@"\G[;,]", RegexOptions.Compiled)]
	private static partial Regex SeparatorPattern();

	public Token[] Tokenize(string code)
	{
		List<Token> tokens = [];
		Position begin = new(0, 0);
		int cursor = 0;
		while (cursor < code.Length)
		{
			bool hasChanges = false;
			foreach ((Regex regex, Types? unknown) in Patterns)
			{
				Match match = regex.Match(code, cursor);
				if (!match.Success || match.Index != cursor) continue;
				(string value, int length) = match;
				cursor += length;

				MutablePosition position = new(begin);
				Position end = position.Increment(value.Take(length - 1)).ToImmutable();
				if (unknown is Types type)
				{
					FixKeyword(ref type, value);
					tokens.Add(new Token(type, value, begin >> end));
				}
				begin = position.Increment(value.TakeLast(1)).ToImmutable();

				hasChanges = true;
				break;
			}
			if (!hasChanges) throw new UnexpectedCharacterIssue(code[cursor], ~begin);
		}
		return [.. tokens];
	}
}
