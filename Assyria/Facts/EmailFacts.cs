using System;
using Assyria.Domains;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace Assyria.Facts
{
    /*
     * save transient object when id is Guid
     */
    public class EmailFacts : TestBase
    {
        public EmailFacts(ITestOutputHelper output) : base(output)
        {
        }
        
        [Fact]
        public void should_save_or_not_save_email()
        {
            var email = new Email {Content = "email content"};

            using(ISession session = OpenSession())
            {
                var id = (Guid) session.Save(email);
                session.Flush();
                Assert.NotEqual(Guid.Empty, email.Id);
                Assert.NotEqual(Guid.Empty, id); 
                
                using(ISession innerSession = OpenSession())
                {
                    Assert.Equal("email content", innerSession.Load<Email>(id).Content); 
                }
            }

            using (ISession session = OpenSession())
            {
                var id = (Guid) session.Save(email);
                Assert.NotEqual(Guid.Empty, email.Id);
                Assert.NotEqual(Guid.Empty, id);

                using (ISession innerSession = OpenSession())
                {
                    Assert.Null(innerSession.Get<Email>(id));
                }
            }
        }
    }
}