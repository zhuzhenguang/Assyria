using FluentNHibernate.Mapping;

namespace Assyria.Domains
{
    public class Office
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual Country Country { get; set; }
    }
    
    public class OfficeMapping : ClassMap<Office>
    {
        public OfficeMapping()
        {
            Table("offices");
            Id(office => office.Id);
            Map(office => office.Name);
            References(office => office.Country);
        }
    }
}