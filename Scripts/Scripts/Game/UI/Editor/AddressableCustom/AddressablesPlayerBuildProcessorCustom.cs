using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using System.IO;

public class AddressablesPlayerBuildProcessorCustom : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{	
	public int callbackOrder
	{
		get { return 100; }
	}

	public void OnPreprocessBuild(BuildReport report)
	{
		DeleteAddressableData();
		CopyToAddressableData();
	} 

	//---------------------------------------------------------
	public void OnPostprocessBuild(BuildReport report)
	{

	}


	//--------------------------------------------------------
	// 어드레서블에 의해 임의로 복사된 불필요 파일을 제거한다 (APK에 들어가지 못하게)
	private static void DeleteAddressableData()
	{
		string targetFile = $"{Addressables.PlayerBuildDataPath}/catalog.json";
		if (File.Exists(targetFile))
		{
			File.Delete(targetFile);
		}
		targetFile = $"{Addressables.PlayerBuildDataPath}/buildLogs.json";
		if (File.Exists(targetFile))
		{
			File.Delete(targetFile);
		}

		targetFile = $"{Addressables.PlayerBuildDataPath}/link.xml";
		if (File.Exists(targetFile))
		{
			File.Delete(targetFile);
		}

		targetFile = $"{Addressables.PlayerBuildDataPath}/settings.json";
		if (File.Exists(targetFile))
		{
			File.Delete(targetFile);
		}
	}
	// SVN 에서 APK로 어드레서블 구동에 필요한 필수 데이터 파일을 복사
	private static void CopyToAddressableData()
	{
		if (!Directory.Exists(Addressables.PlayerBuildDataPath))
			Directory.CreateDirectory(Addressables.PlayerBuildDataPath);

		string sourceFile = $"{ExtractCustomBuildPath()}/link.xml";
		string destFile = $"{Addressables.PlayerBuildDataPath}/link.xml";
		File.Copy(sourceFile, destFile);

		sourceFile = $"{ExtractCustomBuildPath()}/settings.json";
		destFile = $"{Addressables.PlayerBuildDataPath}/settings.json";	

		File.Copy(sourceFile, destFile);
	}

	private static string ExtractCustomBuildPath()
	{
		return string.Format("{0}/{1}", AddressableAssetSettingsDefaultObject.Settings.ConfigFolder, PlatformMappingService.GetPlatform());
	}
}
