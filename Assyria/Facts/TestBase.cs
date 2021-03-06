using System.IO;
using Assyria.Domains;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using Xunit.Abstractions;

namespace Assyria.Facts
{
    public class TestBase
    {
        private readonly ISessionFactory sessionFactory;
        private readonly string dbFile = "/Users/zgzhu/assyria.db";
        protected readonly ITestOutputHelper output;

        protected TestBase(ITestOutputHelper output)
        {
            sessionFactory = Fluently
                .Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(dbFile).ShowSql)
                // .Database(SQLiteConfiguration.Standard.InMemory)
                .Mappings(m => m.FluentMappings.AddFromAssembly(typeof(User).Assembly))
                .ExposeConfiguration(config =>
                {
                    config.SetInterceptor(new SqlStatementInterceptor(output));
                    BuildSchema(config);
                })
                .BuildSessionFactory();
            this.output = output;
        }

        protected ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession();
            session.FlushMode = FlushMode.Manual;
            return session;
        }

        private void BuildSchema(Configuration config)
        {
            if (File.Exists(dbFile)) File.Delete(dbFile);

            var se = new SchemaExport(config);
            se.Create(false, true);
        }
    }

    public class SqlStatementInterceptor : EmptyInterceptor
    {
        private readonly ITestOutputHelper output;

        public SqlStatementInterceptor(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override SqlString OnPrepareStatement(SqlString sql)
        {
            output.WriteLine(sql.ToString());
            return sql;
        }
    }
}