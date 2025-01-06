using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;

using Microsoft.AspNetCore.Http;
using PMS.Shared.Utilities;
using PMS.EFCore.Helper;
using System.IO;
using Newtonsoft.Json;
using PMS.Shared.Models;
using PMS.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using PMS.Shared;
using Microsoft.IdentityModel.JsonWebTokens;
using AM.EFCore.Data;
using AM.EFCore.Services;
using AM.EFCore.Models;
using System.Linq.Expressions;
using System.Net.Mail;

namespace AM.EFCore.Services
{
    //public interface IAuthenticationService
    //{
    //    WebSession Login(string userName, string password);
    //    WebSession LoginByToken(string tokenString);
    //    WebSession LoginByToken(IHeaderDictionary headers);

    //    WebSession LoginBySession(string sessionString);

    //    WebSession GetSession(IHeaderDictionary headers);
    //    WebSession LoginBySession(IHeaderDictionary headers);

    //    void Logout(string tokenString);
    //    void Logout(IHeaderDictionary headers);
    //    List<string> GetAuthorizedMenuIdByUserName(string userName);
    //    List<string> GetAuthorizedMenuId(string tokenString);
    //    List<string> GetAuthorizedMenuId(IHeaderDictionary headers);
    //    List<USERACCESS> GetAuthorizedMenu(string userName, string menuId, List<string> menuIds);
    //    List<USERACCESS> GetAuthorizedMenu(IHeaderDictionary headers, string menuId, List<string> menuIds);


    //    List<VUNIT> GetAuthorizedUnitByUserName(string userName, string unitId);
    //    List<VUNIT> GetAuthorizedUnit(string tokenString, string unitId);
    //    List<VUNIT> GetAuthorizedUnit(IHeaderDictionary headers, string unitId);

    //    bool IsAuthorizedUnit(string userName, string unitId);

    //    Expression<Func<VUNIT, bool>> GetFilterUnitByUserName(string userName, string unitId);

    //    Expression<Func<VDIVISI, bool>> GetFilterDivisiByUserName(string userName, string unitId, string divisiId);
    //    List<VDIVISI> GetAuthorizedDivisiByUserName(string userName, string unitId, string divisiId);
    //    List<VDIVISI> GetAuthorizedDivisi(string tokenString, string unitId, string divisiId);
    //    List<VDIVISI> GetAuthorizedDivisi(IHeaderDictionary headers, string unitId, string divisiId);
    //    bool IsAuthorizedDivisi(string userName, string divisiId);

    //    Expression<Func<VBLOCK, bool>> GetFilterBlockByUserName(string userName, string unitId, string divisiId, string blockId);
    //    List<VBLOCK> GetAuthorizedBlockByUserName(string userName, string unitId, string divisiId, string blockId);
    //    List<VBLOCK> GetAuthorizedBlock(string tokenString, string unitId, string divisiId, string blockId);
    //    List<VBLOCK> GetAuthorizedBlock(IHeaderDictionary headers, string unitId, string divisiId, string blockId);

    //    bool IsAuthorizedBlock(string userName,string blockId);

    //    WebSession ChangePassword(string sessionString, string oldPassword, string newPassword);
    //    WebSession ChangePassword(IHeaderDictionary headers, string oldPassword, string newPassword);

    //    bool ResetPasssordRequest(string userNameOrEmail);

    //    List<string> GetAuthorizedLocationIdByUserName(string userName, out bool allLocations);
    //    List<string> GetAuthorizedDepartmentIdByUserName(string userName, out bool allDepartment);

    //    VAMUSER GetUserInfo(string userNameorEmail);

    //    List<VAMUSER> GetAllUsers();

    //    List<string> GetAthorizedUserNamesByUnit(string unitId);

    //    List<string> GetAthorizedUserNamesByDepartment(string departmentId);
    //    List<string> GetAthorizedUserNamesByPermission(string permissionAlias, string permissionDetailAlias);
    //    List<string> GetAthorizedUserNamesByPermissionDetails(string permissionDetailAlias);

    //    List<PermissionMatrix> GetPermissionMatrix(string userName, string permission, string permissionDetails);

    //    bool IsAuthorizedPermission(string userName, string permissionAlias, string permissionDetailAlias);

    //    bool ResetPassword(string tokenString, string newPassword);
    //}

    public class AuthenticationServiceBase /*: IAuthenticationService*/
    {
        private PMSContextBase _context;
        private AMContextBase _amContext;
        private User _service;
        private IWebSessionService _webSession;
        AppSetting _appSetting;
        readonly IBackgroundTaskQueue _taskQueue;
        readonly IEmailSender _emailSender;
        private AuditContext _auditContext;

