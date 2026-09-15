using System.Security.Claims;
using Azure.Identity;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthorizationBuilder();

#if (UseApiOnly)
        // Pure API: callers obtain their own tokens from Keycloak and send them as
        // "Authorization: Bearer <token>" — this app only validates them.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
                options.Audience = builder.Configuration["Authentication:Keycloak:ClientId"];
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            });
#else
        // SPA (BFF pattern): this app is a confidential OIDC client. It drives the
        // Authorization Code flow with Keycloak server-side and hands the browser a
        // session cookie — the SPA never sees the access/id tokens.
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                builder.Configuration.GetSection("Authentication:Keycloak").Bind(options);

                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.TokenValidationParameters.NameClaimType = "preferred_username";
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                options.Events = new OpenIdConnectEvents
                {
                    // Keycloak needs the id_token_hint to complete RP-initiated logout
                    // (without it, it can't tell which session to end and shows a confirmation page).
                    OnRedirectToIdentityProviderForSignOut = async context =>
                    {
                        var idToken = await context.HttpContext.GetTokenAsync("id_token");
                        if (!string.IsNullOrEmpty(idToken))
                        {
                            context.ProtocolMessage.IdTokenHint = idToken;
                        }
                    }
                };
            });
#endif

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
#if (UseApiOnly)
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
#endif
        });

        builder.Services.AddCors();
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
