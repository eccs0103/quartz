using Quartz.Shared.Helpers;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Evaluating;

internal class ModuleBuilder(Module module, Scope location)
{
	public ModuleBuilder DeclareClass(string name, string? @base = null, IEnumerable<string>? generics = null, Action<ClassBuilder, IEnumerable<Class>>? configure = null)
	{
		generics ??= [];
		configure ??= (_, _) => { };

		if (generics.Any())
		{
			Generic generic = new(name, generics, (type, args, scope) =>
			{
				configure(new ClassBuilder(type, scope), args);
			}, location);
			location.Register(name, generic, ~Position.Zero);
			return this;
		}

		Scope scope = name.Equals(RuntimeBuilder.NameWorkspace)
			? RuntimeBuilder.Workspace
			: location.GetSubscope(name);
		
		Class? typeBase = null;
		if (@base != null) module.TryReadClass(@base, out typeBase);

		Class type = new(name, scope, typeBase);
		location.Register(name, type, ~Position.Zero);

		configure(new ClassBuilder(type, scope), []);
		return this;
	}
}
