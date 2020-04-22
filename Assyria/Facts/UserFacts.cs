using System.Collections.Generic;
using Assyria.Domains;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace Assyria.Facts
{
    public class UserFacts : TestBase
    {
        private readonly ITestOutputHelper testOutputHelper;

        public UserFacts(ITestOutputHelper output, ITestOutputHelper testOutputHelper) : base(output)
        {
            this.testOutputHelper = testOutputHelper;
        }

        /*
		 * 1. save transient user
		 * 2. save persistent user
		 * 3. save detached user
		 */
        [Fact]
        public void should_save_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                var userId = (long) session.Save(user);

                #region Assertion

                Assert.True(userId > 0);
                Assert.True(user.Id > 0);
                VerifyUserName(userId, "Zhu");

                #endregion
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";
                session.Save(user);

                #region Assertion

                VerifyUserName(user.Id, "Zhu");

                #endregion
            }

            using (ISession session = OpenSession())
            {
                user.Name = "Guang";
                session.Save(user);

                #region Assertion

                VerifyUserName(user.Id, "Zhu");
                VerifyUsersCount(2);

                #endregion
            }
        }

        [Fact]
        public void when_open_a_transaction()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            using (session.BeginTransaction())
            {
                long id = (long) session.Save(user);
                Assert.True(id > 0);

                #region Assertion

                session.Transaction.Commit();
                // VerifyUserName(id, "Zhu");

                #endregion
            }
        }

        /*
         * 1. persistent transient user
         * 2. persistent persistent user
         * 3. persistent detached user
         */
        [Fact]
        public void should_persistent_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.Persist(user);
                Assert.NotEqual(0, user.Id);
                VerifyUserName(user.Id, "Zhu");
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";
                session.Persist(user);

                #region Assertion

                VerifyUserName(user.Id, "Zhu");

                #endregion
            }

            using (ISession session = OpenSession())
            {
                user.Name = "Guang";

                #region Assertion

                var exception = Assert.Throws<PersistentObjectException>(() => session.Persist(user));
                Assert.Equal("detached entity passed to persist: Assyria.Domains.User", exception.Message);

                #endregion
            }
        }

        /*
         * 1. update transient user
         * 2. update persistent user
         * 3. update detached user
         */
        [Fact]
        public void should_update_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.Update(user);

                #region Assertion

                VerifyUsersCount(0);
                var exception = Assert.Throws<StaleStateException>(() => session.Flush());
                Assert.Equal(
                    "Batch update returned unexpected row count from update; actual row count: 0; expected: 1",
                    exception.Message);

                #endregion
            }

            using (ISession session = OpenSession())
            {
                session.Save(user);
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";

                session.Flush();

                VerifyUserName(user.Id, "Zhen");
            }

            var detachedUser = new User {Id = user.Id, Name = "Guang"};
            using (ISession session = OpenSession())
            {
                session.Update(detachedUser);

                session.Flush();
                VerifyUserName(detachedUser.Id, "Guang");
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                session.Delete(user);
                session.Flush();

                session.Update(user);

                #region Assertion

                var exception = Assert.Throws<StaleStateException>(() => session.Flush());
                Assert.Equal(
                    "Batch update returned unexpected row count from update; actual row count: 0; expected: 1",
                    exception.Message);

                #endregion
            }
        }

        /*
         * 1. save or update transient user
         * 2. save or update persistent user
         * 3. save or update detached user
         */
        [Fact]
        public void should_save_or_update_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.SaveOrUpdate(user);
                Assert.NotEqual(0, user.Id);
                VerifyUserName(user.Id, "Zhu");
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                user.Name = "Zhen";

                session.SaveOrUpdate(user);

                #region Assertion

                VerifyUserName(user.Id, "Zhu");

                #endregion
            }

            using (ISession session = OpenSession())
            {
                session.SaveOrUpdate(user);

                user.Name = "Guang";
                session.Flush();

                #region Assertion

                Assert.Equal("Guang", user.Name);

                #endregion
            }
        }

        [Fact]
        public void should_lock_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                var exception = Assert.Throws<TransientObjectException>(() => session.Lock(user, LockMode.None));
                Assert.Equal("cannot lock an unsaved transient instance: Hibernate_PersistenceApi.User",
                    exception.Message);
            }

            using (ISession session = OpenSession())
            {
                session.Save(user);
            }

            using (ISession session = OpenSession())
            {
                session.Lock(user, LockMode.None);

                user.Name = "Zhen";
                session.Flush();

                #region Assertion

                VerifyUserName(user.Id, "Zhen");

                #endregion
            }
        }

        /*
         * 1. merge transient user
         * 2. merge persistent user
         * 3. merge detached user
         */
        [Fact]
        public void should_merge_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                User persistentUser = session.Merge(user);

                #region Assertion

                Assert.Equal(0, user.Id);
                Assert.NotEqual(0, persistentUser.Id);
                VerifyUserName(persistentUser.Id, "Zhu");

                #endregion
            }

            using (ISession session = OpenSession())
            {
                user = session.QueryOver<User>().SingleOrDefault();
                User anotherPersistentUser = session.Merge(user);

                #region Assertion

                Assert.Same(user, anotherPersistentUser);

                #endregion
            }

            var request = new User
            {
                Id = user.Id,
                Name = "Zhen"
            };

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                testOutputHelper.WriteLine($"original user name is {user.Name}");

                #region Assertion

                var exception = Assert.Throws<NonUniqueObjectException>(() => session.Update(request));
                Assert.Equal(
                    "a different object with the same identifier value was already associated with the session: 1, of entity: Hibernate_PersistenceApi.User",
                    exception.Message);

                #endregion
            }

            using (ISession session = OpenSession())
            {
                user = session.Load<User>(user.Id);
                testOutputHelper.WriteLine($"original user name is {user.Name}");

                session.Merge(request);
                session.Flush();

                #region Assertion

                VerifyUserName(user.Id, "Zhen");

                #endregion
            }
        }

        [Fact]
        public void should_delete_user()
        {
            var user = new User {Name = "Zhu"};

            using (ISession session = OpenSession())
            {
                session.Save(user);
                session.Delete(user);
                session.Flush();

                VerifyUsersCount(0);
                Assert.NotEqual(0, user.Id);
                Assert.False(session.Contains(user));
            }

            using (ISession session = OpenSession())
            {
                session.Save(user);
            }

            using (ISession session = OpenSession())
            {
                session.Delete(user);
                session.Flush();

                #region Assertion

                VerifyUsersCount(0);
                Assert.NotEqual(0, user.Id);
                Assert.False(session.Contains(user));

                #endregion
            }

            using (ISession session = OpenSession())
            {
                session.Delete(new User {Name = "Zhen"});
                session.Flush();
            }
        }

        [Fact]
        public void should_be_flush()
        {
            var user = new User {Name = "Zhu"};
            using (ISession session = OpenSession())
            {
                session.Save(user);
            }

            using (ISession session = OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.FlushMode = FlushMode.Auto;
                session.Update(user);

                user.Name = "Zhen";

                transaction.Commit();
            }

            VerifyUserName(user.Id, "Zhen");

            /*using (ISession session = OpenSession())
            {
                session.FlushMode = FlushMode.Auto;
                session.Update(user);

                user.Name = "Guang";

                List<User> users = session.Query<User>().Where(u => u.Name == "Guang").ToList();
                VerifyUserName(user.Id, "Guang");
            }*/
        }

        private void VerifyUserName(long id, string expectedName)
        {
            using ISession session = OpenSession();
            var user = session.Load<User>(id);
            Assert.Equal(expectedName, user.Name);
        }

        private void VerifyUsersCount(int count)
        {
            using ISession session = OpenSession();
            IList<User> users = session.QueryOver<User>().List();
            Assert.Equal(count, users.Count);
        }
    }
}