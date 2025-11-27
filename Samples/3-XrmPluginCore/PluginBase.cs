using Microsoft.Extensions.DependencyInjection;
using XrmPluginCore;

namespace SamplePlugins
{
	internal class PluginBase : Plugin
	{
		protected override IServiceCollection OnBeforeBuildServiceProvider(IServiceCollection services)
		{
			return services.AddScoped<IAccountService, AccountService>();
		}
	}
}