        public AuthenticationServiceBase(PMSContextBase context, AMContextBase amContext, AuditContext auditContext, IWebSessionService webSession, IOptions<AppSetting> appSettings, IBackgroundTaskQueue taskQueue, IEmailSender emailSender)
        {

            _context = context;
            _amContext = amContext;
            _service = new User(_amContext, auditContext);
            _webSession = webSession;
            if (appSettings !=null)
                _appSetting = appSettings.Value;
            _taskQueue = taskQueue;
            _emailSender = emailSender;
            _auditContext = auditContext;
        }

        public WebSession Login(string userName, string password)
        {

            string errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("User name atau password anda salah.");

            _auditContext.SaveAuditTrail(userName, "Login", "Try to login");
            AuthenticationResults result;

            var user = _service.AuthenticateUser(userName, password, string.Empty, false, out result);

            throwInvalidAuthorization(userName, password, result);
            _auditContext.SaveAuditTrail(userName, "Login", "Success");

            var webSession = _webSession.NewSession(userName, user.PASSWORD, user.NAME, (result == AuthenticationResults.ExpiredPassword), false);

            return webSession;

        }

        public WebSession GetSession(IHeaderDictionary headers)
        {
            string xHeaders = GetSessionHeader(headers);
            return xHeaders.Deserialize<WebSession>();
        }



        public WebSession LoginByToken(string tokenString)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return null;
            var webSession = _webSession.ValidateSession(tokenString);
            AuthenticationResults result;
            _service.AuthenticateUser(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, string.Empty, true, out result);
            throwInvalidAuthorization(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, result);

            if (result == AuthenticationResults.ExpiredPassword)
            {
                _webSession.RemoveSession(tokenString);
                return _webSession.NewSession(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, webSession.DecodedToken.FullName, true, false);
            }
            return webSession;
        }

        public WebSession LoginBySession(string sessionString)
        {
            if (string.IsNullOrWhiteSpace(sessionString))
                return null;

            WebSession webSession = sessionString.Deserialize<WebSession>();
            AuthenticationResults result;
            _service.AuthenticateUser(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, string.Empty, true, out result);
            throwInvalidAuthorization(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, result);
            if (result == AuthenticationResults.ExpiredPassword)
            {
                _webSession.RemoveSession(webSession.Token);
                return _webSession.NewSession(webSession.DecodedToken.UserName, webSession.DecodedToken.Key, webSession.DecodedToken.FullName, true, false);
            }
            return webSession;
        }

        public WebSession LoginBySession(IHeaderDictionary headers)
        {
            return LoginBySession(GetSessionHeader(headers));
        }

        public WebSession LoginByToken(IHeaderDictionary headers)
        {
            return LoginByToken(GetLoginBearer(headers));
        }
        public void Logout(string tokenString)
        {
            _webSession.RemoveSession(tokenString);
        }

        public void Logout(IHeaderDictionary headers)
        {
            Logout(GetLoginBearer(headers));
        }
        public List<USERACCESS> GetAuthorizedMenu(string sessionString, string menuId, List<string> menuIds)
        {
            string userName = string.Empty;
            var webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedMenuByUserName(webSession.DecodedToken.UserName, menuId, menuIds);
        }

        public List<string> GetAuthorizedReport(string sessionString, string reportId, List<string> reportIds)
        {
            string userName = string.Empty;
            var webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedReportByUserName(webSession.DecodedToken.UserName, reportId,reportIds);
        }


        public List<USERACCESS> GetAuthorizedMenuByUserName(string userName, string menuId, List<string> menuIds)
        {


            var allPermissions = _service.GetPermissionMatrix(userName, menuId, menuIds);
            List<USERACCESS> userAccesses = new List<USERACCESS>();
            foreach (var id in allPermissions.Keys)
            {
                var userAccess = new USERACCESS();
                userAccess.MENUCODE = id;
                userAccess.ACTIVE = true;
                allPermissions[id].ForEach(e => {
                    userAccess.AllPermissions.Add(e);
                    if (e.Equals("Add"))
                        userAccess.FADD = true;
                    else if (e.Equals("Edit"))
                        userAccess.FEDIT = true;
                    else if (e.Equals("Delete"))
                        userAccess.FDEL = true;
                    else if (e.Equals("Approve"))
                        userAccess.FAPPR = true;
                    else if (e.Equals("Cancel"))
                        userAccess.FCANCEL = true;
                    else
                        userAccess.CustomPermissions.Add(e);
                });
                userAccesses.Add(userAccess);

            }



            return userAccesses;


        }

