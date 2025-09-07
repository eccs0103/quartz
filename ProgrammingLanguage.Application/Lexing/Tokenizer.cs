using System.Text;
using System.Text.RegularExpressions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Extensions;
using ProgrammingLanguage.Shared.Helpers;
using static ProgrammingLanguage.Application.Lexing.Token;

namespace ProgrammingLanguage.Application.Lexing;

public partial class Tokenizer
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
	private static readonly HashSet<string> Keywords = ["datum", "null", "import"];

	[GeneratedRegex(@"^\s+", RegexOptions.Compiled)]
	private static partial Regex WhitespacePattern();

	[GeneratedRegex(@"^\d+(\.\d+)?", RegexOptions.Compiled)]
	private static partial Regex NumberPattern();

	[GeneratedRegex(@"^""(.)*?(?<!\\)""", RegexOptions.Compiled)]
	private static partial Regex StringPattern();

	[GeneratedRegex(@"^(\+|-|\*|/|:)", RegexOptions.Compiled)]
	private static partial Regex OperatorPattern();

	[GeneratedRegex(@"^[A-z]\w*", RegexOptions.Compiled)]
	private static partial Regex IdentifierPattern();

	[GeneratedRegex(@"^[()]", RegexOptions.Compiled)]
	private static partial Regex BracketsPattern();

	[GeneratedRegex(@"^[;,]", RegexOptions.Compiled)]
	private static partial Regex SeparatorPattern();

	public Token[] Tokenize(string code)
	{
		Position begin = new(0, 0);
		List<Token> tokens = [];
		for (StringBuilder text = new(code); text.Length > 0;)
		{
			bool hasChanges = false;
			foreach ((Regex regex, Types? unknown) in Patterns)
			{
				Match match = regex.Match(text.ToString());
				if (!match.Success) continue;

				(string value, int length) = match;
				text.Remove(0, length);

				Position end = value.Aggregate(new MutablePosition(begin), (position, symbol) => position.IncrementBySymbol(symbol)).Seal();
				if (unknown is Types type)
				{
					if (type == Types.Identifier && Keywords.Contains(value))
					{
						type = Types.Keyword;
					}
					tokens.Add(new(type, value, new(begin, end)));
				}
				begin = end;

				hasChanges = true;
				break;
			}
			if (!hasChanges) throw new Issue($"Unidentified term '{text[0]}'", begin);
		}
		return [.. tokens];
	}
}
