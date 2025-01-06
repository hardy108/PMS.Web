using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using PMS.Shared.Utilities;
//using System.Configuration;

namespace PMS.EFCore.Helper
{

    public class DBContextOption<T>
        where T:DbContext
    {
        public static DbContextOptions<T> GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<T>(), connectionString).Options;
        }
        //public static DbContextOptions<T> GetOptionsByConnectionName(string connectionName, string encryptionKey)
        //{
        //    return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<T>(), GetConnectionStringByName(connectionName, encryptionKey)).Options;
        //}
        public static DbContextOptions<T> GetOptions(string connectionString, string encryptionKey)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<T>(), GetConnectionString(connectionString,encryptionKey)).Options;
        }

        public static DbContextOptions<T> GetOptions(string dbSource, string dbName, string userId, string password, string encryptionKey)
        {
           return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<T>(),
               GetConnectionString(dbSource,dbName,userId,password,encryptionKey)).Options;
        }

        public static string GetConnectionString(string connectionString, string encryptionKey)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {   
                builder.UserID = PMSEncryption.Decrypt(builder.UserID, encryptionKey);
                builder.Password = PMSEncryption.Decrypt(builder.Password, encryptionKey);
                
            }
            return builder.ConnectionString;
        }

        //public static string GetConnectionStringByName(string connectionName, string encryptionKey)
        //{
        //    string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        //    return GetConnectionString(connectionString, encryptionKey);
        //}

        public static string GetConnectionString(string dbSource, string dbName, string userId, string password, string encryptionKey)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = dbSource;
            builder.IntegratedSecurity = false;
            builder.InitialCatalog = dbName;
            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {
                builder.UserID = PMSEncryption.Decrypt(userId, encryptionKey);
                builder.Password = PMSEncryption.Decrypt(password, encryptionKey);
            }
            else
            {
                builder.UserID = userId;
                builder.Password = password;
            }
            return builder.ConnectionString;
        }

    }
}
