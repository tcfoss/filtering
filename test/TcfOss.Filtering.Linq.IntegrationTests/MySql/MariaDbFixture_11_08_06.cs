using System.Data.Common;
using MySqlConnector;
using Testcontainers.MariaDb;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace TcfOss.Filtering.Linq.IntegrationTests.MySql;

public class MariaDbFixture_11_08_06(IMessageSink messageSink)
    : DbContainerFixture<MariaDbBuilder, MariaDbContainer>(messageSink)
{
    protected override MariaDbBuilder Configure()
    {
        return new MariaDbBuilder("mariadb:11.8.6")
            .WithStandardOptions();
    }

    public override DbProviderFactory DbProviderFactory => MySqlConnectorFactory.Instance;
}
