using System;

namespace ActivityScheduler.Data.DataAccess
{
    public class BaseEntity
    {
        public Guid Id { get; set; }

        public BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedDateTime = DateTime.Now;
        }

        public string? Tag { get; set; }
        public string ShortTimeStamp { get { return $"{CreatedDateTime.ToShortDateString()} {CreatedDateTime.ToShortTimeString()}"; } }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastModifiedDatTime { get; set; }

        public virtual BaseEntity Clone()
        {
            return new BaseEntity() { };
        }

    }
}