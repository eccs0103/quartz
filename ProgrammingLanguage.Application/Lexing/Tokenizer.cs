using System.Text;
using System.Text.RegularExpressions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Extensions;
using ProgrammingLanguage.Shared.Helpers;
using static ProgrammingLanguage.Application.Lexing.Token;

namespace ProgrammingLanguage.Application.Lexing;

internal partial class Tokenizer
{
	private static readonly Dictionary<Regex, Types?> Patterns = new()
	{
		{ StringPattern(), Types.String },
		{ WhitespacePattern(), null },
		{ NumberPattern(), Types.Number },
		{ OperatorPattern(), Types.Operator },
		{ IdentifierPattern(), Types.Identifier },
		{ BracketsPattern(), Types.Bracket },
		{ SeparatorPattern(), Types.Separator },
	};
	private static readonly HashSet<string> Keywords = ["true", "false", "if", "else"];

	private static void FixKeyword(ref Types type, string value)
	{
		if (type != Types.Identifier || !Keywords.Contains(value)) return;
		type = Types.Keyword;
	}

	[GeneratedRegex(@"^\s+", RegexOptions.Compiled)]
	private static partial Regex WhitespacePattern();

	[GeneratedRegex(@"^\d+(\.\d+)?", RegexOptions.Compiled)]
	private static partial Regex NumberPattern();

	[GeneratedRegex(@"^""([^""\\]|\\.)*""", RegexOptions.Compiled)]
	private static partial Regex StringPattern();

	[GeneratedRegex(@"^(<=|>=|<|>|=|\+|-|\*|/|:)", RegexOptions.Compiled)]
	private static partial Regex OperatorPattern();

	[GeneratedRegex(@"^[A-z]\w*", RegexOptions.Compiled)]
	private static partial Regex IdentifierPattern();

	[GeneratedRegex(@"^[(){}]", RegexOptions.Compiled)]
	private static partial Regex BracketsPattern();

	[GeneratedRegex(@"^[;,]", RegexOptions.Compiled)]
	private static partial Regex SeparatorPattern();

	public Token[] Tokenize(string code)
	{
		List<Token> tokens = [];
		Position begin = new(0, 0);
		for (StringBuilder text = new(code); text.Length > 0;)
		{
			bool hasChanges = false;
			foreach ((Regex regex, Types? unknown) in Patterns)
			{
				Match match = regex.Match(text.ToString());
				if (!match.Success) continue;
				(string value, int length) = match;
				text.Remove(0, length);

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
			if (!hasChanges) throw new UnidentifiedIssue($"term '{text[0]}'", ~begin);
		}
		return [.. tokens];
	}
}
