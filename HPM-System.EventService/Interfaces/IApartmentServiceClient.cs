namespace HPM_System.EventService.Interfaces
{
    public interface IApartmentServiceClient
    {
        Task<List<Guid>> GetHouseUserIdsAsync(long houseId, CancellationToken ct = default);
    }
}
