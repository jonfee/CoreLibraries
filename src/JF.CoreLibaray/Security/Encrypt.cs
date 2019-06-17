using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JF.Security
{
    /// <summary>
    /// 加/解密
    /// </summary>
    public static class Encrypt
    {
        #region MD5

        /// <summary>
        /// 获取MD5值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <returns>MD5值</returns>
        public static string GetMD5(this string sourceString)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return GetHash(sourceString, md5);
        }

        #endregion

        #region RSA

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="sourceString">待签名的内容</param>
        /// <param name="hashAlgorithm">签名方式</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string RSASign(this string sourceString, string privateKey, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);//加载私钥
            var dataBytes = Encoding.GetEncoding(encoding).GetBytes(sourceString);
            var hashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            return Convert.ToBase64String(hashbyteSignature);
        }

        #endregion

        #region SHA

        /// <summary>
        /// 获取SHA1值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <returns>SHA1值</returns>
        public static string GetSHA1(this string sourceString)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            return GetHash(sourceString, sha1);
        }

        /// <summary>
        /// 获取SHA256值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <returns>SHA256值</returns>
        public static string GetSHA256(this string sourceString)
        {
            SHA256 sha256 = SHA256.Create();
            return GetHash(sourceString, sha256);
        }

        /// <summary>
        /// 获取SHA384值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <returns>SHA384值</returns>
        public static string GetSHA384(this string sourceString)
        {
            SHA384 sha384 = SHA384.Create();
            return GetHash(sourceString, sha384);
        }

        /// <summary>
        /// 获取SHA512值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <returns>SHA512值</returns>
        public static string GetSHA512(this string sourceString)
        {
            SHA512 sha512 = SHA512.Create();
            return GetHash(sourceString, sha512);
        }

        /// <summary>
        /// 获取某个哈希算法对应下的哈希值
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <param name="algorithm">哈希算法</param>
        /// <returns>经过计算的哈希值</returns>
        private static string GetHash(string sourceString, HashAlgorithm algorithm)
        {
            if (string.IsNullOrWhiteSpace(sourceString) || algorithm == null) return string.Empty;

            byte[] sourceBytes = Encoding.UTF8.GetBytes(sourceString);
            byte[] result = algorithm.ComputeHash(sourceBytes);

            algorithm.Clear();

            StringBuilder sb = new StringBuilder(32);
            foreach (byte cha in result)
            {
                sb.Append(cha.ToString("X2"));
            }
            return sb.ToString();
        }

        #endregion

        #region Base64

        public static string GetFileBase64String(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    try
                    {
                        return GetBase64String(reader.ReadBytes((int)fs.Length));
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        public static string GetBase64String(this string sourceString)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(sourceString);
            return GetBase64String(buffer);
        }

        public static string GetBase64String(this string sourceString, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(sourceString);
            return GetBase64String(buffer);
        }

        private static string GetBase64String(byte[] sourceBytes)
        {
            string base64String = System.Convert.ToBase64String(sourceBytes);
            return base64String;
        }

        #endregion

        #region DESC 对称加解密

        /// <summary>
        /// Des对称解密方法
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptFor(this string sourceString, string key)
        {
            return sourceString.DecryptFor(key, key);
        }

        /// <summary>
        /// Des对称解密方法
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DecryptFor(this string sourceString, string key, string iv)
        {
            string cipher = string.Empty;

            try
            {
                if (sourceString == null) sourceString = string.Empty;
                sourceString = sourceString.Replace("%", "+");

                key = key ?? string.Empty;
                iv = iv ?? string.Empty;
                key = key.PadLeft(8, '0');
                iv = iv.PadLeft(8, '0');
                if (key.Length > 8) key = key.Remove(8);
                if (iv.Length > 8) iv = iv.Remove(8);

                byte[] buffKey = Encoding.ASCII.GetBytes(key);
                byte[] buffIV = Encoding.ASCII.GetBytes(iv);

                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                provider.Mode = CipherMode.ECB;
                provider.Key = buffKey;
                provider.IV = buffIV;

                ICryptoTransform transform = provider.CreateDecryptor();

                using (var ms = new MemoryStream(Convert.FromBase64String(sourceString)))
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    cipher = sr.ReadToEnd();
                }
            }
            catch { }

            return cipher;
        }


        /// <summary>
        /// Des对称加密方法
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptFor(this string sourceString, string key)
        {
            return sourceString.EncryptFor(key, key);
        }

        /// <summary>
        /// Des对称加密方法
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string EncryptFor(this string sourceString, string key, string iv)
        {
            string cipher = string.Empty;

            try
            {
                if (sourceString == null) sourceString = string.Empty;
                key = key ?? string.Empty;
                iv = iv ?? string.Empty;
                key = key.PadLeft(8, '0');
                iv = iv.PadLeft(8, '0');
                if (key.Length > 8) key = key.Remove(8);
                if (iv.Length > 8) iv = iv.Remove(8);

                byte[] buffKey = Encoding.ASCII.GetBytes(key);
                byte[] buffIV = Encoding.ASCII.GetBytes(iv);

                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                provider.Mode = CipherMode.ECB;
                provider.Key = buffKey;
                provider.IV = buffIV;
                ICryptoTransform transform = provider.CreateEncryptor();

                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(sourceString);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    sw.Flush();
                    var signData = ms.ToArray();
                    cipher = Convert.ToBase64String(signData);
                }
                provider.Clear();
            }
            catch { }

            return cipher.Replace("+", "%");
        }

        #endregion
    }
}
