namespace RendleLabs.DetectChanges;

public static class PathHelper
{
    /// <summary>
    /// Fixes directory separator characters in paths
    /// </summary>
    /// <param name="original">The original string</param>
    /// <returns>Fixed string</returns>
    public static string FixSeparator(string original)
    {
        return Path.DirectorySeparatorChar switch
        {
            '/' => original.Replace('\\', '/'),
            '\\' => original.Replace('/', '\\'),
            _ => original
        };
    }
}