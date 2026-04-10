using MHAuthorWebsite.Data.Shared;

namespace MHAuthorWebsite.Tests.Services;

[SetUpFixture]
public sealed class TestAssemblySetup
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        QueryBridge.Initialize();
    }
}
