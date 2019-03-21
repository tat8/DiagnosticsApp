using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.DatabaseModels;
using DiagnosticsApp.Services;
using DiagnosticsApp.Services.Blobs;
using DiagnosticsApp.Services.Clients;
using DiagnosticsApp.Services.Diagnostic;
using DiagnosticsApp.Services.Errors;
using DiagnosticsApp.Services.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiagnosticsApp
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
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DiagnosticsDBContext>(options =>
                options.UseSqlServer(connection));


            services.AddTransient<IErrorService, ErrorService>();
            services.AddTransient<IBlobService, BlobService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDiagnosticsService, DiagnosticsService>();
            services.AddTransient<IClientService, ClientService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapSpaFallbackRoute("angular-fallback",
                    new { controller = "Home", action = "Index" });
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
