using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExampleProject
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Load the settings so we can use DI later on...
            //services.Configure<XenaProviderSettings>(Configuration.GetSection("XenaProvider"));
            var xenaSettings = Configuration.GetSection("XenaProvider").Get<XenaProviderSettings>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                })
                .AddOpenIdConnect(options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = xenaSettings.Authority;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = xenaSettings.ClientID;
                    options.ClientSecret = xenaSettings.ClientSecret;
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("testapi");

                    options.ClaimActions.MapJsonKey("website", "website");
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //When this demo is running inside Docker it is actually NAT'ed through, we need to support this by forwarding the headers...
            var forwardedHeadersOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);
            app.UseHttpMethodOverride();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
