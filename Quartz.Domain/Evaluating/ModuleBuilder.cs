using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class ModuleBuilder(Module module, Scope location)
{
	public ModuleBuilder DeclareClass(string name, Action<ClassBuilder> configure)
	{
		return DeclareClass(name, "Any", configure);
	}

	public ModuleBuilder DeclareClass(string name, string? parentName, Action<ClassBuilder> configure)
	{
		Scope scope = name.Equals(RuntimeBuilder.NameWorkspace)
			? RuntimeBuilder.Workspace
			: location.GetSubscope(name);
		
		Class? parent = null;
		if (parentName != null && name != "Any")
		{
			// Try to find the parent class in the module's scope
			/* Note: This assumes parent is already declared. 
			   If forward references are needed, this needs to be lazy or two-pass. 
			   For built-ins, order matters. */
			try 
			{
				parent = module.ReadClass(parentName, ~Position.Zero);
			}
			catch (NotExistIssue) { /* Ignore if not found, or maybe throw? For now assume it might be null or handled elsewhere */ }
		}

		Class type = module.RegisterClass(name, parent, scope, ~Position.Zero);
		configure(new ClassBuilder(type, scope));
		return this;
	}
}
