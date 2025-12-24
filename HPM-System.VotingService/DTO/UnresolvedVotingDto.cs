namespace DTO;

public class UnresolvedVotingDto
{
    public Guid VotingId { get; set; }
    public string QuestionPut { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCompleted { get; set; }

    // Новые поля
    public int TotalParticipants { get; set; }  // Общее количество участников
    public int VotedCount { get; set; }         // Количество проголосовавших
}