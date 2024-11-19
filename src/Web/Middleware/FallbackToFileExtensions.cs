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

    public static IApplicationBuilder UseFallbackToFile(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<FallbackToFileMiddleware>();
    }
}
