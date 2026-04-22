using Soenneker.Utils.File.Replacer.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Utils.File.Replacer.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class FileReplacerTests : HostedUnitTest
{
    private readonly IFileReplacer _util;

    public FileReplacerTests(Host host) : base(host)
    {
        _util = Resolve<IFileReplacer>(true);
    }

    [Test]
    public void Default()
    {

    }
}
