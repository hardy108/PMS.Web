using AM.EFCore.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PMS.EFCore.Helper;
using AM.EFCore.Models;
using PMS.Shared.Utilities;
using AM.EFCore.Models.Filters;
using Microsoft.Extensions.Configuration;

namespace AM.EFCore.Services
{
    public class AMService
    {
        private readonly AMContextBase _amContext;

        private IDictionary<string, List<string>> _dictLocationIds;
        private List<VAMRELATIONORGANIZATIONVALID> _relationLocations;
        private DateTime _lastTimeLoaded;


        public AMService(AMContextBase amContext)
        {
            _amContext = amContext;
            _dictLocationIds = new Dictionary<string, List<string>>();
            _relationLocations = new List<VAMRELATIONORGANIZATIONVALID>();
            _lastTimeLoaded = new DateTime();
        }


        public List<string> GetAthorizedUserNamesByUnit(string unitId)
        {
            DateTime now = DateTime.Now;
            var locationKeys = _amContext.GetLocationKeys(unitId);
            return
                (
                    from a in _amContext.VAMUSER
                    join b in _amContext.AMUSERLOCATION on a.ID equals b.USERID into b1
                    from b2 in b1.DefaultIfEmpty()
                    where (a.LOCID == "0000" || a.LOCID == "OU" || (b2 != null && locationKeys.Contains(b2.LOCID))) && now >= a.VALIDFROM && now <= a.VALIDTO
                    select a.ALIAS
                ).Distinct().ToList();
        }

        public List<string> GetAthorizedUserNamesByDepartment(string departmentId)
        {
            if (!string.IsNullOrWhiteSpace(departmentId))
                departmentId = departmentId.ToLower();
            return
                (
                    from a in _amContext.VAMUSER
                    join b in _amContext.VAMPROPERTY on a.ID equals b.OBJECTID
                    where b.PROPERTYALIAS.ToLower().Equals("userdepartment") && (b.VALUE.ToLower().Equals(departmentId) || b.VALUE.ToLower().Equals("alldept"))
                    select a.ALIAS
                ).Distinct().ToList();
        }

        public List<string> GetAthorizedUserNamesByPermission(string permissionAlias, string permissionDetailAlias)
        {
            if (!string.IsNullOrWhiteSpace(permissionAlias))
                permissionAlias = permissionAlias.ToLower();
            if (!string.IsNullOrWhiteSpace(permissionDetailAlias))
                permissionDetailAlias = permissionDetailAlias.ToLower();

            var criteria = PredicateBuilder.True<VAMPERMISSIONALLVALID>();
            if (!string.IsNullOrWhiteSpace(permissionDetailAlias))
                criteria = criteria.And(d => d.RELATIONALIAS.ToLower().Equals(permissionDetailAlias));
            if (!string.IsNullOrWhiteSpace(permissionAlias))
                criteria = criteria.And(d => d.PARENTALIAS.ToLower().Equals(permissionAlias));

            return _amContext.VAMPERMISSIONALLVALID
                .Where(criteria)
                .Select(d => d.CHILDALIAS).Distinct().ToList();
        }

        public List<string> GetAthorizedUserNamesByPermissionDetails(string permissionDetailAlias)
        {
            return GetAthorizedUserNamesByPermission(string.Empty, permissionDetailAlias);
        }

        public List<VAMUSER> GetAllUsers()
        {
            return _amContext.VAMUSER.ToList();
        }

