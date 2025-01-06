using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using PMS.EFCore.Helper;
using PMS.Shared.Services;

namespace PMS.EFCore.Services.Utilities
{
    public interface IDbServerServices
    {
        List<DBServer> GetServers(List<string> unitIdOrAliases, List<DBServerType> dbServerTypes);
        List<DBServer> GetServers(List<string> unitIdOrAliases, DBServerType dbServerType);
        List<DBServer> GetServers(string unitIOrAlias, List<DBServerType> dbServerTypes);
        DBServer GetServer(string unitIOrAlias, DBServerType dbServerType);
    }


    
    public class DbServerServices : IDbServerServices
    {
        private List<DBServer> _servers = new List<DBServer>();
        


        public void InitServices(string dbServerFile)
        {
            using (StreamReader stream = System.IO.File.OpenText(dbServerFile))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var x = Newtonsoft.Json.JsonSerializer.Create();
                    _servers.AddRange(x.Deserialize<IEnumerable<DBServer>>(reader));
                }
            }
        }

        public DbServerServices(IHostingEnvironment hostingEnvirontment, IOptions<AppSetting> appSettings)
        {
            string dbServerFile = hostingEnvirontment.ContentRootPath + appSettings.Value.DbServerJsonFile;
            InitServices(dbServerFile);
        }


        public DbServerServices(string dbServerJsonFile)
        {
            InitServices(dbServerJsonFile);
        }

        public List<DBServer> GetServers(List<string> unitIdOrAliases, List<DBServerType> dbServerTypes)
        {


            var criteria = PredicateBuilder.True<DBServer>();
            bool byAlias = unitIdOrAliases != null && unitIdOrAliases.Any(),
                 byType = dbServerTypes != null && dbServerTypes.Any();

            var query = _servers.AsEnumerable();
            if (byAlias)
                query = query.Where(d => unitIdOrAliases.Contains(d.UNITCODE) || unitIdOrAliases.Contains(d.ALIAS));
            if (byType)
                query = query.Where(d => dbServerTypes.Contains(d.DBTYPE));
            return query.ToList();

        }

        public List<DBServer> GetServers(List<string> unitIdOrAliases, DBServerType dbServerType)
        {
            return GetServers(unitIdOrAliases, new List<DBServerType> { dbServerType });
        }

        public List<DBServer> GetServers(string unitIOrAlias, List<DBServerType> dbServerTypes)
        {
            return GetServers(new List<string> { unitIOrAlias }, dbServerTypes);
        }

        public DBServer GetServer(string unitIOrAlias, DBServerType dbServerType)
        {
            return GetServers(new List<string> { unitIOrAlias }, new List<DBServerType> { dbServerType }).FirstOrDefault();
        }
    }
}
