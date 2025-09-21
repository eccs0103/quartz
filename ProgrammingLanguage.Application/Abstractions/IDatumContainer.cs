using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Abstractions;

internal interface IDatumContainer
{
	public Datum RegisterConstant(string tag, string name, object? value, Range<Position> range);
	public Datum RegisterVariable(string tag, string name, object? value, Range<Position> range);
	public Datum ReadDatum(string name, Range<Position> range);
	public void WriteDatum(string name, string tag, object? value, Range<Position> range);
}
