
namespace HabbitLogger;
public class HabitRecord
{
    public string HabitName { get; set; } = string.Empty;
    public int NumberOfOccurences { get; set; }
    public bool UseCurrentDate { get; set; }
    public int? Day { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public DateTime? CurrentDate { get; set;}
}
