using Microsoft.Data.Sqlite;
using System.Data;

namespace HabbitLogger;
internal static class HabitController
{

    // Create
    public static void CreateHabit()
    {
        var habitRecord = CreatingNewHabitRow();

        if (habitRecord.UseCurrentDate)
        {
            // Create a table in the database using the values and the current date using ADO.Net
            HabitRepository.CreateHabitRecord(habitRecord.HabitName, habitRecord.NumberOfOccurences, habitRecord.CurrentDate!.Value);
        }
        else
        {
            // Create a table in the database using the values and the date the user entered using ADO.Net
            HabitRepository.CreateHabitRecord(habitRecord.HabitName, habitRecord.NumberOfOccurences, new DateTime(habitRecord.Year!.Value, habitRecord.Month!.Value, habitRecord.Day!.Value));
        }
    }

    // Read
    public static void ReadHabit()
    {
        Console.WriteLine("\nDisplaying all habits in the database");
        HabitRepository.ReadHabitRecords();
    }

    public static void UpdateHabit()
    {
        int id = 0;
        int userChoiceId;
        Console.WriteLine("----Updating a habit in the database----");

        Console.WriteLine($"\nWhich habit do you want to modify enter the ID of the habit: ");
        var isValidHabitNum = Console.ReadLine();

        while (!int.TryParse(isValidHabitNum, out userChoiceId) || userChoiceId < 1)
        {
            Console.WriteLine($"Invalid choice. Enter a valid number for the ID: ");
            isValidHabitNum = Console.ReadLine();
        }

        // Grab the id and the habit row of the passed in userChoice
        string query = $"SELECT Id, {DbConstants.HabitNameString}, {DbConstants.NumOfOccurencesString}, " +
            $"{DbConstants.DateString} FROM {DbConstants.TableNameString} WHERE Id = @{DbConstants.IdString}";

        try
        {
            using (SqliteConnection conn = new SqliteConnection(DbConstants.connectionString))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{DbConstants.IdString}", userChoiceId);


                    using (SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {

                        if (reader.Read())
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id"));
                            string habitName = reader.GetString(reader.GetOrdinal($"{DbConstants.HabitNameString}"));
                            int numOfOccurences = reader.GetInt32(reader.GetOrdinal($"{DbConstants.NumOfOccurencesString}"));
                            DateTime date = reader.GetDateTime(reader.GetOrdinal($"{DbConstants.DateString}"));

                            Console.WriteLine("\n----Habit To Update----");
                            Console.WriteLine($"\n ID: {id}, HabitName: {habitName}, NumberOfOccurences: {numOfOccurences}, date: {date}");
                        }
                    }
                }
            }

            var habitRecord = CreatingNewHabitRow();
            HabitRepository.UpdateHabitRecord(id, habitRecord);
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

    

    public static void DeleteHabit()
    {
        Console.WriteLine("---Deleting a habit in the databse----");
        int id;

        // Displaying all the habits in the database
        ReadHabit();

        Console.WriteLine($"\nWhich habit do you want to delete select the Id: ");
        var isValidId = Console.ReadLine();

        while (!int.TryParse(isValidId, out id) || id < 1)
        {
            Console.WriteLine($"Invalid choice. Choose a valid number for the Id that not less than 0.");
            isValidId = Console.ReadLine();
        }

        // Grab the id and the habit row of the passed in userChoice
        string query = $"SELECT Id, {DbConstants.HabitNameString}, {DbConstants.NumOfOccurencesString}, " +
            $"{DbConstants.DateString} FROM {DbConstants.TableNameString} WHERE Id = @{DbConstants.IdString}";

        try
        {
            using (SqliteConnection conn = new SqliteConnection(DbConstants.connectionString))
            {
                conn.Open();

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{DbConstants.IdString}", id);
                    using (SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id"));
                            string habitName = reader.GetString(reader.GetOrdinal($"{DbConstants.HabitNameString}"));
                            int numOfOccurences = reader.GetInt32(reader.GetOrdinal($"{DbConstants.NumOfOccurencesString}"));
                            DateTime date = reader.GetDateTime(reader.GetOrdinal($"{DbConstants.DateString}"));

                            Console.WriteLine("\n----Habit To Delete----");
                            Console.WriteLine($"\n ID: {id}, HabitName: {habitName}, NumberOfOccurences: {numOfOccurences}, date: {date}");
                        }
                    }
                }
            }

            HabitRepository.DeleteHabitRecord(id);
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
        while (string.IsNullOrWhiteSpace(habitName))
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

        while (string.IsNullOrWhiteSpace(userChoiceDate))
        {
            Console.WriteLine("Enter a valid choice: ");
            Console.WriteLine("-c: Current Date");
            Console.WriteLine("-Any key: User defined date");
            userChoiceDate = Console.ReadLine();
        }

        if (userChoiceDate == "c")
        {
            // Use the current date if the user chooses C
            habitRecord.UseCurrentDate = true;
            habitRecord.CurrentDate = currentDate;
        }
        else
        {
            // Ask user to enter the month between 1 and 12 and use a method to get the number of days within that month
            // Later in the future implement displaying the number with the month so it is more clear but not now

            // Getting the Month
            Console.WriteLine("Pick a number between 1 and 12 for the month for this habit");
            var isValidMonth = Console.ReadLine();
            while (!int.TryParse(isValidMonth, out month) || month < 1 || month > 12)
            {
                Console.WriteLine("Invalid input, Enter a valid number between 1 and 12");
                isValidMonth = Console.ReadLine();
            }

            var numberOfdaysInMonth = GetDaysInMonth(year, month);

            // Getting the Day of the month
            Console.WriteLine($"Enter the day your habit. Pick a number between 1 and {numberOfdaysInMonth}");
            var isValidDay = Console.ReadLine();
            while (!int.TryParse(isValidDay, out day) || day < 1 || day > numberOfdaysInMonth)
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

