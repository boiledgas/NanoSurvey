using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
			builder.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
			});
			builder.UseStartup<Startup>();

			var host = builder.Build();
			host.Run();
		}
	}
}
