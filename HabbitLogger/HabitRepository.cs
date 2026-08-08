using Microsoft.Data.Sqlite;

namespace HabbitLogger;
internal static class HabitRepository
{   
    // Insert / Create
    public static void CreateHabitRecord(string habitName, int numberOfOccurences, DateTime date)
    {
        try
        {
            // Creates a connection to the sqlite database
            using (var conn = new SqliteConnection(DbConstants.connectionString))
            {
                // Create an sqlite command
                // Here we are stating the columns we are inserting into (HabitName, NumberOfOccurences, Date)
                // Here we are saying the values will comes from these parameters (@HabitName, @NumberOfOccurences, @Date)
                string query = "INSERT INTO HabitLogs (HabitName, NumberOfOccurences, Date) VALUES (@HabitName, @NumberOfOccurences, @Date)";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    // Give the command the values
                    // The parameterName argument must be an exact match with the value defined in values in the query.
                    cmd.Parameters.AddWithValue($"@{DbConstants.HabitNameString}", habitName);
                    cmd.Parameters.AddWithValue($"@{DbConstants.NumOfOccurencesString}", numberOfOccurences);
                    cmd.Parameters.AddWithValue($"@{DbConstants.DateString}", date);

                    // Opening the database
                    conn.Open();
                    int rows = cmd.ExecuteNonQuery(); // Executes the sqlite command against the database
                    Console.WriteLine($"\n{rows} record(s) inserted.");
                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected Error: " + ex.Message);
        }
    }

    public static void ReadHabitRecords()
    {
        try
        {
            using (var conn = new SqliteConnection(DbConstants.connectionString))
            {
                string query = "SELECT * FROM HabitLogs";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    conn.Open();
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n--- Habits ---");
                        while (reader.Read())
                        {
                            Console.WriteLine($"" +
                                $"\nId: {reader[DbConstants.IdString]}, " +
                                $"HabitName: {reader[DbConstants.HabitNameString]}, " +
                                $"Times Performed: {reader[DbConstants.NumOfOccurencesString]}, " +
                                $"Date: {reader[DbConstants.DateString]}");
                        }
                    }
                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected Error: " + ex.Message);
        }
    }

    public static void UpdateHabitRecord(int Id, HabitRecord habitRecord)
    {
        DateTime date;

        // Assign the date based on whether we are using the current date or the date defined by the user
        if (habitRecord.UseCurrentDate)
        {
            date = habitRecord.CurrentDate!.Value;
        }
        else
        {
            date = new DateTime(habitRecord.Year!.Value, habitRecord.Month!.Value, habitRecord.Day.Value);
        }

        try
        {
            using (var conn = new SqliteConnection(DbConstants.connectionString))
            {
                string query = $"UPDATE HabitLogs SET HabitName = @{DbConstants.HabitNameString}, " +
                    $"NumberOfOccurences = @{DbConstants.NumOfOccurencesString}, " +
                    $"Date = @{DbConstants.DateString} WHERE Id = @{DbConstants.IdString}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{DbConstants.IdString}", Id);
                    cmd.Parameters.AddWithValue($"@{DbConstants.HabitNameString}", habitRecord.HabitName);
                    cmd.Parameters.AddWithValue($"@{DbConstants.NumOfOccurencesString}", habitRecord.NumberOfOccurences);
                    cmd.Parameters.AddWithValue($"@{DbConstants.DateString}", date);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"{rowsAffected} row(s) updated.");
                    }
                    else
                    {
                        Console.WriteLine("0 rows affected. Invalid Id passed in");
                        // UpdateHabit() - remove in future but reminder just in case you want to re call the update habit method.
                    }

                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected Error:" + ex.Message);
        }
    }

    public static void DeleteHabitRecord(int Id)
    {
        try
        {
            using (var conn = new SqliteConnection(DbConstants.connectionString))
            {
                conn.Open();
                string query = $"DELETE FROM HabitLogs WHERE Id = @{DbConstants.IdString}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    // Add parameter to prevent SQL injection
                    cmd.Parameters.AddWithValue($"@{DbConstants.IdString}", Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine($"Row with ID {Id} deleted successfully.");
                    else
                        Console.WriteLine($"No row found with ID {Id}.");
                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected Error:" + ex.Message);
        }
    }
}

