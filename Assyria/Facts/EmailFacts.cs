using System;
using Assyria.Domains;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace Assyria.Facts
{
    public class EmailFacts : TestBase
    {
        public EmailFacts(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void should_save_email()
        {
            var email = new Email {Content = "email content"};

            using (ISession session = OpenSession())
            {
                var id = (Guid) session.Save(email);
                session.Flush();
                Assert.NotEqual(Guid.Empty, email.Id);
                Assert.NotEqual(Guid.Empty, id);

                WithNewSession(session => Assert.Equal("email content", session.Load<Email>(id).Content));
            }
        }

        [Fact]
        public void should_not_save_email()
        {
            var email = new Email {Content = "email content"};

            using (ISession session = OpenSession())
            {
                var id = (Guid) session.Save(email);
                Assert.NotEqual(Guid.Empty, email.Id);
                Assert.NotEqual(Guid.Empty, id);

                WithNewSession(session => Assert.Null(session.Get<Email>(id)));
            }
        }
    }
}