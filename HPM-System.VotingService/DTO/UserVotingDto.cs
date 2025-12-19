namespace DTO;

public class UserVotingDto
{
    public Guid VotingId { get; set; }
    public string QuestionPut { get; set; } = string.Empty;
    public DateTime EndTime { get; set; }
    public bool IsCompleted { get; set; }

    // Ответ пользователя (может быть null, если не голосовал)
    public string? Response { get; set; }

    // Новые поля
    public int TotalParticipants { get; set; }  // Общее количество участников
    public int VotedCount { get; set; }         // Количество проголосовавших
    public bool HasDecision { get; set; }       // Вынесено ли решение
}