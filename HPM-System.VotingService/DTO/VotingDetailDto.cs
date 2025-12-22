namespace DTO;

public class VotingDetailDto
{
    public Guid Id { get; set; }
    public string QuestionPut { get; set; } = string.Empty;
    public List<string> ResponseOptions { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public string? Decision { get; set; }

    // Статистика
    public int TotalParticipants { get; set; }
    public int VotedCount { get; set; }
    public bool HasDecision { get; set; }

    // Информация о текущем пользователе (если он участник)
    public bool IsParticipant { get; set; }
    public bool HasVoted { get; set; }
    public string? UserResponse { get; set; }
    public long? UserApartmentId { get; set; }
}