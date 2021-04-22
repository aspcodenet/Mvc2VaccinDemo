using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mvc1VaccinDemo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mvc1VaccinDemo.Controllers;
using Mvc1VaccinDemo.Services;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.Services.PersonGenerator;

namespace Mvc1VaccinDemo
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
            services.AddTransient<IOrderedVaccinService,OrderedVaccinService>();
            
            services.AddTransient<IPersonGeneratorService, PersonGeneratorService>();

            services.Configure<KrisInfoConfig>(Configuration.GetSection("KrisInfoConfig"));

            services.AddTransient<IKrisInfoService, KrisInfoService>();
            services.Decorate<IKrisInfoService, CachedKrisInfoService>();


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddDefaultIdentity<IdentityUser>(
                    options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "dump",
                    pattern: "dump.sql",
                    defaults:new {controller="Home",action="Dump"});
                endpoints.MapControllerRoute(
                    name: "dump2",
                    pattern: "backup.zip",
                    defaults: new { controller = "Home", action = "Backup" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