        public List<string> GetAuthorizedReportByUserName(string userName, string reportId, List<string> reportIds)
        {


            var authorizedReportIds = _service.GetPermissionMatrix(userName, "PMS.Reports", new List<string>()).FirstOrDefault().Value;
            if (reportIds == null)
                reportIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(reportId))
                reportIds.Add(reportId);
            if (reportIds.Count == 0)
                return authorizedReportIds;
            var result = new List<string>();
            reportIds.ForEach(d => {
                if (authorizedReportIds.Contains(d))
                    result.Add(d);
            });
            return result;

            
        }

        public List<USERACCESS> GetAuthorizedMenu(IHeaderDictionary headers, string menuId, List<string> menuIds)
        {
            return GetAuthorizedMenu(GetSessionHeader(headers), menuId, menuIds);
        }

        public List<string> GetAuthorizedReport(IHeaderDictionary headers, string reportId, List<string> reportIds)
        {
            return GetAuthorizedReport(GetSessionHeader(headers), reportId, reportIds);
        }




        private string GetLoginBearer(IHeaderDictionary headers)
        {
            string result = string.Empty;

            try
            {
                result = headers["Authorization"].ToArray()[0];
                if (result.ToLower().StartsWith("bearer "))
                    result = result.Substring(7);
                else
                    result = string.Empty;
            }
            catch { result = string.Empty; }
            return result;
        }

        private string GetSessionHeader(IHeaderDictionary headers)
        {
            string result = string.Empty;

            try
            {
                result = headers["X-Session"];
            }
            catch { result = string.Empty; }
            return result;
        }



        public List<string> GetAuthorizedMenuId(string sessionString)
        {
            WebSession webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedMenuIdByUserName(webSession.DecodedToken.UserName);
        }

        public List<string> GetAuthorizedMenuIdByUserName(string userName)
        {

            return (
                    from a in _context.MROLEFORM
                    join b in _context.MUSERMASTER on a.ROLEID equals b.ROLEID
                    where b.USERID.Equals(userName) && a.ACTIVE && b.ACTIVE
                    select a.MENUCODE).Distinct().ToList();
        }
        public List<string> GetAuthorizedMenuId(IHeaderDictionary headers)
        {
            return GetAuthorizedMenuId(GetSessionHeader(headers));
        }



        public Expression<Func<VUNIT, bool>> GetFilterUnitByUserName(string userName, string unitId)
        {
            bool all = false;
            var criteria = PredicateBuilder.True<VUNIT>();
            if (!string.IsNullOrWhiteSpace(unitId))
                criteria = criteria.And(d => d.UNITCODE.Equals(unitId));

            List<string> locationIds = _service.GetAuthorizedLocationIds(userName, out all);
            if (!all)
            {
                var unitIds = _context.VDIVISI.Where(d => locationIds.Contains(d.UNITCODE) || locationIds.Contains(d.DIVID)).Select(d => d.UNITCODE).Distinct().ToList();
                if (unitIds != null && unitIds.Any())
                    criteria = criteria.And(d => locationIds.Contains(d.UNITCODE) || unitIds.Contains(d.UNITCODE));
            }
            return criteria;
        }
        public List<VUNIT> GetAuthorizedUnitByUserName(string userName, string unitId)
        {
            var criteria = GetFilterUnitByUserName(userName, unitId);
            criteria = criteria.And(d => d.ACTIVE.HasValue && d.ACTIVE.Value);
            return _context.VUNIT.Where(criteria).ToList();
        }

        public List<VUNIT> GetAuthorizedUnit(string sessionString, string unitId)
        {

            WebSession webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedUnitByUserName(webSession.DecodedToken.UserName, unitId);
        }

        public List<VUNIT> GetAuthorizedUnit(IHeaderDictionary headers, string unitId)
        {
            return GetAuthorizedUnit(GetSessionHeader(headers), unitId);
        }

        public List<VDIVISI> GetAuthorizedDivisiByUserName(string userName, string unitId, string divisiId)
        {
            var criteria = GetFilterDivisiByUserName(userName, unitId, divisiId);
            criteria = criteria.And(d => d.ACTIVE.HasValue && d.ACTIVE.Value);
            return _context.VDIVISI.Where(criteria).ToList();
        }

        public Expression<Func<VDIVISI, bool>> GetFilterDivisiByUserName(string userName, string unitId, string divisiId)
        {
            bool all = false;
            var criteria = PredicateBuilder.True<VDIVISI>();
            if (!string.IsNullOrWhiteSpace(unitId))
                criteria = criteria.And(d => d.UNITCODE.Equals(unitId));
            if (!string.IsNullOrWhiteSpace(divisiId))
                criteria = criteria.And(d => d.DIVID.Equals(divisiId));

            List<string> locationIds = _service.GetAuthorizedLocationIds(userName, out all);
            if (!all)
                criteria = criteria.And(d => locationIds.Contains(d.UNITCODE) || locationIds.Contains(d.DIVID));

            return criteria;
        }

        public Expression<Func<VBLOCK, bool>> GetFilterBlockByUserName(string userName, string unitId, string divisiId, string blockId)
        {
            bool all = false;
            var criteria = PredicateBuilder.True<VBLOCK>();
            if (!string.IsNullOrWhiteSpace(unitId))
                criteria = criteria.And(d => d.UNITCODE.Equals(unitId));
            if (!string.IsNullOrWhiteSpace(divisiId))
                criteria = criteria.And(d => d.DIVID.Equals(divisiId));
            if (!string.IsNullOrWhiteSpace(blockId))
                criteria = criteria.And(d => d.BLOCKID.Equals(blockId));

            List<string> locationIds = _service.GetAuthorizedLocationIds(userName, out all);
            if (!all)
                criteria = criteria.And(d => locationIds.Contains(d.UNITCODE) || locationIds.Contains(d.DIVID));

            return criteria;
        }

        public List<VDIVISI> GetAuthorizedDivisi(string sessionString, string unitId, string divisiId)
        {

            WebSession webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedDivisiByUserName(webSession.DecodedToken.UserName, unitId, divisiId);
        }

        public List<VDIVISI> GetAuthorizedDivisi(IHeaderDictionary headers, string unitId, string divisiId)
        {
            return GetAuthorizedDivisi(GetSessionHeader(headers), unitId, divisiId);
        }

        public List<VBLOCK> GetAuthorizedBlockByUserName(string userName, string unitId, string divisiId, string blockId)
        {
            var criteria = GetFilterBlockByUserName(userName, unitId, divisiId, blockId);
            criteria = criteria.And(d => d.ACTIVE.HasValue && d.ACTIVE.Value);
            return _context.VBLOCK.Where(criteria).ToList();
        }

        public List<VBLOCK> GetAuthorizedBlock(string sessionString, string unitId, string divisiId, string blockId)
        {

            WebSession webSession = sessionString.Deserialize<WebSession>();
            return GetAuthorizedBlockByUserName(webSession.DecodedToken.UserName, unitId, divisiId, blockId);
        }

        public List<VBLOCK> GetAuthorizedBlock(IHeaderDictionary headers, string unitId, string divisiId, string blockId)
        {
            return GetAuthorizedBlock(GetSessionHeader(headers), unitId, divisiId, blockId);
        }


        public WebSession ChangePassword(IHeaderDictionary headers, string oldPassword, string newPassword)
        {
            return ChangePassword(GetSessionHeader(headers), oldPassword, newPassword);
        }
        public WebSession ChangePassword(string sessionString, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new Exception("Password baru tidak boleh kosong");
            if (newPassword == oldPassword)
                throw new Exception("Password baru tidak boleh sama dengan password sebelumnya");
            if (newPassword.Length <= 2)
                throw new Exception("Password baru harus lebih dari 3 karakter");


            WebSession webSession = sessionString.Deserialize<WebSession>();
            AuthenticationResults result;
            VAMUSER amUser = _service.AuthenticateUser(webSession.DecodedToken.UserName, oldPassword, newPassword, false, out result);
            throwInvalidAuthorization(webSession.DecodedToken.UserName, oldPassword, result);
            _webSession.RemoveSession(webSession.Token);
            return _webSession.NewSession(webSession.DecodedToken.UserName, amUser.PASSWORD, webSession.DecodedToken.FullName, false, false);
        }

        private void throwInvalidAuthorization(string userName, string password, AuthenticationResults authenticationResults)
        {
            switch (authenticationResults)
            {
                case AuthenticationResults.AccountDoesNotExist:
                    throw new Exception("User name atau password anda salah.");
                case AuthenticationResults.WrongPassword:
                    _auditContext.SaveAuditTrail(userName, "Login", "Invalid Login [" + password + "]");
                    throw new Exception("User name atau password anda salah.");
                case AuthenticationResults.InActiveAcount:
                    throw new Exception("User anda tidak aktif." + Environment.NewLine + "Anda tidak dapat login sekarang.");
                case AuthenticationResults.LockedAccount:
                    throw new Exception("User anda terkunci." + Environment.NewLine + "Anda tidak dapat login sekarang.");
                case AuthenticationResults.SystemError:
                    throw new Exception("System error" + Environment.NewLine + "Mohon hubungi IT Helpdesk");

            }
        }

        public bool ResetPasssordRequest(string userNameOrEmail)
        {
            VAMUSER user = _service.GetByUserNameOrEmail(userNameOrEmail);
            if (user == null)
                throw new Exception("User tidak ditemukan");
            if (string.IsNullOrWhiteSpace(user.EMAIL))
                throw new Exception("User tidak memiliki email, silakan hubungi IT Helpdesk");

            if (string.IsNullOrWhiteSpace(_appSetting.UIResetPasswordTokenProcessor) || string.IsNullOrWhiteSpace(_appSetting.UIResetPasswordPage))
                throw new Exception("Reset password belum dikonfigurasi, silakan hubungi IT Helpdesk");

            var session = _webSession.NewSession(user.ALIAS, user.PASSWORD, user.NAME, false, true, _appSetting.UIResetPasswordPage);
            var url = _appSetting.UIResetPasswordTokenProcessor.Replace("{token}", session.Token);
            var message = _appSetting.UIResetPasswordMessage.Replace("{name}", user.NAME).Replace("{link}", url);
            if (string.IsNullOrWhiteSpace(message))
                message = url;
            MailMessage mailMessage = new MailMessage();
            mailMessage.Subject = "[PMS] Permintaan reset password " + user.ALIAS;
            mailMessage.To.Add(new MailAddress(user.EMAIL, user.NAME));
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message;
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                await _emailSender.SendEmailAsync(mailMessage);
            });
            return true;
        }

        public bool ResetPassword(string tokenString, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new Exception("Password baru tidak boleh kosong");
            if (newPassword.Length <= 2)
                throw new Exception("Password baru harus lebih dari 2 karakter");
            var session = _webSession.ValidateSession(tokenString);
            if (session == null || !session.ForResetPassword || !session.DecodedToken.ForResetPassword)
                throw new Exception("Token untuk reset password tidak valid");
            if (session.Password != session.DecodedToken.Key)
                throw new Exception("Token untuk reset password tidak valid");
            AuthenticationResults result;
            VAMUSER amUser = _service.ChangePasswordByToken(session.DecodedToken.UserName, newPassword, out result);
            throwInvalidAuthorization(session.DecodedToken.UserName, string.Empty, result);
            _webSession.RemoveSession(session.Token);
            return true;
        }

        public List<string> GetAuthorizedLocationIdByUserName(string userName, out bool allLocations)
        {
            return _service.GetAuthorizedLocationIds(userName, out allLocations);
        }

        public bool IsAuthorizedUnit(string userName, string unitId)
        {
            try
            {
                return GetAuthorizedUnitByUserName(userName, unitId).Any();
            }
            catch { return false; }
        }

        public bool IsAuthorizedDivisi(string userName, string divisiId)
        {
            try
            {
                return GetAuthorizedDivisiByUserName(userName, string.Empty, divisiId).Any();
            }
            catch { return false; }
        }

        public bool IsAuthorizedBlock(string userName, string blockId)
        {
            try
            {
                return GetAuthorizedBlockByUserName(userName, string.Empty, string.Empty, blockId).Any();
            }
            catch { return false; }
        }

        public VAMUSER GetUserInfo(string userNameorEmail)
        {
            return _service.GetByUserNameOrEmail(userNameorEmail);
        }

        public List<string> GetAuthorizedDepartmentIdByUserName(string userName, out bool allDepartment)
        {
            return _service.GetAuthorizedDepartmentIds(userName, out allDepartment);
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
    }


    public class AuthenticationServiceEstate:AuthenticationServiceBase
    {
        public AuthenticationServiceEstate(PMSContextEstate context, AMContextEstate amContext, AuditContext auditContext, IWebSessionService webSession, IOptions<AppSetting> appSettings, IBackgroundTaskQueue taskQueue, IEmailSender emailSender):base (context,amContext,auditContext,webSession,appSettings,taskQueue,emailSender)
        {

        }
    }

    public class AuthenticationServiceHO : AuthenticationServiceBase
    {
        public AuthenticationServiceHO(PMSContextHO context, AMContextHO amContext, AuditContext auditContext, IWebSessionService webSession, IOptions<AppSetting> appSettings, IBackgroundTaskQueue taskQueue, IEmailSender emailSender) : base(context, amContext, auditContext, webSession, appSettings, taskQueue, emailSender)
        {

        }
    }





}

