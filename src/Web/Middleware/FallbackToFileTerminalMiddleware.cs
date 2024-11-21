using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MarcelN.StaticFileServer.Web.Middleware;

internal sealed class FallbackToFileTerminalMiddleware
{
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string? _defaultContentType;
    private readonly FallbackToFileOptions _options;

    public FallbackToFileTerminalMiddleware(IWebHostEnvironment hostEnvironment, IOptions<StaticFileOptions> staticFileOptions, IOptions<FallbackToFileOptions> options)
    {
        _fileProvider = staticFileOptions.Value.FileProvider ?? hostEnvironment.WebRootFileProvider;
        _contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
        _defaultContentType = staticFileOptions.Value.DefaultContentType;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var fallbackFile = _fileProvider.GetFileInfo(_options.FallbackFilePath);

        if (fallbackFile.Exists)
        {
            // Determine content type of the fallback file
            if (!_contentTypeProvider.TryGetContentType(_options.FallbackFilePath, out var fallbackFileContentType))
            {
                fallbackFileContentType = _defaultContentType;
            }

            // Write the file contents to the response
            context.Response.Clear();

            context.Response.ContentLength = fallbackFile.Length;
            context.Response.ContentType = fallbackFileContentType;
            context.Response.StatusCode = StatusCodes.Status200OK;

            await context.Response.SendFileAsync(fallbackFile).ConfigureAwait(continueOnCapturedContext: false);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}