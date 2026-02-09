using Quartz.Domain.Evaluating;

namespace Quartz.Application.Evaluating;

internal class RuntimeBuilder
{
	private const string NameGlobal = "@";
	private static Scope Location { get; } = new(NameGlobal);
	private Module Global { get; } = new(NameGlobal, Location);

	public const string NameWorkspace = "@Workspace";
	public static Scope Workspace { get; } = Location.GetSubscope(NameWorkspace);

	public void DeclareModule(Action<ModuleBuilder> configure)
	{
		configure.Invoke(new ModuleBuilder(Global, Location));
	}
}
