using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <param name="openSSL">私钥</param>
        /// <param name="plainText">待签名的内容</param>
        /// <param name="hashAlgorithm">签名方式</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string RSAEncryptForOpenssl(this string plainText, string openSSL, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            Regex regex = new Regex(@"-----(BEGIN|END)[^-]+-----", RegexOptions.Compiled | RegexOptions.Multiline);
            string privateKey = regex.Replace(openSSL, "");

            using(var rsa = DecodeRSAPrivateKey(privateKey))
            {
                var dataBytes = Encoding.GetEncoding(encoding).GetBytes(plainText);
                HashAlgorithm algorithm = HashAlgorithm.Create(hashAlgorithm);
                var hashbyteSignature = rsa.SignData(dataBytes, algorithm);
                return Convert.ToBase64String(hashbyteSignature);
            }
        }

        /// <summary>
        /// RSA加密,随机生成公私钥对并作为出参返回
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="publicKey"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAEncrypt(this string plainText, out string publicKey, out string privateKey)
        {
            publicKey = "";
            privateKey = "";
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            byte[] dataToEncrypt = byteConverter.GetBytes(plainText);
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                publicKey = Convert.ToBase64String(rsa.ExportCspBlob(false));
                privateKey = Convert.ToBase64String(rsa.ExportCspBlob(true));

                //OAEP padding is only available on Microsoft Windows XP or later. 
                byte[] bytes_Cypher_Text = rsa.Encrypt(dataToEncrypt, false);
                publicKey = Convert.ToBase64String(rsa.ExportCspBlob(false));
                privateKey = Convert.ToBase64String(rsa.ExportCspBlob(true));
                string str_Cypher_Text = Convert.ToBase64String(bytes_Cypher_Text);
                return str_Cypher_Text;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSADecrypt(this string cipherText, string privateKey)
        {
            byte[] DataToDecrypt = Convert.FromBase64String(cipherText);
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                //RSA.ImportParameters(RSAKeyInfo);
                byte[] bytes_Public_Key = Convert.FromBase64String(privateKey);
                rsa.ImportCspBlob(bytes_Public_Key);

                //OAEP padding is only available on Microsoft Windows XP or later. 
                byte[] bytes_Plain_Text = rsa.Decrypt(DataToDecrypt, false);
                UnicodeEncoding ByteConverter = new UnicodeEncoding();
                string str_Plain_Text = ByteConverter.GetString(bytes_Plain_Text);
                return str_Plain_Text;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        #region rsa private

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(string privateKey)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
        #endregion

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
