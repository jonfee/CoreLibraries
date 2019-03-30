using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace JF.Common
{
    /// <summary>
    /// 对<see cref="XmlDocument"/>的扩展类
    /// </summary>
    public static class XmlDocumentExtensions
    {
        // 解密加载
        public static void XmlLoadDecrypt(this XmlDocument xmlDoc, string fileName, string key, string iv)
        {
            key = key ?? string.Empty;
            iv = iv ?? string.Empty;
            key = key.PadLeft(8, '0');
            iv = iv.PadLeft(8, '0');
            if (key.Length > 8) key = key.Remove(8);
            if (iv.Length > 8) iv = iv.Remove(8);

            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            byte[] bsXml = new byte[fileStream.Length];
            fileStream.Read(bsXml, 0, bsXml.Length);
            fileStream.Close();

            MemoryStream ms = new MemoryStream();
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv)), CryptoStreamMode.Write);
            encStream.Write(bsXml, 0, bsXml.Length);
            encStream.FlushFinalBlock();

            xmlDoc.Load(new MemoryStream(ms.ToArray()));
        }

        // 加密存储
        public static void XmlSaveEncrypt(this XmlDocument xmlDoc, string fileName, string key, string iv)
        {
            key = key ?? string.Empty;
            iv = iv ?? string.Empty;
            key = key.PadLeft(8, '0');
            iv = iv.PadLeft(8, '0');
            if (key.Length > 8) key = key.Remove(8);
            if (iv.Length > 8) iv = iv.Remove(8);

            if (!File.Exists(fileName))
                File.Create(fileName).Close();

            FileStream fileStream = new FileStream(fileName, FileMode.Truncate);
            MemoryStream msXml = new MemoryStream();
            xmlDoc.Save(msXml);

            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            CryptoStream cs = new CryptoStream(fileStream, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv)), CryptoStreamMode.Write);
            cs.Write(msXml.ToArray(), 0, msXml.ToArray().Length);
            cs.FlushFinalBlock();

            msXml.Close();
            fileStream.Close();
        }
    }
}
