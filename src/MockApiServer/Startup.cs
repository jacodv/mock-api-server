using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockApiServer.Services;
using Serilog;

namespace MockApiServer
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddCors();
      services.AddMvcCore()
        .AddNewtonsoftJson()
        .AddFluentValidation();
      services.AddSingleton<IMockDataService, MockDataService>();
      services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddSerilog();
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      
      UseCorsPolicy(app);
      //app.UseHttpsRedirection();
      app.UseRouting();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
          name:"TestSetup",
          pattern:"api/{controller=TestSetup}/{*path}");
        endpoints.MapControllers();
      });

    }
    private static void UseCorsPolicy(IApplicationBuilder app)
    {
      app.UseCors(policy =>
      {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        policy.Build();
      });
    }
  }
}
