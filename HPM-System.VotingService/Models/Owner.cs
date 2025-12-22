using System.Text.Json.Serialization;

namespace VotingService.Models;

public class Owner
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }          // ID пользователя из UserService
    public long ApartmentId { get; set; }     // ID квартиры
    public long HouseId { get; set; }         // ID дома
    public decimal ApartmentArea { get; set; }
    public decimal Share { get; set; }
    public string Response { get; set; } = string.Empty;
    public decimal VoteWeight { get; set; }

    [JsonIgnore]
    public Voting? Voting { get; set; }
    public Guid VotingId { get; set; }
}