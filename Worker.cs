using Newtonsoft.Json;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;
using System.Net.Http;

using System.Text;

namespace ENMService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private System.Threading.Timer validationTimer;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            validationTimer = new System.Threading.Timer(PerformValidation, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));


        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            
            while (!stoppingToken.IsCancellationRequested)
            {
                //string sourceConnectionString = "Server=localhost;Port=5436;Database=cl.qfree.zen_0.0.9_202308;user id=qfree;Password=123456;";
                string sourceConnectionString = "Server=localhost;Database=smc;user id=postgres;Password=nolose;";
                string destinationConnectionString = "Server=localhost;Database=enm_db;user id=postgres;Password=nolose;";
               

                using NpgsqlConnection sourceConnection = new NpgsqlConnection(sourceConnectionString);
                await sourceConnection.OpenAsync();

                using NpgsqlConnection destinationConnection = new NpgsqlConnection(destinationConnectionString);
                await destinationConnection.OpenAsync();

                using (var deleteCommand = new NpgsqlCommand("DELETE FROM enm.events_log", destinationConnection))
                {
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                //using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log where event_code  <> '99401' and event_code in ('XX000');", sourceConnection);
                using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log where event_code in ('99401','XX000','P0001','99999','99480','99409','99403','99400','42P01','42883','42846','42809','42804','42803','42704','42703','42702','42601','3F000','2D000','23505','22P02','22023','22012','22007','22004','22003','22001','21000','0A000') AND event_datetime AT TIME ZONE 'CST7CDT' BETWEEN CURRENT_TIMESTAMP - INTERVAL '10 minutes' AND CURRENT_TIMESTAMP;", sourceConnection);
                //using NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM events.events_log where event_code  <> '99401' and event_code in ('XX000','P0001','99999','99480','99409','99403','99400','42P01','42883','42846','42809','42804','42803','42704','42703','42702','42601','3F000','2D000','23505','22P02','22023','22012','22007','22004','22003','22001','21000','0A000') AND event_datetime AT TIME ZONE 'CST7CDT' BETWEEN CURRENT_TIMESTAMP - INTERVAL '10 minutes' AND CURRENT_TIMESTAMP;", sourceConnection);
                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                
                //using NpgsqlTransaction transaction = destinationConnection.BeginTransaction();

                try
                {
                    
                    while (await reader.ReadAsync())
                    {
                        // Assuming destination table has the same structure as the source table


                        var rowData = new Dictionary<string, object> 
                        {

                       {"param1" , reader["event_id"] },
                       {"param2" , reader["event_type_id"] },
                       {"param3" , reader["event_level_id"] },
                       {"param4" , reader["event_system_id"] },
                       {"param5" , reader["event_module_id"] },
                       {"param6" , reader["event_object_id"] },
                       {"param7" , reader["event_datetime_utc"] },
                       {"param8" , reader["event_datetime"] },
                       {"param9" , reader["event_offset"] },
                       {"param10" , reader["event_code"] },
                       {"param11" , reader["event_message"] },
                       {"param12" , reader["event_info"] },
                       {"param13" , reader["partition_id"] },



                        };
                        rowData.Add("NotFrom", "emanotmod@gmail.com");
                        rowData.Add("NotTo", "emanotmod@gmail.com");
                        rowData.Add("NotType", 2);
                        rowData.Add("NotState", 1);
                        rowData.Add("NotResponse", 200);
                        rowData.Add("NotSubject", reader["event_message"]);
                        rowData.Add("NotContent", reader["event_info"]);
                        rowData.Add("EventId", reader["event_id"]);
                                           
                                              
                        

                        using NpgsqlCommand insertCommandIN = new NpgsqlCommand("INSERT INTO enm.events_log VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11, @param12, @param13)", destinationConnection);

                        insertCommandIN.Parameters.AddWithValue("param1", rowData["param1"]);
                        insertCommandIN.Parameters.AddWithValue("param2", rowData["param2"]);
                        insertCommandIN.Parameters.AddWithValue("param3", rowData["param3"]);
                        insertCommandIN.Parameters.AddWithValue("param4", rowData["param4"]);
                        insertCommandIN.Parameters.AddWithValue("param5", rowData["param5"]);
                        insertCommandIN.Parameters.AddWithValue("param6", rowData["param6"]);
                        insertCommandIN.Parameters.AddWithValue("param7", rowData["param7"]);
                        insertCommandIN.Parameters.AddWithValue("param8", rowData["param8"]);
                        insertCommandIN.Parameters.AddWithValue("param9", rowData["param9"]);
                        insertCommandIN.Parameters.AddWithValue("param10", rowData["param10"]);
                        insertCommandIN.Parameters.AddWithValue("param11", rowData["param11"]);
                        insertCommandIN.Parameters.AddWithValue("param12", rowData["param12"]);
                        insertCommandIN.Parameters.AddWithValue("param13", rowData["param13"]);

                                             
                        // Add parameters for all columns

                        await insertCommandIN.ExecuteNonQueryAsync();

                        var payload = JsonConvert.SerializeObject(rowData);

                        //transaction.Commit();

                       
                        using (var httpClient = new HttpClient())
                        {
                            var apiUrl = "https://localhost:5001/EmailSender/api/saveMail/";

                            var content = new StringContent(payload, Encoding.UTF8, "application/json");
                            //var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                            var response = await httpClient.PostAsync(apiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                _logger.LogInformation("API POST request succeeded for a row.");
                            }
                            else
                            {
                                _logger.LogError("API POST request failed with status code: {statusCode} for a row.", response.StatusCode);
                            }
                        }
                    }

                    

                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    Console.WriteLine("Error: " + ex.Message);
                }

                sourceConnection.Close();
                destinationConnection.Close();



                _logger.LogInformation("Table copy operation completed: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                //await Task.Delay(15000, stoppingToken);
                
            }
        }



        private void PerformValidation(object? state)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                _logger.LogError("Validation error: {error}", ex.Message);
            }
        }

       
    }
}