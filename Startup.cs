using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ASp.netCore_empty_tutorial.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace ASp.netCore_empty_tutorial
{
    public class Startup
    {
        protected readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //add connection for SQLServer
            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("EmployeeConnectionStrings"));
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseStaticFiles();
            // app.UseMvcWithDefaultRoute();
            //add Authentication middleware
            app.UseAuthentication();
            app.UseMvc(route =>
            {
                route.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            //app.UseMvc();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // обработка запроса - получаем констекст запроса в виде объекта context
                endpoints.MapGet("/", async context =>
                {
                    // отправка ответа в виде строки "Hello World!"
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }


    }
}
