using System.Data;
using System.Data.SqlClient;
using Serilog;

namespace CSVtoDBLoader.Services
{
    public class Util
    {
        public static IDbConnection GetOpenConnection()
        {
            var connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=IMDBChallenge;Trusted_Connection=True;MultipleActiveResultSets=true");
            connection.Open();
            return connection;
        }

        public static void SetupLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("../../../logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Starting");
        }
    }
}
