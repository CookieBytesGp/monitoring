using Monitoring.Domain.SeedWork;

namespace Domain.Aggregates.Page.ValueObjects
{
    public class PageStatus : Enumeration
    {
        public static readonly PageStatus Draft = new(1, "Draft");
        public static readonly PageStatus Published = new(2, "Published");
        public static readonly PageStatus Archived = new(3, "Archived");
        public static readonly PageStatus Scheduled = new(4, "Scheduled");


        private PageStatus()
        {
            
        }
        private PageStatus(int value, string name) : base(value, name)
        {
        }
    }
}
