using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>()
                {
                    new CultureInfo("vi-VN"),
                    new CultureInfo("en-US"),
                };

                options.DefaultRequestCulture = new RequestCulture(supportedCultures.First());
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });


            builder.Services.AddSingleton<SharedViewLocalizer>();

            builder.Services.AddRequiredServices(configuration);



            //======================================================================================================================================

            var app = builder.Build();
            var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);
            app.UseSession();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseMiddleware<TokenValidationMiddleware>();
            app.UseAuthorization();

            app.UseMiddleware<AuditMobileHandlerMiddleware>();

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                app.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapGet(commonAPIConstant.Endpoint.Index, context =>
                {
                    var currUser = context.UserFromContext();
                    if (currUser?.AccountType == nameof(AccountType.TaiKhoanGiamSat))
                    {
                        return Task.Run(() => context.Response.Redirect(commonAPIConstant.Endpoint.WebApp_AuditMobile));
                    }

                    return Task.Run(() => context.Response.Redirect(commonAPIConstant.Endpoint.WebApp_InventoryReporting));
                });
            });

            app.Run();
        }
    }
}
