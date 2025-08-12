using Monitoring.Domain.Aggregates.Camera;
using Microsoft.EntityFrameworkCore;
using Monitoring.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitoring.Infrastructure.Persistence;
using Domain.Aggregates.Camera.ValueObjects;

namespace Persistence.Camera
{
    public class CameraRepository : Repository<Monitoring.Domain.Aggregates.Camera.Camera>, ICameraRepository
    {
        public CameraRepository(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
        {

        }

        public async Task<Monitoring.Domain.Aggregates.Camera.Camera?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(c => c.Configuration)
                .Include(c => c.Location)
                .Include(c => c.Network)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Monitoring.Domain.Aggregates.Camera.Camera> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(current => current.Name == name, cancellationToken);
        }

        public async Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByLocationAsync(string location, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Location.Value.Contains(location))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByStatusAsync(CameraStatus status, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Monitoring.Domain.Aggregates.Camera.Camera>> GetByTypeAsync(CameraType type, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Type == type)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AnyAsync(current => current.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsWithNetworkAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .AnyAsync(current => current.Network.IpAddress == ipAddress && current.Network.Port == port, cancellationToken);
        }
    }
}
