using System.ComponentModel.DataAnnotations;

namespace VotingService.Models;

public class Voting
{
    public Guid Id { get; set; }

    [Required]
    public string QuestionPut { get; set; } = string.Empty;

    public List<string> ResponseOptions { get; set; } = new();

    public List<Owner> OwnersList { get; set; } = new();

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsCompleted { get; set; }

    // поле для хранения решения комиссии
    public string? Decision { get; set; }
}