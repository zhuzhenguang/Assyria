using System;
using FluentNHibernate.Mapping;

namespace Assyria.Domains
{
    public class Email
    {
        public virtual Guid Id { get; set; }
        public virtual string Content { get; set; }
    }
    
    public class EmailMapping : ClassMap<Email>
    {
        public EmailMapping()
        {
            Table("emails");
            Id(user => user.Id).GeneratedBy.GuidComb();
            Map(user => user.Content);
        }
    }
}