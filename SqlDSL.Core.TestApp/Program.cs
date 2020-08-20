using System;
using System.Collections.Generic;
using System.Linq;
using SqlDSL.Core.Impl;

namespace SqlDSL.Core.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //TestTSQL();
            TestPLSQL();
            Console.ReadLine();
        }

        private static void TestPLSQL()
        {
            List<IRow> rows = OraDb()
                .Where(InParam.Decimal("id", 4))
                .Select()
                .ExecuteSql("SELECT message from TEST.LOGS WHERE LOG_ID = :id");
            rows.ForEach(row => Console.WriteLine("MESSAGE: x" + row.String("MESSAGE") + "x"));
        }

        private static void TestTSQL()
        {
            Db().ExecuteInsertOrDeleteOrUpdateSQL(@"
IF OBJECT_ID('dbo.TEST_DDL', 'U') IS NULL 
CREATE TABLE [dbo].[TEST_DDL](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[EVEN_XML] [xml] NULL,
	[NOTES] [nvarchar](40) NULL,
	[ASCII_NOTES] [varchar](40) NULL,
	[REMEMBER_WHEN] [datetime] NULL,
	[WHEN_TS] [rowversion] NOT NULL,
 CONSTRAINT [PK_TEST_DDL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

            var ids = Db()
                .Where(InParam.Xml("x", "<PmtInf><PmtInfId>PM51</PmtInfId></PmtInf>")
                    , InParam.String("n", "άστα να πάνε")
                    , InParam.String("an", "asta vrasta")
                    , InParam.DateTime("r", DateTime.Now)
                    //, InParam.Binary("WHEN_TS", )
                )
                .Select(Value.Decimal("ID"))


                .ExecuteSql
                    //.ExecuteInsertOrDeleteOrUpdateSQL
                    (@"INSERT INTO TEST_DDL 
-- OUTPUT inserted.ID
(EVEN_XML, 
NOTES, ASCII_NOTES, REMEMBER_WHEN) 
VALUES (@x, 
@n, @an, @r);
select scope_identity() as ID;
")
                .Select(r => r.Decimal("ID"));
            ids.ToList().ForEach(id => Console.WriteLine($"id: {id}"));
            //List<IRow> rows = Db()
            //    .Select()
            //    .ExecuteSql("SELECT [ID] FROM[dbo].[PAYINFOBLOCKS]");
            //rows.ForEach(row => Console.WriteLine(row.Decimal("ID")));
        }

        private static DataBase Db()
        {
            return new DataBase(@"Server=localhost\SQLEXPRESS;Database=helter_skelter;Trusted_Connection=True;");
        }

        private static DataBase OraDb()
        {
            return new DataBase("Data Source=127.0.0.1:1521/orcl;User Id=luser;Password=passwork;"
                , databaseEngineFactory: new DatabaseEngineFactory("PLSQL", null));
        }
    }
}
