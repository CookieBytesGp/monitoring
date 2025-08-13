using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Common.Interfaces;

namespace Monitoring.Infrastructure.Repositories.Camera
{
    public interface ICameraRepository : IRepository<Monitoring.Domain.Aggregates.Camera.Camera>
    {
        Task<Monitoring.Domain.Aggregates.Camera.Camera?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Monitoring.Domain.Aggregates.Camera.Camera> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByLocationAsync(string location, CancellationToken cancellationToken = default);
        Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByStatusAsync(CameraStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByTypeAsync(CameraType type, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNetworkAsync(string ipAddress, int port, CancellationToken cancellationToken = default);
    }
}
