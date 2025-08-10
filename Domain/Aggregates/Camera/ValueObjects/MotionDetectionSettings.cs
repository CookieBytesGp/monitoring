using Domain.SeedWork;
using FluentResults;

namespace Domain.Aggregates.Camera.ValueObjects
{
    public class MotionDetectionSettings : ValueObject
    {
        public bool IsEnabled { get; private set; }
        public int Sensitivity { get; private set; } // 1-10
        public string DetectionZone { get; private set; } // JSON coordinates

        private MotionDetectionSettings() { }

        private MotionDetectionSettings(bool isEnabled, int sensitivity, string detectionZone = null)
        {
            IsEnabled = isEnabled;
            Sensitivity = sensitivity;
            DetectionZone = detectionZone;
        }

        public static Result<MotionDetectionSettings> Create(bool isEnabled, int sensitivity, string detectionZone = null)
        {
            var result = new Result<MotionDetectionSettings>();

            if (sensitivity < 1 || sensitivity > 10)
            {
                result.WithError("Motion sensitivity must be between 1 and 10");
                return result;
            }

            var settings = new MotionDetectionSettings(isEnabled, sensitivity, detectionZone);
            result.WithValue(settings);
            return result;
        }

        public static MotionDetectionSettings CreateDefault()
        {
            return new MotionDetectionSettings(false, 5);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsEnabled;
            yield return Sensitivity;
            yield return DetectionZone ?? string.Empty;
        }
    }
}
