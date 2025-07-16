using System.IO;

namespace StarConflictsRevolt.Clients.Wpf.Extensions;

internal static class ServicesCollectionExtensions
{
    /// <summary>
    /// Sets the content root path to be the same as the EXE
    /// </summary>
    /// <param name="context"></param>
    /// <returns>The directory for the application root path</returns>
    public static DirectoryInfo SetContentPathToApplicationDirectory(this HostBuilderContext context)
    {
        context.HostingEnvironment.ContentRootPath = AppContext.BaseDirectory;
        return new DirectoryInfo(context.HostingEnvironment.ContentRootPath);
    }
}