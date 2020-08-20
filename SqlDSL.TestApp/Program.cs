using System;
using System.Collections.Generic;

namespace SqlDSL.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IRow> rows = new DataBase(@"Server=localhost\SQLEXPRESS;Database=danaos_banks;Trusted_Connection=True;")
                .Select()
                .ExecuteSql("SELECT [ID] FROM[dbo].[PAYINFOBLOCKS]");
            rows.ForEach(row => Console.WriteLine(row.Decimal("ID")));
        }
    }
}
