using Microsoft.EntityFrameworkCore;
using MVCDHProject.Models;

namespace MVCDHProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            //Injector Code
            //builder.Services.AddScoped<ICustomerDAL, CustomerXmlDAL>();
            builder.Services.AddScoped<ICustomerDAL, CustomerSqlDAL>();

            //Doing Dependency injection if we want to connect with sql we just need to change UseSqlServer to UseOracle and Connnection string key name
            builder.Services.AddDbContext<MVCCoreDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                //Client Side Middleware
                //app.UseStatusCodePages();
                //app.UseStatusCodePagesWithRedirects("/ClientError/{0}");    //It is like Response.Redirect()
                app.UseStatusCodePagesWithReExecute("/ClientError/{0}");    //It is like Server.Redirect()

                //Server Side Middleware
                //app.UseExceptionHandler("/Home/Error");
                app.UseExceptionHandler("/ServerError");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            //app.MapStaticAssets();
            app.UseStaticFiles();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
