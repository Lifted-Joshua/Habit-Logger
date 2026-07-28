
using System.Data;
using Microsoft.Data.Sqlite;

namespace HabbitLogger;

class Program
{
    private static readonly string connectionString = "Data Source=habbitlogger.db";
    private const string IdString = "Id";
    private const string TableNameString = "HabitLogs";
    private const string HabitNameString = "HabitName";
    private const string NumOfOccurencesString = "NumberOfOccurences";
    private const string DateString = "Date";
    static void Main()
    {
        InitializeDatabase();
    }

    static void InitializeDatabase()
    {
        // Creating an sqlLite database
        using var connection = new SqliteConnection(connectionString);

        connection.Open();

        using var command = new SqliteCommand(string.Empty, connection);

        // Creating the table within the sqlLite database
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS HabitLogs (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            HabitName TEXT,
            NumberOfOccurences INTEGER,
            Date DATE
        )";

        command.ExecuteNonQuery();

        // Later add a check here to to call create habit instantly when there a no rows in the table
        // If table has at least one row give user option to select any of the crud operations

        // Later create a main menu that based on user choice calls one of the crud methods

        // CreateHabit();
        ReadHabitRecords();
        // UpdateHabit();
        // DeleteHabit();
    }


    public static void CreateHabit()
    {
        var habitRecord = CreatingNewHabitRow();


        if(habitRecord.UseCurrentDate)
        {
            // Create a table in the database using the values and the current date using ADO.Net
            InsertHabitRecord(habitRecord.HabitName, habitRecord.NumberOfOccurences, habitRecord.CurrentDate!.Value);
        }
        else
        {
            // Create a table in the database using the values and the date the user entered using ADO.Net
            InsertHabitRecord(habitRecord.HabitName, habitRecord.NumberOfOccurences, new DateTime(habitRecord.Year!.Value, habitRecord.Month!.Value, habitRecord.Day!.Value));
        }
    }

