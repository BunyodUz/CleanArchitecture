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
// Every frontend route is a real file in the static export (<route>/index.html, served by
// UseFileServer above), so a request that reaches here doesn't exist. Answer with the exported
// not-found page and a real 404, rather than index.html with a 200 — which rendered the Home
// page for unknown URLs and broke deep links/refreshes on every other route.
app.MapFallback(async context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;

    var notFoundPage = app.Environment.WebRootFileProvider.GetFileInfo("404.html");
    if (notFoundPage.Exists && !context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(notFoundPage);
    }
});
#endif

app.Run();
