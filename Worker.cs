using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENMService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string sourceConnectionString = "Server=localhost;Database=smc;user id=postgres;Password=nolose;";
                string destinationConnectionString = "Server=localhost;Database=enm_db;user id=postgres;Password=nolose;";

                using NpgsqlConnection sourceConnection = new NpgsqlConnection(sourceConnectionString);
                sourceConnection.Open();

                using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log", sourceConnection);
                using NpgsqlDataReader reader = command.ExecuteReader();

                using NpgsqlConnection destinationConnection = new NpgsqlConnection(destinationConnectionString);
                destinationConnection.Open();

                using NpgsqlTransaction transaction = destinationConnection.BeginTransaction();

                try
                {
                    while (reader.Read())
                    {
                        // Assuming your destination table has the same structure as the source table
                        using NpgsqlCommand insertCommand = new NpgsqlCommand("INSERT INTO destination_table VALUES (@param1, @param2, ...)", destinationConnection);
                        insertCommand.Parameters.AddWithValue("param1", reader["column1"]);
                        insertCommand.Parameters.AddWithValue("param2", reader["column2"]);
                        // Add parameters for all columns

                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error: " + ex.Message);
                }





                _logger.LogInformation("Table copy operation completed: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}