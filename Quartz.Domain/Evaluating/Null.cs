namespace Quartz.Domain.Evaluating;

public class Null
{
	public static Null Instance { get; } = new();

	private Null()
	{
	}

	public override string ToString()
	{
		return "null";
	}
}
