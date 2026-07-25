
using Microsoft.Data.Sqlite;

namespace HabbitLogger;

class Program
{

    private static readonly string connectionString = "Data Source=habbitlogger.db";
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
        CreateHabit();
        ReadHabitRecords();
    }


    public static void CreateHabit()
    {
        string habitName = string.Empty;
        int numberOfOccurrences = 0;
        bool useCurrentDate = false;

        int month = 0;
        int day = 0;
        int year = 2026; // Use code to dynamically get the year, do not hardcode the year

        DateTime now = DateTime.Now;
        var currentDate = new DateTime(now.Year, now.Month, now.Day);

        // Ask user to enter the name of the habit then the number of occurences then the name
        Console.WriteLine("Enter the name of your habit: ");
        habitName = Console.ReadLine();

        while(string.IsNullOrWhiteSpace(habitName))
        {
            Console.WriteLine("\nHabit name cannot be null or whitespace");
            Console.WriteLine("Enter the name of your habit: ");
            habitName = Console.ReadLine();
        }

        // Ask the user for the number of times they have performed their habit
        Console.WriteLine("\nEnter the number of occurences performed for this habit, cannot be less than 0: ");
        var isValidNumber = Console.ReadLine();

        // While loop check that run if invalid number is typed, or number is valid but less than 0 and larger than 5
        while (!int.TryParse(isValidNumber, out numberOfOccurrences) || numberOfOccurrences < 0)
        {
            Console.WriteLine("\nInvalid Input, enter a number that is greater than or equal to 0");
            isValidNumber = Console.ReadLine();
        }

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
            useCurrentDate = true;
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
        }


        if(useCurrentDate)
        {
            // Create a table in the database using the values and the current date using ADO.Net
            InsertHabitRecord(habitName, numberOfOccurrences, currentDate);
        }
        else
        {
            // Create a table in the database using the values and the date the user entered using ADO.Net
            InsertHabitRecord(habitName, numberOfOccurrences, new DateTime(year, month, day));
        }
    }

    // Create
    private static void InsertHabitRecord(string habitName, int numberOfOccurences, DateTime date)
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

    // Read
    private static void ReadHabitRecords()
    {
        Console.WriteLine("\nDisplaying all habits in the database");

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
                        Console.WriteLine($"\nHabitName: {reader[HabitNameString]}, Times Performed: {reader[NumOfOccurencesString]}, Date: {reader[DateString]}");
                    }
                }
            }
        }
    }

    private static void UpdateHabitRecord()
    {
        // Display
    }



    private static int GetDaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

}