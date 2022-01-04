using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ASp.netCore_empty_tutorial.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;
using ASp.netCore_empty_tutorial.Security;

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
            //add identity .net_core 
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                //configure password input points
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;

            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy", policy =>
                 {
                     policy.RequireClaim("Delete Role");
                 });
                options.AddPolicy("EditRolePolicy", policy =>
                 {
                     policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement());

                 });
                options.AddPolicy("AdminRolePolicy", policy =>
                  {
                      policy.RequireAssertion(context =>
                         context.User.IsInRole("Admin") ||
                         context.User.IsInRole("Super Admin")
                     );
                  });

            });
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();

            //Connect Google Autentication
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "652435436708-5d73e7gkvb2an5fkv8uf921185ofmr5l.apps.googleusercontent.com";
                options.ClientSecret = "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";
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminRoleHandler>();
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
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvc(route =>
            {

                //route.MapRoute("default", "{controller=Account}/{Login}");
                route.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvc();
            app.UseRouting();

            //endpoints by default
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
