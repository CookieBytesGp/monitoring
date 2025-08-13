using System.Collections.Generic;
using System.Linq;
using Monitoring.Application.Interfaces.Camera;
using Domain.Aggregates.Camera;

namespace Monitoring.Application.Services.Camera
{
    public class CameraStreamStrategyFactory : ICameraStreamStrategyFactory
    {
        private readonly List<ICameraStreamStrategy> _strategies;

        public CameraStreamStrategyFactory(IEnumerable<ICameraStreamStrategy> strategies)
        {
            _strategies = strategies.ToList();
        }

        public ICameraStreamStrategy GetStrategy(Domain.Aggregates.Camera.Camera camera)
        {
            var strategy = _strategies.FirstOrDefault(s => s.SupportsCamera(camera));
            
            if (strategy == null)
            {
                throw new NotSupportedException($"Camera type {camera.Type} is not supported");
            }
            
            return strategy;
        }

        public List<ICameraStreamStrategy> GetAllStrategies()
        {
            return _strategies;
        }
    }
}
