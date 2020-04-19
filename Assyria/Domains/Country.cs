using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace Assyria.Domains
{
    public class Country
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        
        public virtual IList<Office> Offices { get; set; } = new List<Office>();
    }

    public class CountryMapping : ClassMap<Country>
    {
        public CountryMapping()
        {
            Table("countries");
            Id(country => country.Id);
            Map(country => country.Name);
            HasMany(country => country.Offices).Inverse().Cascade.SaveUpdate();
        }
    }
}