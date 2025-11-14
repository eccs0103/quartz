namespace Quartz.Domain.Evaluating;

internal class RuntimeBuilder
{
	private const string NameGlobal = "@";
	private static readonly Scope Location = new(NameGlobal);
	private readonly Module Global = new(NameGlobal, Location);

	public const string NameWorkspace = "@Workspace";
	public static readonly Scope Workspace = Location.GetSubscope(NameWorkspace);

	public void DeclareModule(Action<ModuleBuilder> configure)
	{
		configure(new ModuleBuilder(Global, Location));
	}
}
