using Soenneker.Utils.File.Replacer.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Utils.File.Replacer.Tests;

[Collection("Collection")]
public class FileReplacerTests : FixturedUnitTest
{
    private readonly IFileReplacer _util;

    public FileReplacerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IFileReplacer>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
