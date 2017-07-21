using System.IdentityModel.Tokens.Jwt;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<XenaProviderSettings>(Configuration.GetSection("XenaProvider"));
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<XenaProviderSettings> xenaSettingsAccessor)
        {
            //We need this for docker! Docker modifies request url and this breaks stuff!
            var forwardedHeadersOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);
            app.UseHttpMethodOverride();
            //...End of docker support

            var xenaSettings = xenaSettingsAccessor.Value;
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies"
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "Cookies",
                
                
                Authority = xenaSettings.Authority,
                RequireHttpsMetadata = false,

                ClientId = xenaSettings.ClientID,
                ClientSecret = xenaSettings.ClientSecret,

                ResponseType = "code id_token",
                Scope = { "testapi" },
                PostLogoutRedirectUri = xenaSettings.CallBackUrl,
                
                GetClaimsFromUserInfoEndpoint = true,
                
                SaveTokens = true //uden den virker HttpContext.Authentication.GetTokenAsync("access_token") ikke
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
