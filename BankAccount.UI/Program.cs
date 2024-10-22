using BankAccountSimulation.UI.Constants;
using BankAccountSimulation.UI.Services;

namespace BankAccountSimulation.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Load configuration from appsettings.json
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Retrieve the base API URL from configuration
            var baseApiUrl = builder.Configuration["BaseApiUrl"];
            ApiConstants.BaseApiUrl = baseApiUrl;


            // Register HttpClient
            //builder.Services.AddHttpClient<IAccountApiService, AccountApiService>(client =>
            //{
            //    client.BaseAddress = new Uri(ApiConstants.BaseApiUrl);
            //});

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient<IAccountApiService, AccountApiService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Bank}/{action=Dashboard}/{id?}");

            app.Run();
        }
    }
}
