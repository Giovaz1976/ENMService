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

                using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log where event_code in ('XX000', 'P00001', )", sourceConnection);
                using NpgsqlDataReader reader = command.ExecuteReader();

                using NpgsqlConnection destinationConnection = new NpgsqlConnection(destinationConnectionString);
                destinationConnection.Open();

                using NpgsqlTransaction transaction = destinationConnection.BeginTransaction();

                try
                {
                    while (reader.Read())
                    {
                        // Assuming your destination table has the same structure as the source table
                        using NpgsqlCommand insertCommand = new NpgsqlCommand("INSERT INTO enm.events_log VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11, @param12, @param13)", destinationConnection);
                        insertCommand.Parameters.AddWithValue("param1", reader["event_id"]);
                        insertCommand.Parameters.AddWithValue("param2", reader["event_type_id"]);
                        insertCommand.Parameters.AddWithValue("param3", reader["event_level_id"]);
                        insertCommand.Parameters.AddWithValue("param4", reader["event_system_id"]);
                        insertCommand.Parameters.AddWithValue("param5", reader["event_module_id"]);
                        insertCommand.Parameters.AddWithValue("param6", reader["event_object_id"]);
                        insertCommand.Parameters.AddWithValue("param7", reader["event_datetime_utc"]);
                        insertCommand.Parameters.AddWithValue("param8", reader["event_datetime"]);
                        insertCommand.Parameters.AddWithValue("param9", reader["event_offset"]);
                        insertCommand.Parameters.AddWithValue("param10", reader["event_code"]);
                        insertCommand.Parameters.AddWithValue("param11", reader["event_message"]);
                        insertCommand.Parameters.AddWithValue("param12", reader["event_info"]);
                        insertCommand.Parameters.AddWithValue("param13", reader["partition_id"]);
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