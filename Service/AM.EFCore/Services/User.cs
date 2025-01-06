using System;
using System.Collections.Generic;

using System.Text;

using PMS.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Helper;
using PMS.Shared.Utilities;
using AM.EFCore.Models;
using AM.EFCore.Data;
using AM.EFCore.Models.Filters;
using PMS.Shared.Models;

namespace AM.EFCore.Services
{

    public enum AuthenticationResults
    {
        SystemError = -1,
        AccountDoesNotExist = 0,
        ExpiredPassword = 4,
        InActiveAcount = 5,
        LockedAccount = 2,
        Ok = 1,
        WrongPassword = 3
    }

    public class User : EntityFactory<AMUSER, VAMUSER, FilterUser, AMContextBase>
    {
        public User(AMContextBase context, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "User";
        }

        public VAMUSER GetByUserName(string userName)
        {
            return _context.VAMUSER.SingleOrDefault(d => d.ALIAS.Equals(userName));
        }

        public VAMUSER GetByUserNameOrEmail(string userNameOrEmail)
        {
            
            return _context.VAMUSER.SingleOrDefault(d => d.ALIAS.Equals(userNameOrEmail) || d.EMAIL.Equals(userNameOrEmail) );
        }


        public override IEnumerable<VAMUSER> GetList(FilterUser filter)
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
            if (filter.Id>0)            
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
                return _context.VAMUSER.Where(criteria);
            return _context.VAMUSER.Where(criteria).GetPaged(filter.PageNo, filter.PageSize).Results;

        }


        public VAMUSER AuthenticateUser(string id, string password, string newPassword,bool isPasswordEncrypted, out AuthenticationResults result)
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

        public VAMUSER AuthenticateUser(VAMUSER user,string encryptedPassword, string newPassword, out AuthenticationResults result)
        {
            var currDate = Utility.GetServerTime(_context);
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
                    var amUser = _context.AMUSER.SingleOrDefault(d=>d.ID == user.ID);
                    amUser.INVALID += 1;
                    if (amUser.INVALID >= 20)
                        amUser.LOCKED = true;

                    SaveUpdate(amUser, user.ALIAS);

                    result = AuthenticationResults.WrongPassword;
                    user.LOCKED = amUser.LOCKED;
                    user.INVALID = amUser.INVALID;
                    return user;
                }

                


                bool changePassword = !string.IsNullOrWhiteSpace(newPassword);
                
                if (user.INVALID > 0 || changePassword)
                {
                    var amuser = GetSingle(user.ID);
                    amuser.INVALID = 0;
                    amuser.LOCKED = false;
                    if (changePassword)
                    {
                        amuser.PASSWORD = PMSEncryption.Encrypt(newPassword, Constants.PasswordEncryptionKey);
                        amuser.PASSEXPIRED = currDate.AddDays(90);
                    }
                    SaveUpdate(amuser, user.ALIAS);

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
            var currDate = Utility.GetServerTime( _context);
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
            

            var amuser = GetSingle(user.ID);
            
            amuser.INVALID = 0;
            amuser.LOCKED = false;
            amuser.PASSWORD = encryptedNewPassword;
            amuser.PASSEXPIRED = currDate.AddDays(90);
            SaveUpdate(amuser, id);

            user.LOCKED = amuser.LOCKED;
            user.INVALID = amuser.INVALID;
            user.PASSEXPIRED = amuser.PASSEXPIRED;
            user.PASSWORD = amuser.PASSWORD;
            return user;
        }

        public List<string> GetAuthorizedLocationIds(string userName,out bool allLocations)
        {

            List<string> results = new List<string>();
            allLocations = false;

            //Authorized divisi by users
            VAMUSER user = GetByUserName(userName);
            if (user == null)
                throw new Exception("Invalid username");

            string locationId = user.LOCID;
            allLocations = locationId.Equals("0000");
            return _context.GetLocationIds(locationId);
            
        }

        public Dictionary<string,List<string>> GetPermissionMatrix(string userName, string permission, List<string> permissions)
        {
            var criteria = PredicateBuilder.True<VAMPERMISSIONALL>();

            if (!string.IsNullOrWhiteSpace(permission))
                criteria = criteria.And(d => d.PARENTALIAS.Equals(permission));
            if (permissions != null && permissions.Any())
                criteria = criteria.And(d => permissions.Contains(d.PARENTALIAS));

            criteria = criteria.And(d => d.CHILDALIAS.Equals(userName));

            var allPermissions = _context.VAMPERMISSIONALL.Where(criteria).ToList();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();            
            allPermissions.Select(d => d.PARENTALIAS).Distinct().ToList().ForEach(d =>
            {
                result.Add(d, allPermissions.Where(p => p.PARENTALIAS.Equals(d)).Select(e =>e.RELATIONALIAS).ToList());
            });
            return result;


        }


        public List<string> GetAuthorizedDepartmentIds(string userName, out bool allDept)
        {

            allDept = false;
            var values =
            (from a in _context.VAMUSER
             join b in _context.VAMPROPERTY on a.ID equals b.OBJECTID
             where a.ALIAS.Equals(userName) && b.PROPERTYALIAS.ToLower() == "userdepartment"
             select b.VALUE).ToList();

            if (values == null || !values.Any())
                return new List<string>();
            allDept = values.Contains("ALLDEPT");            
            return values;
        }


    }

    
}
