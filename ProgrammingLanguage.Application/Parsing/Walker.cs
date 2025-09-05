using ProgrammingLanguage.Application.Lexing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;

namespace ProgrammingLanguage.Application.Parsing;

public class Walker(in Token[] tokens, in Range<uint> range)
{
	private readonly Token[] Tokens = tokens;
	private Wrapper<uint> IndexWrapper = new(0);
	public readonly Range<uint> RangeIndex = range;
	public uint Index
	{
		get => IndexWrapper.Value;
		set => IndexWrapper.Value = value;
	}
	public bool InRange => Max(RangeIndex.Begin, 0) <= Index && Index < Min(Tokens.Length, RangeIndex.End);
	public Token Token => Tokens[Index];
	public readonly Range<Position> RangePosition = new(tokens.FirstOrDefault()?.RangePosition.Begin ?? Position.Zero, tokens.LastOrDefault()?.RangePosition.End ?? Position.Zero);
	public Walker GetSubwalker(in uint begin, in uint end)
	{
		return new(Tokens, new(begin, end))
		{
			IndexWrapper = IndexWrapper
		};
	}
}
