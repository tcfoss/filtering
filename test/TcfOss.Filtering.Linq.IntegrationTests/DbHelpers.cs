using DotNet.Testcontainers.Builders;
using Testcontainers.MariaDb;

namespace TcfOss.Filtering.Linq.IntegrationTests;

public static class DbHelpers
{
    private static readonly DirectoryInfo s_schemaDir = new(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, "..", "Resources", "MySqlSchema"));

    public static MariaDbBuilder WithStandardOptions(this MariaDbBuilder builder)
    {
        return builder
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithDatabase("test_library")
            .WithResourceMapping(s_schemaDir.FullName, "/docker-entrypoint-initdb.d/");
    }
}
