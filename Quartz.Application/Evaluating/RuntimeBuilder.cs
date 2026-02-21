using Quartz.Domain.Evaluating;
using static Quartz.Shared.Constants;

namespace Quartz.Application.Evaluating;

internal delegate void ModuleConfigurator(ModuleBuilder module);

internal class RuntimeBuilder
{
	private const string NameGlobal = "@";
	private static Scope Location { get; } = new(NameGlobal);
	private Module Global { get; } = new(NameGlobal, Location);

	public static Scope Workspace { get; } = Location.GetSubscope(Types.Workspace);

	public void DeclareModule(ModuleConfigurator configurator)
	{
		configurator.Invoke(new ModuleBuilder(Global, Location));
	}
}
