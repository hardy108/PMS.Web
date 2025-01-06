using Microsoft.Extensions.Options;
using PMS.EFCore.Model;
using PMS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using PMS.EFCore.Helper;
using PMS.Shared.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AM.EFCore.Models;
using AM.EFCore.Data;
using Microsoft.Extensions.Logging;
using PMS.Shared.Utilities;
using System.Threading;

namespace AM.EFCore.Services
{

    public interface IWebSessionService
    {
        void RemoveExpiredSessions();
        void RemoveSession(string tokenString);
        



        WebSession RegisterSession(DecodedToken token);
        
        WebSession NewSession(string userName, string password, string fullName, bool changePassword, bool resetPassword, string url);
        WebSession NewSession(string userName, string password, string fullName, bool changePassword, bool resetPassword);
        WebSession NewSession(string tokenString);
        
        WebSession RegisterSession(string tokenString, bool forResetPassword, string key);
        


        WebSession ValidateSession(string tokenString);
        
        




    }
    public class WebSessionService: IWebSessionService
    {
        private List<WebSession> _activeSessions;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<WebSessionService> _logger;
        private AMContextBase _context;
        string connectionStringEncryptKey = "mps";
        public WebSessionService(IOptions<ConnectionString> connectionStrings,ILogger<WebSessionService> logger,IBackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _context = new AMContextBase(DBContextOption<AMContextBase>.GetOptions(connectionStrings.Value.AM, connectionStringEncryptKey));
            LoadSessions();
            
        }

        private void LoadSessions()
        {
            try
            {
                _activeSessions = _context.WebSession.AsNoTracking().Where(d => d.ExpiredDate > DateTime.Now).ToList();
            }
            catch { }
            if (_activeSessions == null)
                _activeSessions = new List<WebSession>();
        }
        


        public void RemoveExpiredSessions()
        {
            RemoveSession(string.Empty);
        }

        

        public void RemoveSession(string tokenString)
        {
            _activeSessions.RemoveAll(d => d.Token.Equals(tokenString) || d.ExpiredDate < DateTime.Now);            

            _taskQueue.QueueBackgroundWorkItem(async token => {
                
                
                try
                {
                    _logger.LogInformation("[Start]Removing expired sessions from database");
                    var expiredSessions = _context.WebSession.AsNoTracking().Where(d => d.Token.Equals(tokenString) || d.ExpiredDate < DateTime.Now).ToList();
                    _context.WebSession.RemoveRange(expiredSessions);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[Finish]Removing expired sessions from database");
                }
                catch (Exception ex)
                {
                    _logger.LogError("[Error]Removing expired sessions from database : " + ExceptionMessage.GetAllExceptionMessage(ex));
                }
            });
        }

        

        public WebSession ValidateSession(string tokenString)
        {
            //RemoveExpiredSessions(context);
            RemoveExpiredSessions();
            var activeSession = _activeSessions.FirstOrDefault(d => d.Token == tokenString);
            if (activeSession != null)
            {
                var decodedToken = JwtTokenRepository.DecodeToken(tokenString);
                if (decodedToken.Key == activeSession.Password)
                {
                    if (!decodedToken.ForResetPassword)
                    {
                        activeSession.LastAccess = DateTime.Now;

                        _taskQueue.QueueBackgroundWorkItem(async token => {
                            try
                            {
                                _logger.LogInformation("[Start]Update active sessions to database");
                                _context.Update(activeSession);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("[Finish]Update active sessions to database");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("[Error]Update active sessions to database : " + ExceptionMessage.GetAllExceptionMessage(ex));
                            }

                        });
                    }
                    activeSession.DecodedToken = decodedToken;
                }
                else
                    throw new ExceptionInvalidToken();

            }
            return activeSession;
        }
        

        




        public WebSession NewSession(string userName, string password, string fullName, bool changePassword, bool resetPassword)
        {
            string tokenString = JwtTokenRepository.GenerateToken(userName, password, fullName, changePassword, resetPassword,string.Empty);
            return RegisterSession(tokenString, resetPassword, password);
        }

        public WebSession NewSession(string userName, string password, string fullName, bool changePassword, bool resetPassword,string url)
        {
            string tokenString = JwtTokenRepository.GenerateToken(userName, password, fullName, changePassword, resetPassword,url);
            return RegisterSession(tokenString, resetPassword, password);
        }


        public WebSession NewSession(string tokenString)
        {

            var session = ValidateSession(tokenString);
            WebSession newSession = null;
            if (session != null)
                newSession = NewSession(session.DecodedToken.UserName,session.DecodedToken.Key,session.DecodedToken.FullName,session.DecodedToken.ChangePassword,session.DecodedToken.ForResetPassword);
            return newSession;
            
        }

       
       

        public WebSession RegisterSession(DecodedToken token)
        {

            WebSession webSession = RegisterSession(token.TokenString, token.ForResetPassword, token.Key);
            webSession.DecodedToken = token;
            return webSession;
        }

        public WebSession RegisterSession(string tokenString, bool forResetPassword,string key)
        {

            RemoveExpiredSessions();
            double lifeTime = forResetPassword ?
                                JwtTokenRepository.LIFETIMERESETPASSWORDDAYS * 24 * 60 * 60 : JwtTokenRepository.LIFETIMESESSIONMINUTES * 60;
            WebSession webSession = new WebSession
            {
                Token = tokenString,
                ForResetPassword = forResetPassword,
                LifeTimeInSeconds = (int)lifeTime,
                LastAccess = DateTime.Now,
                Password = key
            };
            _activeSessions.Add(webSession);
            _taskQueue.QueueBackgroundWorkItem(async token => {
                try
                {
                    _logger.LogInformation("[Start]Register new session to database");
                    _context.WebSession.Add(webSession);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[Finish]Register new session to database");
                }
                catch (Exception ex)
                {
                    _logger.LogError("[Error]Register new session to database : " + ExceptionMessage.GetAllExceptionMessage(ex));
                }
            });
            return webSession;
        }

       
    }
}
