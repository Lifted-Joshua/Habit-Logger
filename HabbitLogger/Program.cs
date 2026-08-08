
using System.Data;
using Microsoft.Data.Sqlite;

namespace HabbitLogger;

class Program
{
    
    
    static void Main()
    {
        bool endApp = false;
        int userChoice;

        InitializeDatabase();

        while(!endApp)
        {
            Console.WriteLine("Enter a choice between 1 and 4 or anything else to exit: ");
            Console.WriteLine("1: Create Habit: ");
            Console.WriteLine("2: Read Habit: ");
            Console.WriteLine("3: Update Habit: ");
            Console.WriteLine("4: Delete Habit: ");

            var choice = Console.ReadLine();

            if (int.TryParse(choice, out userChoice) && userChoice >= 1 && userChoice <= 4)
            {
                switch (userChoice)
                {
                    case 1:
                        HabitController.CreateHabit();
                        break;
                    case 2:
                        HabitController.ReadHabit();
                        break;
                    case 3:
                        HabitController.UpdateHabit();
                        break;
                    case 4: HabitController.DeleteHabit();
                        break;
                }
            } 
            else
            {
                Console.WriteLine("Exiting the app. Goodbye");
                endApp = true; 
            }
        }
    }

    static void InitializeDatabase()
    {
        // Creating an sqlLite database
        using var connection = new SqliteConnection(DbConstants.connectionString);

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
    }
}