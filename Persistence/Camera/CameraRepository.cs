using CBG;
using Domain.Aggregates.Camera;
using DTOs.Camera;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Camera
{
    public class CameraRepository : Repository<Domain.Aggregates.Camera.Camera>, ICameraRepository
    {
        public CameraRepository(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
        {

        }

        public async Task<Domain.Aggregates.Camera.Camera> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(current => current.Name == name, cancellationToken);
        }

        public async Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByLocationAsync(string location, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Location.Value.Contains(location))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByStatusAsync(CameraStatus status, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Domain.Aggregates.Camera.Camera>> GetByTypeAsync(CameraType type, CancellationToken cancellationToken = default)
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

        public async Task<CameraViewModel> GetCameraViewModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(current => current.Id == id)
                .Select(current => new CameraViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    Location = current.Location.Value,
                    LocationZone = current.Location.Zone,
                    IpAddress = current.Network.IpAddress,
                    Port = current.Network.Port,
                    Type = current.Type,
                    Status = current.Status,
                    CreatedAt = current.CreatedAt,
                    LastActiveAt = current.LastActiveAt,
                    IsOnline = current.Status == CameraStatus.Active && 
                              current.LastActiveAt.HasValue &&
                              DateTime.UtcNow.Subtract(current.LastActiveAt.Value).TotalMinutes < 2
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<CameraViewModel>> GetAllCameraViewModelsAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Select(current => new CameraViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    Location = current.Location.Value,
                    LocationZone = current.Location.Zone,
                    IpAddress = current.Network.IpAddress,
                    Port = current.Network.Port,
                    Type = current.Type,
                    Status = current.Status,
                    CreatedAt = current.CreatedAt,
                    LastActiveAt = current.LastActiveAt,
                    IsOnline = current.Status == CameraStatus.Active && 
                              current.LastActiveAt.HasValue &&
                              DateTime.UtcNow.Subtract(current.LastActiveAt.Value).TotalMinutes < 2
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CameraSummaryViewModel>> GetCameraSummariesAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Select(current => new CameraSummaryViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    Location = current.Location.Value,
                    Status = current.Status,
                    Type = current.Type,
                    IsOnline = current.Status == CameraStatus.Active && 
                              current.LastActiveAt.HasValue &&
                              DateTime.UtcNow.Subtract(current.LastActiveAt.Value).TotalMinutes < 2
                })
                .ToListAsync(cancellationToken);
        }
    }
}
