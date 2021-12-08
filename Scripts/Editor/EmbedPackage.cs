using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UnityEditor.Extensions
{
#if UNITY_2017_3_OR_NEWER

	/// <summary>
	/// 로컬로 패키지 이동시켜주는 기능 
	/// </summary>
	/// <remarks>
	/// 유니티의 패키지 로컬로 옮겨서 수정해서 써야할 때 사용하면됨. 단. 수정이력 잘 관리해야함.
	/// </remarks>
	public static class EmbedPackage
	{
		[MenuItem("Assets/Embed Package", false, 1000000)]
		private static void EmbedPackageMenuItem()
		{
			var selection = Selection.activeObject;
			var packageName = Path.GetFileName(AssetDatabase.GetAssetPath(selection));

			Debug.Log($"Embedding package '{packageName}' into the project.");

			Client.Embed(packageName);

			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/Embed Package", true)]
		private static bool EmbedPackageValidation()
		{
			var selection = Selection.activeObject;

			if (selection == null)
			{
				return false;
			}

			var path = AssetDatabase.GetAssetPath(selection);
			var folder = Path.GetDirectoryName(path);

			// We only deal with direct folders under Packages/
			return folder == "Packages";
		}
	}

#endif
}