        public List<PermissionMatrix> GetPermissionMatrix(string userName, string permission, string permissionDetails)
        {
            if (!string.IsNullOrWhiteSpace(userName))
                userName = userName.ToLower();
            if (!string.IsNullOrWhiteSpace(permission))
                permission = permission.ToLower();
            if (!string.IsNullOrWhiteSpace(permissionDetails))
                permissionDetails = permissionDetails.ToLower();
            var permissionMatrixes = _amContext.VAMPERMISSIONALLVALID
                .Where(d =>
                    (d.CHILDALIAS.ToLower().Equals(userName) || string.IsNullOrWhiteSpace(userName)) &&
                    (d.PARENTALIAS.ToLower().Equals(permission) || string.IsNullOrWhiteSpace(permission)) &&
                    (d.RELATIONALIAS.ToLower().Equals(permissionDetails) || string.IsNullOrWhiteSpace(permissionDetails)))
                .Select(d => new PermissionMatrix { UserName = d.CHILDALIAS, Permission = d.PARENTALIAS, PermissionDetails = d.RELATIONALIAS })
                .ToList();
            return permissionMatrixes;
        }

        public bool IsAuthorizedPermission(string userName, string permissionAlias, string permissionDetailAlias)
        {
            return GetPermissionMatrix(userName, permissionAlias, permissionDetailAlias).Count > 0;
        }



        private void LoadLocationStructures()
        {
            bool needToReload = false;
            if (_lastTimeLoaded == new DateTime())
                needToReload = true;
            else if (_lastTimeLoaded.AddHours(1) < DateTime.Now)
                needToReload = true;

            if (needToReload)
            {
                _dictLocationIds = new Dictionary<string, List<string>>();
                _lastTimeLoaded = Utility.GetServerTime(_amContext);
                _relationLocations = new List<VAMRELATIONORGANIZATIONVALID>();

                _relationLocations =
                    _amContext.VAMRELATIONORGANIZATIONVALID.ToList();

                if (_relationLocations == null)
                    _relationLocations = new List<VAMRELATIONORGANIZATIONVALID>();

                if (!_relationLocations.Any())
                    return;

                List<string> unitIds = new List<string>();
                _relationLocations.Where(d => d.PARENTALIAS.Equals("OU")).ToList().ForEach(ou =>
                {
                    unitIds.AddRange(GetChildrenLocationIds(ou.CHILDID, ou.CHILDALIAS));
                });
                _dictLocationIds.Add("OU", unitIds.Distinct().ToList());
            }

        }

        private List<string> GetChildrenLocationIds(int Id, string alias)
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

        private List<string> GetLocationIds(string locationId)
        {

            var locationIds = locationId.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return _amContext.GetLocationIds(locationIds);
        }

