using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Amporis.TasksChooser.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddMvc();          
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(env.WebRootPath, "App_Data"));
            app.UseMvc(
            routes =>
            {
                routes.MapRoute("index", "{fileName?}",
                    defaults: new { controller = "Home", action = "Index" });
                //routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            /*
            app.Run(async (context) =>
            {
                //string data = TasksProces.LoadData(context);
                //await context.Response.WriteAsync("HW");
            });//*/
        }
    }
}
