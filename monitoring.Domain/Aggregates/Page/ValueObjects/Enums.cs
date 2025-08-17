using Monitoring.Domain.SeedWork;

namespace Domain.Aggregates.Page.ValueObjects
{
    /// <summary>
    /// جهت نمایش صفحه - Enumeration
    /// </summary>
    public class DisplayOrientation : Enumeration
    {
        public static readonly DisplayOrientation Portrait = new(1, "Portrait");
        public static readonly DisplayOrientation Landscape = new(2, "Landscape");
        public static readonly DisplayOrientation Square = new(3, "Square");

        private DisplayOrientation()
        {
            
        }
        private DisplayOrientation(int value, string name) : base(value, name) { }
    }
}
