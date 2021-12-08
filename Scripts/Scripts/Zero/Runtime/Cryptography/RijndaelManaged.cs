using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zero
{
	public class RijndaelManaged
	{
		static string key = "zerogameszerogamzerogameszerogam";
		static string iv = "zerogameszerogam";
		
		public static string Encrypt(string _inputData)
		{
			var aes = new System.Security.Cryptography.RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = Encoding.UTF8.GetBytes(key);
			aes.IV = Encoding.UTF8.GetBytes(iv);

			ICryptoTransform encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
			byte[] xBuff = null;
			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
				{
					byte[] xXml = Encoding.UTF8.GetBytes(_inputData);
					cs.Write(xXml, 0, xXml.Length);
				}
				xBuff = ms.ToArray();
			}

			return Convert.ToBase64String(xBuff);
		}

		public static string Decrypt(string _inputData)
		{
			var aes = new System.Security.Cryptography.RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = Encoding.UTF8.GetBytes(key);
			aes.IV = Encoding.UTF8.GetBytes(iv);

			ICryptoTransform decrypt = aes.CreateDecryptor();
			byte[] xBuff = null;

			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
				{
					byte[] xXml = Convert.FromBase64String(_inputData);
					cs.Write(xXml, 0, xXml.Length);
				}

				xBuff = ms.ToArray();
			}
			return Encoding.UTF8.GetString(xBuff);
		}
	}
}