namespace Quartz.Domain.Evaluating;

public class Empty
{
	public static Empty Instance { get; } = new();

	private Empty()
	{
	}

	public override string ToString()
	{
		return "null";
	}
}
