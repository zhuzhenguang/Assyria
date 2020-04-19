using Assyria.Domains;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace Assyria.Facts
{
    public class CountryFacts : TestBase
    {
        public CountryFacts(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void should_save_country()
        {
            var country = new Country {Name = "UK"};
            var office = new Office {Name = "London"};
            
            country.Offices.Add(office);
            office.Country = country;

            using (ISession session = OpenSession())
            {
                session.Save(country);
                Assert.True(country.Id > 0);
            }
        }
    }
}