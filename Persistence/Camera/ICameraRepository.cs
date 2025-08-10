using Application.Interfaces;

namespace Persistence.Camera
{
    public interface ICameraRepository : IRepository<Domain.Aggregates.Camera.Camera>
    {
        Task<Domain.Aggregates.Camera.Camera> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByLocationAsync(string location, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByStatusAsync(Domain.Aggregates.Camera.CameraStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByTypeAsync(Domain.Aggregates.Camera.CameraType type, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNetworkAsync(string ipAddress, int port, CancellationToken cancellationToken = default);
    }
}
