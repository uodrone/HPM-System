using VotingService.Models;

namespace VotingService.Validation;

public class VoteContext
{
    public Guid VotingId { get; set; }

    public Guid UserId { get; set; }

    public string Response { get; set; } = string.Empty;

    public Voting? Voting { get; set; }

    public List<Owner>? UserOwners { get; set; }

    public bool IsFromTelegram { get; set; }

    public long? SpecificApartmentId { get; set; }
}