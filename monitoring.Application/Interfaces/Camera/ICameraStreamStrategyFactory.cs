using System.Collections.Generic;

namespace Monitoring.Application.Interfaces.Camera;

public interface ICameraStreamStrategyFactory
{
    ICameraStreamStrategy GetStrategy(Domain.Aggregates.Camera.Camera camera);
    List<ICameraStreamStrategy> GetAllStrategies();
}


