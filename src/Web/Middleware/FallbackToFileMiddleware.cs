using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MarcelN.StaticFileServer.Web.Middleware;

internal sealed class FallbackToFileMiddleware : IMiddleware
{
    private readonly IFileInfo _fallbackFile;
    private readonly string? _fallbackFileContentType;

    public FallbackToFileMiddleware(IWebHostEnvironment hostEnvironment, IOptions<StaticFileOptions> staticFileOptions, IOptions<FallbackToFileOptions> options)
    {
        var fileProvider = staticFileOptions.Value.FileProvider ?? hostEnvironment.WebRootFileProvider;

        _fallbackFile = fileProvider.GetFileInfo(options.Value.FallbackFilePath);
        var contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(options.Value.FallbackFilePath, out _fallbackFileContentType))
        {
            _fallbackFileContentType = staticFileOptions.Value.DefaultContentType;
        }
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context).ConfigureAwait(continueOnCapturedContext: false);

        if (_fallbackFile.Exists && context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            context.Response.Clear();

            context.Response.ContentType = _fallbackFileContentType;
            context.Response.ContentLength = _fallbackFile.Length;

            await context.Response.SendFileAsync(_fallbackFile).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}