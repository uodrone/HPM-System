namespace DTO
{
    public class InternalVotingDto
    {
        public Guid Id { get; set; }
        public string QuestionPut { get; set; } = string.Empty;
        public List<string> ResponseOptions { get; set; } = new();
    }
}
