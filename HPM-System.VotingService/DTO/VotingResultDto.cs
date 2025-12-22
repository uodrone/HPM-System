namespace DTO;

public class VotingResultDto
{
    public string QuestionPut { get; set; } = string.Empty;
    public decimal TotalVotedWeight { get; set; }
    public Dictionary<string, double> Responses { get; set; } = new();
    public string Decision { get; set; } = "Решение не опубликовано";
}