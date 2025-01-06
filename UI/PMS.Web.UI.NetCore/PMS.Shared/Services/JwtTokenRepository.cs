using Microsoft.IdentityModel.Tokens;
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace PMS.Shared.Services
{
    public class JwtTokenRepository
    {
        
        
        public static string SIGNKEY = string.Empty;
        public static double LIFETIMERESETPASSWORDDAYS = 1;
        public static double LIFETIMESESSIONMINUTES = 60;
        private static string _payloadEncrypKey = "0(73#thisyear87#$";
        
        

        public static string GenerateToken(string userName, string password, string fullName, bool changePassword, bool resetPassword)
        {
            if (string.IsNullOrWhiteSpace(SIGNKEY))
                throw new ExceptionInvalidToken();
            var encodedSignKey = Encoding.ASCII.GetBytes(SIGNKEY);
            SigningCredentials signCredential = new SigningCredentials(new SymmetricSecurityKey(encodedSignKey), SecurityAlgorithms.HmacSha256);
            


            List<Claim> claims = new List<Claim>();            
            claims.Add(new Claim("user", userName));
            claims.Add(new Claim("fullname", fullName));
            if (changePassword)
                    claims.Add(new Claim("chgpwd", "1"));
            if (resetPassword)
                claims.Add(new Claim("rstpwd", "1"));

            DateTime utcNow = DateTime.UtcNow;
            TokenVerifyer verifier = new TokenVerifyer 
            {   
                Password = password, 
                UtcCreation = utcNow,
                Key =_payloadEncrypKey 
            };

            string verifyerString = verifier.Serialize();

            string encryptedVetifyer = PMSEncryption.Encrypt(verifyerString, _payloadEncrypKey);

            claims.Add(new Claim("key", encryptedVetifyer));
            

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = signCredential,
                Expires = utcNow.AddDays(30)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            return tokenString;
        }



        public static string GenerateInsecuredToken(List<Claim> payloads, double expirationMinutes)
        {
            if (string.IsNullOrWhiteSpace(SIGNKEY))
                throw new ExceptionInvalidToken();
            var encodedSignKey = Encoding.ASCII.GetBytes(SIGNKEY);
            SigningCredentials signCredential = new SigningCredentials(new SymmetricSecurityKey(encodedSignKey), SecurityAlgorithms.HmacSha256);
            if (payloads == null || !payloads.Any())
                throw new Exception("No token payloads");

            

            DateTime utcNow = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(payloads),
                SigningCredentials = signCredential,
                Expires = utcNow.AddMinutes(expirationMinutes)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        

       

        public static string DecryptVerifier(DecodedToken token)
        {       
            string verifyerString = PMSEncryption.Decrypt(token.Session, _payloadEncrypKey);
            TokenVerifyer tokenVerifyer = verifyerString.Deserialize<TokenVerifyer>();                
            if (tokenVerifyer.Key != _payloadEncrypKey)
                throw new ExceptionInvalidToken();
            return tokenVerifyer.Password;
        }

        public static List<Claim> DecodeTokenClaims(string tokenString)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return new List<Claim>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);
            return token.Claims.ToList();
        }

        public static DecodedToken DecodeToken(string tokenString)
        {

            List<Claim> tokenClaimns = DecodeTokenClaims(tokenString);
            var result = new DecodedToken { TokenString = tokenString };

            foreach (var claim in tokenClaimns)
            {
                switch (claim.Type)
                {
                    case "user":
                        result.UserName = claim.Value;
                        break;

                    case "rstpwd":
                        result.ForResetPassword = claim.Value == "1";
                        break;
                    case "chgpwd":
                        result.ChangePassword = claim.Value == "1";
                        break;
                    case "fullname":
                        result.FullName = claim.Value;
                        break;
                    case "key":
                        result.Session = claim.Value;
                        break;
                }
            }

            result.Key = DecryptVerifier(result);

            return result;


        }

        public static bool IsValidToken(string tokenString)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return true;
            if (tokenString.StartsWith("bearer "))
                tokenString = tokenString.Substring(7);
            try
            {
                DecodeToken(tokenString);
                return true;
            }
            catch { }
            return false;


        }

        private class TokenVerifyer
        {
            public string Password { get; set; }
            public DateTime UtcCreation { get; set; }

            public string Key { get; set; }
        }
    }
}
