using CleanArchitecture.Shared;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddAzurePostgresFlexibleServer(Services.DatabaseServer)
    .WithPasswordAuthentication()
    .RunAsContainer(container =>
        container.WithLifetime(ContainerLifetime.Persistent))
    .AddDatabase(Services.Database);

// Port is pinned so it matches the static "Authentication:Keycloak:Authority" default in
// appsettings.json (http://localhost:8080/realms/cleanarchitecture) without extra wiring.
var keycloak = builder
    .AddKeycloak(Services.Keycloak, port: 8080)
    .WithDataVolume()
    .WithRealmImport("../../deploy/keycloak")
    .WithLifetime(ContainerLifetime.Persistent);

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

#if (!UseApiOnly)
if (builder.ExecutionContext.IsRunMode)
{
    builder.AddJavaScriptApp(Services.WebFrontend, "./../Web/ClientApp")
        .WithRunScript("dev")
        .WithReference(web)
        .WaitFor(web)
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints();
}
#endif

builder.Build().Run();