        private List<string> GetLocationIds(List<string> locationIds)
        {


            if (locationIds == null || !locationIds.Any())
                return new List<string>();

            LoadLocationStructures();

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

        private List<string> GetLocationKeys(string unitId)
        {
            LoadLocationStructures();
            List<string> result = new List<string>();
            foreach (var key in _dictLocationIds.Keys)
            {
                if (_dictLocationIds[key].Contains(unitId))
                    result.Add(key);
            }
            return result;
        }

        public List<string> GetAuthorizedLocationIds(string userName, out bool allLocations)
        {

            List<string> results = new List<string>();
            allLocations = false;
            userName = userName.ToLower().Trim();
            //Authorized divisi by users
            VAMUSER user = _amContext.VAMUSER.FirstOrDefault(d => d.ALIAS.ToLower().Equals(userName));
            if (user == null)
                throw new Exception("Invalid username");

            DateTime now = DateTime.Now;
            
            if (!StandardUtility.DateBetween(now,user.VALIDFROM.Value, user.VALIDTO.Value))
                throw new Exception("User tidak aktif");

            string locationId = user.LOCID;
            allLocations = locationId.Equals("0000");
            return GetLocationIds(locationId);

        }

        public VAMUSER GetByUserName(string userName)
        {
            return _amContext.VAMUSER.SingleOrDefault(d => d.ALIAS.Equals(userName));
        }

        public VAMUSER GetByUserNameOrEmail(string userNameOrEmail)
        {
            return _amContext.VAMUSER.SingleOrDefault(d => d.ALIAS.Equals(userNameOrEmail) || d.EMAIL.Equals(userNameOrEmail));
        }

        public IEnumerable<VAMUSER> GetList(FilterUser filter)
        {
            var criteria = PredicateBuilder.True<VAMUSER>();
            if (filter.IsActive.HasValue && filter.IsActive.Value)
                criteria = criteria.And(d => d.VALIDFROM <= DateTime.Now && d.VALIDTO >= DateTime.Now);

            if (!string.IsNullOrWhiteSpace(filter.Username))
                criteria = criteria.And(d => d.ALIAS.Equals(filter.Username));
            if (filter.Aliases.Any())
                criteria = criteria.And(d => filter.Aliases.Contains(d.ALIAS));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                criteria = criteria.And(d => d.EMAIL.Equals(filter.Email));
            if (filter.Emails.Any())
                criteria = criteria.And(d => filter.Email.Contains(d.EMAIL));

            if (!string.IsNullOrWhiteSpace(filter.UsernameOrEmail))
                criteria = criteria.And(d => d.EMAIL.Equals(filter.UsernameOrEmail) || d.ALIAS.Equals(filter.UsernameOrEmail));



            int id = 0;
            if (filter.Id > 0)
                criteria = criteria.And(d => d.ID == id);



            if (filter.Ids.Any())
            {
                if (filter.Ids.Any())
                    criteria = criteria.And(d => filter.Ids.Contains(d.ID));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                criteria = criteria.And
                (d =>
                    d.ALIAS.ContainsIgnoreCase(filter.SearchTerm)
                    || d.NAME.ContainsIgnoreCase(filter.SearchTerm)
                    || d.NIK.ContainsIgnoreCase(filter.SearchTerm)
                    || d.EMAIL.ContainsIgnoreCase(filter.SearchTerm)
                    || d.DESCRIPTION.ContainsIgnoreCase(filter.SearchTerm)
                );

            if (filter.PageSize <= 0)
                return _amContext.VAMUSER.Where(criteria);
            return _amContext.VAMUSER.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }


        public VAMUSER AuthenticateUser(string id, string password, string newPassword, bool isPasswordEncrypted, out AuthenticationResults result)
        {

            if (string.IsNullOrEmpty(id))
            {
                result = AuthenticationResults.AccountDoesNotExist;
                return null;
            }

            var user = GetByUserName(id);


            if (user == null)
            {
                result = AuthenticationResults.AccountDoesNotExist;
                return null;
            }

            string encPassword = isPasswordEncrypted ? password : PMSEncryption.Encrypt(password, Constants.PasswordEncryptionKey);

            return AuthenticateUser(user, encPassword, newPassword, out result);

        }

        public VAMUSER AuthenticateUser(VAMUSER user, string encryptedPassword, string newPassword, out AuthenticationResults result)
        {
            var currDate = Utility.GetServerTime(_amContext);
            result = AuthenticationResults.Ok;

            if (user == null)
            {
                result = AuthenticationResults.AccountDoesNotExist;
                return null;
            }

            if (!(user.VALIDFROM <= currDate && user.VALIDTO >= currDate))
            {
                result = AuthenticationResults.InActiveAcount;
                return user;
            }




            if (user.LOCKED)
            {
                result = AuthenticationResults.LockedAccount;
                return user;
            }



            try
            {

                if (encryptedPassword != user.PASSWORD)
                {
                    var amUser = GetAMUser(user.ID);
                    amUser.INVALID += 1;
                    if (amUser.INVALID >= 20)
                        amUser.LOCKED = true;

                    SaveAMUser(amUser);

                    result = AuthenticationResults.WrongPassword;
                    user.LOCKED = amUser.LOCKED;
                    user.INVALID = amUser.INVALID;
                    return user;
                }




                bool changePassword = !string.IsNullOrWhiteSpace(newPassword);

                if (user.INVALID > 0 || changePassword)
                {
                    var amuser = GetAMUser(user.ID);
                    amuser.INVALID = 0;
                    amuser.LOCKED = false;
                    if (changePassword)
                    {
                        amuser.PASSWORD = PMSEncryption.Encrypt(newPassword, Constants.PasswordEncryptionKey);
                        amuser.PASSEXPIRED = currDate.AddDays(90);
                    }
                    SaveAMUser(amuser);

                    user.LOCKED = amuser.LOCKED;
                    user.INVALID = amuser.INVALID;
                    user.PASSEXPIRED = amuser.PASSEXPIRED;
                    user.PASSWORD = amuser.PASSWORD;
                }
                if (user.PASSEXPIRED <= currDate)
                    result = AuthenticationResults.ExpiredPassword;
                return user;

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        public VAMUSER ChangePasswordByToken(string id, string newPassword, out AuthenticationResults result)
        {
            var currDate = Utility.GetServerTime(_amContext);
            result = AuthenticationResults.Ok;

            if (string.IsNullOrEmpty(id))
            {
                result = AuthenticationResults.AccountDoesNotExist;
                return null;
            }

            var user = GetByUserName(id);
            if (user == null)
            {
                result = AuthenticationResults.AccountDoesNotExist;
                return null;
            }

            if (!(user.VALIDFROM <= currDate && user.VALIDTO >= currDate))
            {
                result = AuthenticationResults.InActiveAcount;
                return user;
            }

            string encryptedNewPassword = PMSEncryption.Encrypt(newPassword, Constants.PasswordEncryptionKey);

            if (user.PASSWORD == encryptedNewPassword)
                throw new Exception("Password baru tidak boleh sama dengan password sebelumnya");


            var amuser = GetAMUser(user.ID);

            amuser.INVALID = 0;
            amuser.LOCKED = false;
            amuser.PASSWORD = encryptedNewPassword;
            amuser.PASSEXPIRED = currDate.AddDays(90);
            SaveAMUser(amuser);


            user.LOCKED = amuser.LOCKED;
            user.INVALID = amuser.INVALID;
            user.PASSEXPIRED = amuser.PASSEXPIRED;
            user.PASSWORD = amuser.PASSWORD;
            return user;
        }

        private AMUSER GetAMUser(int Id)
        {
            return _amContext.AMUSER.SingleOrDefault(d => d.ID.Equals(Id));
        }

        private void SaveAMUser(AMUSER amUser)
        {
            _amContext.Update(amUser);
            _amContext.SaveChanges();
        }

        public Dictionary<string, List<string>> GetPermissionMatrix(string userName, string permission, List<string> permissions)
        {
            var criteria = PredicateBuilder.True<VAMPERMISSIONALL>();

            if (!string.IsNullOrWhiteSpace(permission))
                criteria = criteria.And(d => d.PARENTALIAS.Equals(permission));
            if (permissions != null && permissions.Any())
                criteria = criteria.And(d => permissions.Contains(d.PARENTALIAS));

            criteria = criteria.And(d => d.CHILDALIAS.Equals(userName));

            var allPermissions = _amContext.VAMPERMISSIONALL.Where(criteria).ToList();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            allPermissions.Select(d => d.PARENTALIAS).Distinct().ToList().ForEach(d =>
            {
                result.Add(d, allPermissions.Where(p => p.PARENTALIAS.Equals(d)).Select(e => e.RELATIONALIAS).ToList());
            });
            return result;


        }


        public List<string> GetAuthorizedDepartmentIds(string userName, out bool allDept)
        {

            allDept = false;
            var values =
            (from a in _amContext.VAMUSER
             join b in _amContext.VAMPROPERTY on a.ID equals b.OBJECTID
             where a.ALIAS.Equals(userName) && b.PROPERTYALIAS.ToLower() == "userdepartment"
             select b.VALUE).ToList();

            if (values == null || !values.Any())
                return new List<string>();
            allDept = values.Contains("ALLDEPT");
            return values;
        }
    }
}
