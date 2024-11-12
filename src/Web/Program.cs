using MarcelN.StaticFileServer.Web;

using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateEmptyBuilder(new() { Args = args });

// Configure host
builder.WebHost.UseKestrelCore();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.AddServerHeader = false;
});

// Configure configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["LogLevel:Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware"] = "Information",
    ["Logging:Console:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
    ["Logging:Console:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics"] = "None",
    ["Logging:Console:LogLevel:Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"] = "None"
});
builder.Configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");
builder.Configuration.AddEnvironmentVariables(prefix: "SFS_");
if (args is { Length: > 0 })
{
    builder.Configuration.AddCommandLine(args);
}

// Register services
builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddSimpleConsole(o => o.IncludeScopes = true);
});
builder.Services.AddHttpLogging(logging =>
{
    // Log everything except for request- and response bodies
    logging.LoggingFields = HttpLoggingFields.All & ~HttpLoggingFields.RequestBody & ~HttpLoggingFields.ResponseBody;

    // Log a single entry containing the request, response and duration
    logging.CombineLogs = true;
});
builder.Services.AddSingleton<FallbackToIndexMiddleware>();
builder.Services.AddDirectoryBrowser();

// Configure middleware pipeline and start the server
var app = builder.Build();

if (app.Configuration.GetValue("DEBUG", false))
{
    app.UseHttpLogging();
}

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
