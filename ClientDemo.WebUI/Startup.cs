using ClientDemo.Application.JsonServerClient.Abstract;
using ClientDemo.Application.JsonServerClient.Concrete;
using ClientDemo.WebUI.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientDemo.WebUI
{
	public class Startup
	{

		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{

			services.AddSingleton(Configuration);

			/*
			 * You can inject with constructor injection; 
			 * 
			 * private readonly WebApiConfiguration _webApiConfig;
			 * public YourClass(IOptions<WebApiConfiguration> webApiConfig)
			 * {
			 *		_webApiConfig = webApiConfig.Value;
			 * }
			 * 
			 */
			services.Configure<WebApiConfiguration>(Configuration.GetSection("JsonServerWebApiSettings"));

			services.AddSingleton<IJsonServerDemoClient>(s => new JsonServerDemoClient(Configuration.GetSection("JsonServerWebApiSettings:BaseAddress").Value));

			services.AddMvc();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvcWithDefaultRoute();
		}
	}
}
