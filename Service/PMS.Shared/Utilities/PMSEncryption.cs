using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Shared.Utilities
{
    public class PMSEncryption
    {
        public static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            var ms = new MemoryStream();

            var alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            //cs.Clear();
            //cs.Dispose();
            cs.Close();

            var decryptedData = ms.ToArray();
            return decryptedData;
        }

        //PasswordKey = pmspwd
        public static string Decrypt(string cipherText, string Password)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            var pdb = new PasswordDeriveBytes(Password,
                                              new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
                                                          0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            var decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Encoding.Unicode.GetString(decryptedData);
        }


        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();

            var encryptedData = ms.ToArray();

            return encryptedData;
        }

        //PasswordKey = pmspwd
        public static string Encrypt(string clearText, string Password)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);

            var pdb = new PasswordDeriveBytes(Password,
                                              new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                                                          0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            var encryptedData = Encrypt(clearBytes,
                                        pdb.GetBytes(32), pdb.GetBytes(16));

            return Convert.ToBase64String(encryptedData);
        }
    }
}
