using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera
{
    /// <summary>
    /// فکتوری برای انتخاب و مدیریت استراتژی‌های اتصال دوربین
    /// </summary>
    public class CameraStrategyFactory : ICameraStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CameraStrategyFactory> _logger;
        private readonly Dictionary<string, ICameraConnectionStrategy> _strategies;
        private readonly object _lockObject = new object();

        public CameraStrategyFactory(
            IServiceProvider serviceProvider,
            ILogger<CameraStrategyFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _strategies = new Dictionary<string, ICameraConnectionStrategy>(StringComparer.OrdinalIgnoreCase);
            
            InitializeStrategies();
        }

        #region Public Methods

        public async Task<Result<ICameraConnectionStrategy>> GetBestStrategyAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Finding best strategy for camera {CameraName} of type {CameraType}", 
                    camera.Name, camera.Type?.Name);

                if (camera == null)
                    return Result.Fail<ICameraConnectionStrategy>("Camera cannot be null");

                // مرحله 1: دریافت استراتژی‌های سازگار
                var supportedStrategiesResult = await GetSupportedStrategiesAsync(camera);
                if (supportedStrategiesResult.IsFailed)
                    return Result.Fail<ICameraConnectionStrategy>(supportedStrategiesResult.Errors);

                var supportedStrategies = supportedStrategiesResult.Value.ToList();
                if (!supportedStrategies.Any())
                    return Result.Fail<ICameraConnectionStrategy>("No supported strategies found for camera");

                // مرحله 2: تست استراتژی‌ها برای یافتن بهترین گزینه
                var workingStrategiesResult = await TestAllStrategiesAsync(camera);
                if (workingStrategiesResult.IsFailed || !workingStrategiesResult.Value.Any())
                {
                    // اگر هیچ استراتژی کار نکرد، بالاترین اولویت را برگردان
                    var highestPriorityStrategy = supportedStrategies
                        .OrderByDescending(s => s.Priority)
                        .First();
                    
                    _logger.LogWarning("No working strategies found, returning highest priority strategy {StrategyName}", 
                        highestPriorityStrategy.StrategyName);
                    
                    return Result.Ok(highestPriorityStrategy);
                }

                // مرحله 3: انتخاب بهترین استراتژی
                var bestStrategy = workingStrategiesResult.Value.First();
                
                _logger.LogInformation("Selected best strategy {StrategyName} for camera {CameraName}", 
                    bestStrategy.StrategyName, camera.Name);

                return Result.Ok(bestStrategy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding best strategy for camera {CameraName}", camera.Name);
                return Result.Fail<ICameraConnectionStrategy>($"Error finding best strategy: {ex.Message}");
            }
        }

        public Result<ICameraConnectionStrategy> GetStrategyByName(string strategyName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strategyName))
                    return Result.Fail<ICameraConnectionStrategy>("Strategy name cannot be null or empty");

                lock (_lockObject)
                {
                    if (_strategies.TryGetValue(strategyName, out var strategy))
                    {
                        _logger.LogDebug("Found strategy {StrategyName}", strategyName);
                        return Result.Ok(strategy);
                    }
                }

                _logger.LogWarning("Strategy {StrategyName} not found", strategyName);
                return Result.Fail<ICameraConnectionStrategy>($"Strategy '{strategyName}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting strategy {StrategyName}", strategyName);
                return Result.Fail<ICameraConnectionStrategy>($"Error getting strategy: {ex.Message}");
            }
        }

        public IEnumerable<ICameraConnectionStrategy> GetAllStrategies()
        {
            lock (_lockObject)
            {
                return _strategies.Values.ToList();
            }
        }

        public async Task<Result<IEnumerable<ICameraConnectionStrategy>>> GetSupportedStrategiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                if (camera == null)
                    return Result.Fail<IEnumerable<ICameraConnectionStrategy>>("Camera cannot be null");

                var supportedStrategies = new List<ICameraConnectionStrategy>();

                lock (_lockObject)
                {
                    foreach (var strategy in _strategies.Values)
                    {
                        if (strategy.SupportsCamera(camera))
                        {
                            supportedStrategies.Add(strategy);
                            _logger.LogDebug("Strategy {StrategyName} supports camera {CameraName}", 
                                strategy.StrategyName, camera.Name);
                        }
                    }
                }

                // مرتب‌سازی بر اساس اولویت
                var orderedStrategies = supportedStrategies
                    .OrderByDescending(s => s.Priority)
                    .ToList();

                _logger.LogInformation("Found {Count} supported strategies for camera {CameraName}: {Strategies}",
                    orderedStrategies.Count, camera.Name, 
                    string.Join(", ", orderedStrategies.Select(s => s.StrategyName)));

                await Task.CompletedTask;
                return Result.Ok<IEnumerable<ICameraConnectionStrategy>>(orderedStrategies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported strategies for camera {CameraName}", camera.Name);
                return Result.Fail<IEnumerable<ICameraConnectionStrategy>>($"Error getting supported strategies: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<ICameraConnectionStrategy>>> TestAllStrategiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                if (camera == null)
                    return Result.Fail<IEnumerable<ICameraConnectionStrategy>>("Camera cannot be null");

                _logger.LogInformation("Testing all strategies for camera {CameraName}", camera.Name);

                var supportedStrategiesResult = await GetSupportedStrategiesAsync(camera);
                if (supportedStrategiesResult.IsFailed)
                    return Result.Fail<IEnumerable<ICameraConnectionStrategy>>(supportedStrategiesResult.Errors);

                var workingStrategies = new List<(ICameraConnectionStrategy Strategy, int Priority, TimeSpan ResponseTime)>();

                foreach (var strategy in supportedStrategiesResult.Value)
                {
                    try
                    {
                        _logger.LogDebug("Testing strategy {StrategyName} for camera {CameraName}", 
                            strategy.StrategyName, camera.Name);

                        var startTime = DateTime.UtcNow;
                        var testResult = await strategy.TestConnectionAsync(camera);
                        var responseTime = DateTime.UtcNow - startTime;

                        if (testResult.IsSuccess && testResult.Value)
                        {
                            workingStrategies.Add((strategy, strategy.Priority, responseTime));
                            _logger.LogInformation("Strategy {StrategyName} test passed for camera {CameraName} (Response time: {ResponseTime}ms)",
                                strategy.StrategyName, camera.Name, responseTime.TotalMilliseconds);
                        }
                        else
                        {
                            _logger.LogWarning("Strategy {StrategyName} test failed for camera {CameraName}: {Error}",
                                strategy.StrategyName, camera.Name, string.Join(", ", testResult.Errors.Select(e => e.Message)));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error testing strategy {StrategyName} for camera {CameraName}",
                            strategy.StrategyName, camera.Name);
                    }
                }

                // مرتب‌سازی بر اساس اولویت و سپس زمان پاسخ
                var orderedWorkingStrategies = workingStrategies
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.ResponseTime)
                    .Select(x => x.Strategy)
                    .ToList();

                _logger.LogInformation("Found {Count} working strategies for camera {CameraName}: {Strategies}",
                    orderedWorkingStrategies.Count, camera.Name,
                    string.Join(", ", orderedWorkingStrategies.Select(s => s.StrategyName)));

                return Result.Ok<IEnumerable<ICameraConnectionStrategy>>(orderedWorkingStrategies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing strategies for camera {CameraName}", camera.Name);
                return Result.Fail<IEnumerable<ICameraConnectionStrategy>>($"Error testing strategies: {ex.Message}");
            }
        }

        public void RegisterStrategy(ICameraConnectionStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            lock (_lockObject)
            {
                _strategies[strategy.StrategyName] = strategy;
                _logger.LogInformation("Registered strategy {StrategyName} with priority {Priority}",
                    strategy.StrategyName, strategy.Priority);
            }
        }

        public bool UnregisterStrategy(string strategyName)
        {
            if (string.IsNullOrWhiteSpace(strategyName))
                return false;

            lock (_lockObject)
            {
                var removed = _strategies.Remove(strategyName);
                if (removed)
                {
                    _logger.LogInformation("Unregistered strategy {StrategyName}", strategyName);
                }
                return removed;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// مقداردهی اولیه استراتژی‌ها از DI Container
        /// </summary>
        private void InitializeStrategies()
        {
            try
            {
                _logger.LogInformation("Initializing camera strategies from DI container");

                // دریافت تمام استراتژی‌های ثبت شده در DI
                var strategies = _serviceProvider.GetServices<ICameraConnectionStrategy>();

                foreach (var strategy in strategies)
                {
                    RegisterStrategy(strategy);
                }

                _logger.LogInformation("Initialized {Count} camera strategies: {Strategies}",
                    _strategies.Count, string.Join(", ", _strategies.Keys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing camera strategies");
                throw;
            }
        }

        #endregion
    }
}
