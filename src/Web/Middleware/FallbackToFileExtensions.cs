namespace MarcelN.StaticFileServer.Web.Middleware;

internal static class FallbackToFileExtensions
{
    public static IServiceCollection AddFallbackToFile(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<FallbackToFileMiddleware>();
    }

    public static IServiceCollection AddFallbackToFile(this IServiceCollection services, Action<FallbackToFileOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services
            .AddFallbackToFile()
            .Configure(configureOptions);
    }

    /// <summary>
    /// Adds a terminal middleware that runs if no endpoint handles the request.
    /// This middleware will fallback to a configured file.
    /// </summary>
    public static IApplicationBuilder UseFallbackToFile(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Run(async (HttpContext context) =>
        {
            var middleware = context.RequestServices.GetRequiredService<FallbackToFileTerminalMiddleware>();
            await middleware.InvokeAsync(context).ConfigureAwait(continueOnCapturedContext: false);
        });

        return app;
    }
}
