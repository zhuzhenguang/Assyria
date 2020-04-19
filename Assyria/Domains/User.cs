using FluentNHibernate.Mapping;

namespace Assyria.Domains
{
    public class User
    {
        public virtual long Id { get;set; }
        public virtual string Name { get; set; }
    }
    
    public class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Table("users");
            Id(user => user.Id);
            Map(user => user.Name);
        }
    }
}