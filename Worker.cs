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

                using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log where event_code in ('XX000', 'P00001')", sourceConnection);
                using NpgsqlDataReader reader = command.ExecuteReader();

                using NpgsqlConnection destinationConnection = new NpgsqlConnection(destinationConnectionString);
                destinationConnection.Open();

                using NpgsqlTransaction transaction = destinationConnection.BeginTransaction();

                try
                {
                    while (reader.Read())
                    {
                        // Assuming your destination table has the same structure as the source table

                        using NpgsqlCommand insertCommandDel = new NpgsqlCommand("Delete from enm.events_log", destinationConnection);

                        insertCommandDel.ExecuteNonQuery(); 

                        using NpgsqlCommand insertCommandIN = new NpgsqlCommand("INSERT INTO enm.events_log VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11, @param12, @param13)", destinationConnection);
                        insertCommandIN.Parameters.AddWithValue("param1", reader["event_id"]);
                        insertCommandIN.Parameters.AddWithValue("param2", reader["event_type_id"]);
                        insertCommandIN.Parameters.AddWithValue("param3", reader["event_level_id"]);
                        insertCommandIN.Parameters.AddWithValue("param4", reader["event_system_id"]);
                        insertCommandIN.Parameters.AddWithValue("param5", reader["event_module_id"]);
                        insertCommandIN.Parameters.AddWithValue("param6", reader["event_object_id"]);
                        insertCommandIN.Parameters.AddWithValue("param7", reader["event_datetime_utc"]);
                        insertCommandIN.Parameters.AddWithValue("param8", reader["event_datetime"]);
                        insertCommandIN.Parameters.AddWithValue("param9", reader["event_offset"]);
                        insertCommandIN.Parameters.AddWithValue("param10", reader["event_code"]);
                        insertCommandIN.Parameters.AddWithValue("param11", reader["event_message"]);
                        insertCommandIN.Parameters.AddWithValue("param12", reader["event_info"]);
                        insertCommandIN.Parameters.AddWithValue("param13", reader["partition_id"]);
                        // Add parameters for all columns

                        insertCommandIN.ExecuteNonQuery();
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