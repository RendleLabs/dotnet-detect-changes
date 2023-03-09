using RendleLabs.DetectChanges;

namespace ChangeDetector.Tests;

public class PathHelperTests
{
    [Fact]
    public void CorrectsSlashes()
    {
        if (Path.DirectorySeparatorChar == '/')
        {
            var actual = PathHelper.FixSeparator(@".\src\Foo\Foo.csproj");
            Assert.Equal("./src/Foo/Foo.csproj", actual);
        }
        else if (Path.DirectorySeparatorChar == '\\')
        {
            var actual = PathHelper.FixSeparator("./src/Foo/Foo.csproj");
            Assert.Equal(@".\src\Foo\Foo.csproj", actual);
        }
    }
}