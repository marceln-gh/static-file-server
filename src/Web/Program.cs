using MarcelN.StaticFileServer.Web;

var builder = WebApplication.CreateEmptyBuilder(new() { Args = args });
//var builder = WebApplication.CreateSlimBuilder(args);

// Configure host
builder.WebHost.UseKestrelCore();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Configure configuration
builder.Configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");
builder.Configuration.AddEnvironmentVariables(prefix: "SFS_");
if (args is { Length: > 0 })
{
    builder.Configuration.AddCommandLine(args);
}

// Register services
builder.Services.AddSingleton<FallbackToIndexMiddleware>();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

if (app.Configuration.GetValue("FALLBACK_TO_INDEX", false))
{
    app.UseMiddleware<FallbackToIndexMiddleware>();
}

var fileServerOptions = new FileServerOptions
{
    EnableDefaultFiles = app.Configuration.GetValue("ALLOW_INDEX", true),
    EnableDirectoryBrowsing = app.Configuration.GetValue("SHOW_LISTING", true)
};
fileServerOptions.DefaultFilesOptions.DefaultFileNames = [FallbackToIndexMiddleware.FallbackFileName];
app.UseFileServer(fileServerOptions);

app.Run();
