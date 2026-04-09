using MHAuthorWebsite.Data.Shared;
using NUnit.Framework;

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
