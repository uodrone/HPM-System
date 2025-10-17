namespace HPM_System.EventService.Repositories
{
    public interface IRepository<T>
    {
        public Task<int> AddAsync(T value, CancellationToken ct);
        public Task UpdateAsync(T value, CancellationToken ct);
        public Task DeleteAsync(T value, CancellationToken ct);
        public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
        public Task<T?> GetByIdAsync(long id, CancellationToken ct);
    }
}
