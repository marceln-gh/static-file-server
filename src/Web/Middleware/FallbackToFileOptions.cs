namespace MarcelN.StaticFileServer.Web.Middleware;

internal sealed class FallbackToFileOptions()
{
    public const string DefaultFallbackFilePath = "index.html";

    public string FallbackFilePath { get; set; } = DefaultFallbackFilePath;
}