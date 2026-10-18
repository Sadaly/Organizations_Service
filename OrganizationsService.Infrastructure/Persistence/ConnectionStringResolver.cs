using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OrganizationsService.Infrastructure.Persistence
{
    public static class ConnectionStringResolver
    {
        public static string Resolve(IConfiguration configuration)
        {
            var url = configuration["DATABASE_URL"];
            if (!string.IsNullOrEmpty(url))
                return FromUrl(url);

            return configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");
        }

        public static string FromUrl(string url)
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
                Database = uri.AbsolutePath.Trim('/'),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            };
            return builder.ConnectionString;
        }
    }
}
