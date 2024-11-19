using System.Globalization;

using MarcelN.StaticFileServer.Web.Middleware;

using Microsoft.AspNetCore.HttpLogging;

static TValue GetEnvironmentVariableOrDefault<TValue>(string variableName, TValue defaultValue) where TValue : IParsable<TValue>
{
    var stringValue = Environment.GetEnvironmentVariable(variableName);
    return TValue.TryParse(stringValue, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
}

var portNumber = GetEnvironmentVariableOrDefault("SFS_PORT", 80);

var builder = WebApplication.CreateEmptyBuilder(new() { Args = args });

// Configure host
builder.WebHost.UseKestrelCore();
builder.WebHost.ConfigureKestrel(options =>
{
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0
    options.ListenAnyIP(portNumber);
    options.AddServerHeader = false;
});

// Configure configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["LogLevel:Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware"] = "Information", // Required by app.UseHttpLogging()
    ["Logging:Console:LogLevel:Microsoft.Hosting.Lifetime"] = "Information", // Displays relevant configuration during startup (e.g. the port and content-root)
    ["Logging:Console:LogLevel:Microsoft.AspNetCore.Hosting.Diagnostics"] = "None", // Disable default logging of request and response metadata
    ["Logging:Console:LogLevel:Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"] = "None" // Disable default logging performed by StaticFileMiddleware
});
builder.Configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");
builder.Configuration.AddEnvironmentVariables(prefix: "SFS_");

// Register services
builder.Services.AddLogging(logging =>
{
    _ = logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    _ = logging.AddSimpleConsole(o => o.IncludeScopes = true);
});
builder.Services.AddHttpLogging(logging =>
{
    // Log everything except for request- and response bodies
    logging.LoggingFields = HttpLoggingFields.All & ~HttpLoggingFields.RequestBody & ~HttpLoggingFields.ResponseBody;

    // Log a single entry containing the request, response and duration
    logging.CombineLogs = true;
});
builder.Services.AddFallbackToFile();
builder.Services.AddDirectoryBrowser();

// Configure middleware pipeline and start the server
var app = builder.Build();

if (app.Configuration.GetValue("DEBUG", false))
{
    _ = app.UseHttpLogging();
}

if (app.Configuration.GetValue("FALLBACK_TO_INDEX", false))
{
    _ = app.UseFallbackToFile();
}

var fileServerOptions = new FileServerOptions
{
    EnableDefaultFiles = app.Configuration.GetValue("ALLOW_INDEX", true),
    EnableDirectoryBrowsing = app.Configuration.GetValue("SHOW_LISTING", true)
};
fileServerOptions.DefaultFilesOptions.DefaultFileNames = ["index.html"];
app.UseFileServer(fileServerOptions);

app.Run();
