namespace HPM_System.VotingService.CustomExceptions
{
    public class AlreadyVotedException : InvalidOperationException
    {
        public string PreviousResponse { get; }

        public AlreadyVotedException(string message, string previousResponse) : base(message)
        {
            PreviousResponse = previousResponse;
        }
    }
}
