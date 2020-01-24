using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace NanoSurvey.Web
{
	/// <summary>
	/// 
	/// </summary>
	public class Program
	{
		static void Main(string[] args)
		{
			var builder = new WebHostBuilder();

			builder.UseContentRoot(Directory.GetCurrentDirectory());
			builder.UseKestrel((builderContext, options) =>
			{
				options.Listen(IPAddress.Any, 50500);
			});
			builder.ConfigureAppConfiguration((hostingContext, config) =>
			{
				var env = hostingContext.HostingEnvironment;
				config.AddEnvironmentVariables();
				if (env.IsDevelopment())
				{
					config.AddUserSecrets<Program>();
				}
			});
			builder.UseStartup<Startup>();

			var host = builder.Build();
			host.Run();
		}
	}
}
