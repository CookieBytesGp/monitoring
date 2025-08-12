using System.Collections.Generic;

namespace Monitoring.Application.Interfaces.Camera;

public interface ICameraStrategyFactory
{
    ICameraStreamStrategy GetStrategy(Domain.Aggregates.Camera.Camera camera);
    List<ICameraStreamStrategy> GetAllStrategies();
}


