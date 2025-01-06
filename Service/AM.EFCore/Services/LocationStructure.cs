using AM.EFCore.Models;
using AM.EFCore.Data;

using PMS.EFCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AM.EFCore.Services
{
    public static class LocationStructure
    {

        private static IDictionary<string, List<string>> _dictLocationIds;
        private static List<VAMRELATIONORGANIZATIONVALID> _relationLocations;
        private static DateTime _lastTimeLoaded = new DateTime();



        public static void LoadLocationStructures(this AMContextBase context)
        {

            _dictLocationIds = new Dictionary<string, List<string>>();
            _lastTimeLoaded = Utility.GetServerTime(context);
            _relationLocations = new List<VAMRELATIONORGANIZATIONVALID>();

            _relationLocations =
                context.VAMRELATIONORGANIZATIONVALID.ToList();

            if (_relationLocations == null)
                _relationLocations = new List<VAMRELATIONORGANIZATIONVALID>();

            if (!_relationLocations.Any())
                return;

            List<string> unitIds = new List<string>();
            _relationLocations.Where(d => d.PARENTALIAS.Equals("OU")).ToList().ForEach(ou => {
                unitIds.AddRange(GetChildrenLocationIds(ou.CHILDID, ou.CHILDALIAS));
            });
            _dictLocationIds.Add("OU", unitIds.Distinct().ToList());

        }

        private static List<string> GetChildrenLocationIds(int Id, string alias)
        {
            List<string> unitIds = new List<string>();
            var chilrend = _relationLocations.Where(d => d.PARENTID == Id).ToList();
            if (chilrend != null && chilrend.Any())
            {
                chilrend.ForEach(child => {
                    unitIds.AddRange(GetChildrenLocationIds(child.CHILDID, child.CHILDALIAS));
                });
            }
            else
            {
                unitIds.Add(alias);
            }
            try
            {
                var location = _dictLocationIds[alias];
                if (location == null)
                    _dictLocationIds.Add(alias, unitIds.Distinct().ToList());
                else
                    _dictLocationIds[alias] = location.Union(unitIds).Distinct().ToList();
                
            }
            catch
            {
                _dictLocationIds.Add(alias, unitIds.Distinct().ToList());
            }
            return unitIds;

        }

        public static List<string> GetLocationIds(this AMContextBase context, string locationId)
        {

            var locationIds = locationId.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return context.GetLocationIds(locationIds);
        }

        public static List<string> GetLocationIds(this AMContextBase context, List<string> locationIds)
        {


            if (locationIds == null || !locationIds.Any())
                return new List<string>();

            if (_lastTimeLoaded.AddHours(1) < DateTime.Now)
                LoadLocationStructures(context);

            if (locationIds.Contains("0000"))
                return _dictLocationIds["OU"];

            List<string> unitIds = new List<string>();
            foreach (string id in locationIds)
            {
                unitIds.Add(id);
                try
                {
                    unitIds.AddRange(_dictLocationIds[id]);
                }
                catch { }
            }
            return unitIds.Distinct().ToList();
        }

        public static List<string> GetLocationKeys(this AMContextBase context, string unitId)
        {
            if (_lastTimeLoaded.AddHours(1) < DateTime.Now)
                LoadLocationStructures(context);
            List<string> result = new List<string>();
            foreach (var key in _dictLocationIds.Keys)
            {
                if (_dictLocationIds[key].Contains(unitId))
                    result.Add(key);
            }
            return result;
        }

    }
}
