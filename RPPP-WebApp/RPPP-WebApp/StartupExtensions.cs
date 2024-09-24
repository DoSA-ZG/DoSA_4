using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RPPP_WebApp.Model;
using System.Reflection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


namespace RPPP_WebApp
{
  public static class StartupExtensions
  {
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
      var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets<Program>()
        .Build();

      var appSection = builder.Configuration.GetSection("AppSettings");
      builder.Services.Configure<AppSettings>(appSection);

      builder.Services.AddLogging(configure =>
        {
          configure.AddConfiguration(configuration.GetSection("Logging"));
          configure.AddConsole();
        })
        .AddDbContext<Rppp15Context>(options =>
        {
          options.UseSqlServer(configuration.GetConnectionString("RPPP15"));
        }, contextLifetime: ServiceLifetime.Transient)
        .AddControllersWithViews();

      builder.Services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc(Constants.ApiVersion, new OpenApiInfo
        {
          Title = "RPPP15 Web API",
          Version = Constants.ApiVersion
        });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
      });

      return builder.Build();
    }


    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
      #region Needed for nginx and Kestrel (do not remove or change this region)
      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                 ForwardedHeaders.XForwardedProto
      });
      string pathBase = app.Configuration["PathBase"];
      if (!string.IsNullOrWhiteSpace(pathBase))
      {
        app.UsePathBase(pathBase);
      }
      #endregion

      if (app.Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        app.Use(async (ctx, next) =>
       {
         await next();
         if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
         {
           ctx.Request.Path = "/Error/404";
           await next();
         }
       });
      }

      app.UseStaticFiles()
       .UseRouting()
       .UseEndpoints(endpoints =>
       {
         endpoints.MapDefaultControllerRoute();
       });

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.RoutePrefix = "docs";
        c.DocumentTitle = "RPPP15 Web Api";
        c.SwaggerEndpoint($"../swagger/{Constants.ApiVersion}/swagger.json", "RPPP15 WebAPI");
      });

      return app;
    }
  }
}