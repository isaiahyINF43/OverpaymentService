using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceMentorshipTest.Database
{
    public static class InitializeDB
    {
        public static void CreateDBTable()
        {
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS [MyTable] (
                                        [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        [Key] NVARCHAR(2048) NULL,
                                        [Value] VARCHAR(2048) NULL
                                        )";

            string createTableQuery2 = @"CREATE TABLE IF NOT EXISTS [OverPayment] (
                                        [OverPaymentID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        [ClaimNumber] NVARCHAR(20) NOT NULL REFERENCES Claims(ClaimNumber),
                                        [MemberID] INTEGER FOREIGN KEY REFERENCES Members(MembersID),
                                        [BalanceAmt] INTEGER 0,
                                        [OverPaymentAmt] INTEGER 0 NOT NULL,
                                        [SysSrcSyncDate] DATETIME2 GetDate(),
                                        [LastUpdated] DATETIME2 GetDate()
                                        )";

            string createTableQuery3 = @"CREATE TABLE IF NOT EXISTS [Members] (
                                        [MemberID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        [FirstName] NVARCHAR(20) NOT NULL,
                                        [LastName] NVARCHAR(20) NOT NULL,
                                        [Email] NVARCHAR(50) NOT NULL,
                                        [Address] NVARCHAR(20) NOT NULL,
                                        [City] NVARCHAR(20) NOT NULL,
                                        [State] NVARCHAR(2) NOT NULL,
                                        [PostCode] NVARCHAR(10) NOT NULL,
                                        [SysSrcSyncDate] DATETIME2 GetDate()
                                        )";

            string createTableQuery4 = @"CREATE TABLE IF NOT EXISTS [Claims] (
                                        [ClaimNumber] NVARCHAR(20) NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        [SysSrcSyncDate] DATETIME2 GetDate()
                                        )";

            //Configure where db file is created
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            System.Data.SQLite.SQLiteConnection.CreateFile(path + "/databaseFile1.db3");
            using (System.Data.SQLite.SQLiteConnection conn = new System.Data.SQLite.SQLiteConnection("data source=|DataDirectory|/databaseFile1.db3"))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(conn))
                {
                    conn.Open();                             // Open the connection to the database

                    com.CommandText = createTableQuery;     // Set CommandText to our query that will create the table
                    com.ExecuteNonQuery();                  // Execute the query

                    com.CommandText = "INSERT INTO MyTable (Key,Value) Values ('key one','value one')";     // Add the first entry into our database 
                    com.ExecuteNonQuery();      // Execute the query
                    com.CommandText = "INSERT INTO MyTable (Key,Value) Values ('key two','value value')";   // Add another entry into our database 
                    com.ExecuteNonQuery();      // Execute the query

                    com.CommandText = "Select * FROM MyTable";      // Select all rows from our database table

                    using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["Key"] + " : " + reader["Value"]);     // Display the value of the key and value column for every row
                        }
                    }
                    conn.Close();        // Close the connection to the database
                }
            }
        }
    }
}