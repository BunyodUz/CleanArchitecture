using CleanArchitecture.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// The frontend is a statically-exported Next.js app with no dev-time proxy (unlike a
// bundler dev server), so in development it calls the API cross-origin with the auth
// cookie attached. AllowAnyOrigin() can't be combined with credentialed requests — browsers
// reject that combination outright — so origins are echoed back instead via
// SetIsOriginAllowed, which keeps "any origin" while remaining compatible with credentials.
app.UseCors(static builder =>
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials());

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

#if (UseApiOnly)
app.Map("/", () => Results.Redirect("/scalar"));
#endif

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

#if (!UseApiOnly)
app.MapFallbackToFile("index.html");
#endif

app.Run();
