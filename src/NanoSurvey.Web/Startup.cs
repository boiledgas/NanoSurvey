using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using NanoSurvey.DB;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace NanoSurvey.Web
{
	/// <summary>
	/// 
	/// </summary>
	public class Startup
	{
		IConfiguration _configuration;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="configuration"></param>
		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContextPool<SurveyContext>(builder =>
				builder
					.UseSqlServer(_configuration.GetConnectionString("NanoSurvey"))
					.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			);
			services.AddSingleton(c => {
				var options = new DbContextOptionsBuilder<SurveyContext>()
					.UseSqlServer(_configuration.GetConnectionString("NanoSurvey"))
					.Options;

				Func<SurveyContext> factory = () => new SurveyContext(options);
				return factory;
			});
			services.AddSingleton(c =>
			{
				var context = c.GetService<SurveyContext>();
				var survey = context.Survey
					.Include(s => s.SurveyQuestion)
						.ThenInclude(s => s.Question)
					.Include(s => s.SurveyQuestionAnswer)
						.ThenInclude(s => s.Answer)
					.OrderBy(s => s.Id)
					.FirstOrDefault();

				Func<Survey> factory = () => survey;
				return factory;
			});

			services.AddLogging();

			services
				.AddMvcCore()
				.AddApiExplorer()
				.AddJsonFormatters(j => j.Formatting = Formatting.Indented);

			services.AddCors(options =>
			{
				options.AddPolicy("Allow", 
					builder => 
						builder
							.AllowAnyOrigin()
							.WithMethods("GET", "POST")
							.WithHeaders("Accept", "Content-Type"));
			});

			services
			.AddSwaggerGen(swagger =>
			{
				swagger.DescribeAllEnumsAsStrings();
				swagger.DescribeAllParametersInCamelCase();
				swagger.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Api NanoSurvey", Version = "v1" });
			})
			.ConfigureSwaggerGen(setup =>
			{
				var basePath = PlatformServices.Default.Application.ApplicationBasePath;
				var xmlPath = Path.Combine(basePath, "NanoSurvey.Web.xml");
				setup.IncludeXmlComments(xmlPath);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseCors("Allow");
			app.UseSwagger();
			app.UseSwaggerUI(setup =>
			{
				setup.SwaggerEndpoint("/swagger/v1/swagger.json", "Api NanoSurvey");
			});
			app.UseMvc();
		}
	}
}