    // Create
    private static void InsertHabitRecord(string habitName, int numberOfOccurences, DateTime date)
    {
        try
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                string query = "INSERT INTO HabitLogs (HabitName, NumberOfOccurences, Date) VALUES (@HabitName, @NumberOfOccurences, @Date)";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{HabitNameString}", habitName);
                    cmd.Parameters.AddWithValue($"@{NumOfOccurencesString}", numberOfOccurences);
                    cmd.Parameters.AddWithValue($"@{DateString}", date);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
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

    // Read
    private static int ReadHabitRecords()
    {
        Console.WriteLine("\nDisplaying all habits in the database");
        int numberOfHabits = 0;

        try
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                string query = "SELECT HabitName, NumberOfOccurences, Date FROM HabitLogs";
                using (var cmd = new SqliteCommand(query, conn))
                {

                    conn.Open();
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n--- Habits ---");
                        while (reader.Read())
                        {
                            Console.WriteLine($"\nHabit: {numberOfHabits+1}, HabitName: {reader[HabitNameString]}, Times Performed: {reader[NumOfOccurencesString]}, Date: {reader[DateString]}");
                            numberOfHabits++;
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

        return numberOfHabits;
    }

    private static void UpdateHabit()
    {
        int id = 0;
        int userChoice;
        Console.WriteLine("----Updating a habit in the database----");
        // Displaying all the habits in the database
        var numberOfHabits = ReadHabitRecords();

        Console.WriteLine($"\nWhich habit do you want to modify between habit 1 and {numberOfHabits}, enter a number: ");
        var isValidHabitNum = Console.ReadLine();

        while(!int.TryParse(isValidHabitNum, out userChoice) || userChoice < 1 || userChoice > numberOfHabits)
        {
            Console.WriteLine($"Invalid choice. Choose a habit between Habit 1 and habit {numberOfHabits}: ");
            isValidHabitNum = Console.ReadLine();
        }

        // Grab the id and the habit row of the passed in userChoice
        string query = $"SELECT Id, {HabitNameString}, {NumOfOccurencesString}, {DateString} FROM {TableNameString} WHERE Id = @{IdString}";

        try
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                using (SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    cmd.Parameters.AddWithValue($"@{IdString}", userChoice);

                    if(reader.Read())
                    {
                        id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string habitName = reader.GetString(reader.GetOrdinal($"{HabitNameString}"));
                        int numOfOccurences = reader.GetInt32(reader.GetOrdinal($"{NumOfOccurencesString}"));
                        DateTime date = reader.GetDateTime(reader.GetOrdinal($"{DateString}"));

                        Console.WriteLine("\n----Habit To Update----");
                        Console.WriteLine($"\n ID: {id}, HabitName: {habitName}, NumberOfOccurences: {numOfOccurences}, date: {date}");
                    }
                }
            }

            var habitRecord = CreatingNewHabitRow();
            UpdateHabitRecord(id, habitRecord);
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

    private static void UpdateHabitRecord(int Id, HabitRecord habitRecord)
    {
        DateTime date;

        // Assign the date based on whether we are using the current date or the date defined by the user
        if(habitRecord.UseCurrentDate)
        {
            date = habitRecord.CurrentDate!.Value;
        }
        else
        {
            date = new DateTime(habitRecord.Year!.Value, habitRecord.Month!.Value, habitRecord.Day.Value);
        }

        try
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                string query = $"UPDATE HabitLogs SET HabitName = @{HabitNameString}, NumberOfOccurences = @{NumOfOccurencesString}, Date = @{DateString} WHERE Id = @{IdString}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{IdString}", Id);
                    cmd.Parameters.AddWithValue($"@{HabitNameString}", habitRecord.HabitName);
                    cmd.Parameters.AddWithValue($"@{NumOfOccurencesString}", habitRecord.NumberOfOccurences);
                    cmd.Parameters.AddWithValue($"@{DateString}", date);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) updated.");
                }
            }
        }
        catch(SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Unexpected Error:" + ex.Message);
        }
    }

    private static void DeleteHabit()
    {
        Console.WriteLine("---Deleting a habit in the databse----");
        int userChoice;
        int id = 0;

        // Displaying all the habits in the database
        var numberOfHabits = ReadHabitRecords();

        Console.WriteLine($"\nWhich habit do you want to delete between habit 1 and {numberOfHabits}, enter a number: ");
        var isValidHabitNum = Console.ReadLine();

        while(!int.TryParse(isValidHabitNum, out userChoice) || userChoice < 1 || userChoice > numberOfHabits)
        {
            Console.WriteLine($"Invalid choice. Choose a habit between Habit 1 and habit {numberOfHabits}: ");
            isValidHabitNum = Console.ReadLine();
        }

        // Grab the id and the habit row of the passed in userChoice
        string query = $"SELECT Id, {HabitNameString}, {NumOfOccurencesString}, {DateString} FROM {TableNameString} WHERE Id = @{IdString}";

        try
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                using (SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    cmd.Parameters.AddWithValue($"@{IdString}", id);

                    if(reader.Read())
                    {
                        id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string habitName = reader.GetString(reader.GetOrdinal($"{HabitNameString}"));
                        int numOfOccurences = reader.GetInt32(reader.GetOrdinal($"{NumOfOccurencesString}"));
                        DateTime date = reader.GetDateTime(reader.GetOrdinal($"{DateString}"));

                        Console.WriteLine("\n----Habit To Delete----");
                        Console.WriteLine($"\n ID: {id}, HabitName: {habitName}, NumberOfOccurences: {numOfOccurences}, date: {date}");
                    }
                }
            }

            DeleteHabitRecord(id);
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

    private static void DeleteHabitRecord(int Id)
    {
        try
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = $"DELETE FROM HabitLogs WHERE Id = @{IdString}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                     // Add parameter to prevent SQL injection
                    cmd.Parameters.AddWithValue($"@{IdString}", Id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        Console.WriteLine($"Row with ID {Id} deleted successfully.");
                    else
                        Console.WriteLine($"No row found with ID {Id}.");
                }
            }
        }
        catch(SqliteException ex)
        {
            Console.WriteLine("Database Error: " + ex.Message);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Unexpected Error:" + ex.Message);
        }
    }

    private static HabitRecord CreatingNewHabitRow()
    {
        int day;
        int month;
        int year = DateTime.Now.Year;
        int numberOfOccurrences;

        HabitRecord habitRecord = new HabitRecord();

        DateTime now = DateTime.Now;
        var currentDate = new DateTime(now.Year, now.Month, now.Day);

        // Ask user to enter the name of the habit then the number of occurences then the name
        Console.WriteLine("\nEnter the name of your habit: ");
        var habitName = Console.ReadLine();

        // Add validation later in the future to confirm if a valid habit was passed in
        while(string.IsNullOrWhiteSpace(habitName))
        {
            Console.WriteLine("\nHabit name cannot be null or whitespace");
            Console.WriteLine("Enter the name of your habit: ");
            habitName = Console.ReadLine();
        }

        habitRecord.HabitName = habitName;

        // Ask the user for the number of times they have performed their habit
        Console.WriteLine("\nEnter the number of occurences performed for this habit, cannot be less than 0: ");
        var isValidNumber = Console.ReadLine();

        // While loop check that run if invalid number is typed, or number is valid but less than 0 and larger than 5
        while (!int.TryParse(isValidNumber, out numberOfOccurrences) || numberOfOccurrences < 0)
        {
            Console.WriteLine("\nInvalid Input, enter a number that is greater than or equal to 0");
            isValidNumber = Console.ReadLine();
        }

        habitRecord.NumberOfOccurences = numberOfOccurrences;

        // Get the date of habit for the user hardcode year so it is 2026, give user option to use current date or enter date
        Console.WriteLine("\nDo you want to enter the date for this habit or use the current date");
        Console.WriteLine("-c: Current Date");
        Console.WriteLine("-Any key: User defined date");

        var userChoiceDate = Console.ReadLine();

        while(string.IsNullOrWhiteSpace(userChoiceDate))
        {
            Console.WriteLine("Enter a valid choice: ");
            Console.WriteLine("-c: Current Date");
            Console.WriteLine("-Any key: User defined date");
            userChoiceDate = Console.ReadLine();
        }

        if(userChoiceDate == "c")
        {
            habitRecord.UseCurrentDate = true;
            habitRecord.CurrentDate = currentDate;
        }
        else
        {
            // Ask user to enter the month between 1 and 12 and use a method to get the number of days within that month
            // Later in the future implement displaying the number with the month so it is more clear but not now

            // Month
            Console.WriteLine("Pick a number between 1 and 12 for the month for this habit");
            var isValidMonth = Console.ReadLine();
            while(!int.TryParse(isValidMonth, out month) || month < 1 || month > 12)
            {
                Console.WriteLine("Invalid input, Enter a valid number between 1 and 12");
                isValidMonth = Console.ReadLine();
            }

            var numberOfdaysInMonth = GetDaysInMonth(year, month);

            // Days
            Console.WriteLine($"Enter the day your habit. Pick a number between 1 and {numberOfdaysInMonth}");
            var isValidDay = Console.ReadLine();
            while(!int.TryParse(isValidDay, out day) || day < 1 || day > numberOfdaysInMonth)
            {
                Console.WriteLine($"Invalid day, enter a day between 1 and {numberOfdaysInMonth}");
                isValidDay = Console.ReadLine();
            }

            habitRecord.Day = day;
            habitRecord.Month = month;
            habitRecord.Year = year;
        }

        return habitRecord;
    }



    private static int GetDaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

}