using System.IO;
using System.Text;

namespace Zero
{
	public class MD5
	{
		static public string ComputeMD5(byte[] bytes)
		{
			using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
			{
				byte[] hash = md5.ComputeHash(bytes);
				var sb = new System.Text.StringBuilder();
				foreach (var h in hash)
				{
					sb.Append(h.ToString("x2"));
				}
				return sb.ToString();
			}
		}

		public static string MakeMD5String(string filePath)
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				using (var stream = File.OpenRead(filePath))
				{
					byte[] md5Data = md5.ComputeHash(stream);
					StringBuilder sBuilder = new StringBuilder();
					for (int i = 0; i < md5Data.Length; i++)
					{
						sBuilder.Append(md5Data[i].ToString("x2"));
					}

					return sBuilder.ToString();
				}
			}
		}
	}
}