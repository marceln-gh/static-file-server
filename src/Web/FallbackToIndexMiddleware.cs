using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MarcelN.StaticFileServer.Web;

internal class FallbackToIndexMiddleware : IMiddleware
{
    public const string FallbackFileName = "index.html";

    private readonly IFileInfo _indexFile;
    private readonly string? _contentType;

    public FallbackToIndexMiddleware(IWebHostEnvironment hostEnvironment, IOptions<StaticFileOptions> staticFileOptions)
    {
        _indexFile = hostEnvironment.WebRootFileProvider.GetFileInfo(FallbackFileName);

        var contentTypeProvider = staticFileOptions.Value.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(FallbackFileName, out _contentType))
        {
            _contentType = staticFileOptions.Value.DefaultContentType;
        }
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context).ConfigureAwait(continueOnCapturedContext: false);

        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            context.Response.Clear();

            context.Response.ContentType = _contentType;
            context.Response.ContentLength = _indexFile.Length;

            await context.Response.SendFileAsync(_indexFile).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
