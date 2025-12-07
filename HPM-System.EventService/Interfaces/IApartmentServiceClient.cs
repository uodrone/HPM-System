namespace HPM_System.EventService.Interfaces
{
    public interface IApartmentServiceClient
    {
        Task<List<Guid>> GetHouseOwnerIdsAsync(long houseId, CancellationToken ct = default);
    }
}
