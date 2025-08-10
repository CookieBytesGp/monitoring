using Domain.SeedWork;
using FluentResults;
using Domain.Aggregates.Camera.ValueObjects;

namespace Domain.Aggregates.Camera.ValueObjects
{
    public class RecordingSettings : ValueObject
    {
        public bool IsEnabled { get; private set; }
        public RecordingQuality Quality { get; private set; }
        public TimeSpan Duration { get; private set; } // Recording duration per session
        public string StoragePath { get; private set; }

        private RecordingSettings() { }

        private RecordingSettings(bool isEnabled, RecordingQuality quality, TimeSpan duration, string storagePath = null)
        {
            IsEnabled = isEnabled;
            Quality = quality;
            Duration = duration;
            StoragePath = storagePath;
        }

        public static Result<RecordingSettings> Create(bool isEnabled, RecordingQuality quality, TimeSpan duration, string storagePath = null)
        {
            var result = new Result<RecordingSettings>();

            if (duration.TotalSeconds < 1 || duration.TotalHours > 24)
            {
                result.WithError("Recording duration must be between 1 second and 24 hours");
                return result;
            }

            var settings = new RecordingSettings(isEnabled, quality, duration, storagePath);
            result.WithValue(settings);
            return result;
        }

        public static RecordingSettings CreateDefault()
        {
            return new RecordingSettings(false, RecordingQuality.Medium, TimeSpan.FromMinutes(5));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsEnabled;
            yield return Quality;
            yield return Duration;
            yield return StoragePath ?? string.Empty;
        }
    }

    public enum RecordingQuality
    {
        Low = 1,
        Medium = 2,
        High = 3,
        UltraHigh = 4
    }
}
