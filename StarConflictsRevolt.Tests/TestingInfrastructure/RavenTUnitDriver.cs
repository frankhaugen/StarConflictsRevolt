using Raven.Client.Documents;
using Raven.Embedded;
using Raven.TestDriver;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal sealed class RavenTUnitDriver : RavenTestDriver
{
    static RavenTUnitDriver()
    {
        ConfigureServer(new TestServerOptions
        {
            Licensing = new ServerOptions.LicensingOptions
            {
                DisableLicenseSupportCheck = true // Disable license support check for testing
                // License = "your license here", // Replace with your actual license,
                // // or
                // LicensePath = "path to license.json file" // Replace with the actual path to your license.json file
            }
        });
    }

    public IDocumentStore NewStore(string database)
    {
        return GetDocumentStore(database: database);
    }
}