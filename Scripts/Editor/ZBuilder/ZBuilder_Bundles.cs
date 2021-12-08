using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public partial class ZBuilder
{
	[Serializable]
	public class BundleBuildSettings
	{
		/// <summary>플랫폼 안맞으면 변경해서 빌드할지 여부</summary>
		public bool IsAllowPlatformChange = false;
		/// <summary>지정된 FTP서버로 빌드된 파일 업로드</summary>
		public bool DoUploadToFTP = false;
		public bool DoSendNotify = false;
		/// <summary> [자동설정] SVN Revision  </summary>
		public int Revision;

		public BundleBuildSettings()
		{ }

		public override string ToString()
		{
			return base.ToString() + $"\nAllowPlatformChange: {IsAllowPlatformChange}";
		}
	}


	[MenuItem("ZGame/Bundles/TEST1", false, priority: 13)]
	static void PreProcessor_BeforeBuild()
	{
		/*
		 * https://docs.unity3d.com/Packages/com.unity.addressables@1.8/manual/AddressableAssetsGettingStarted.html#building-your-addressable-content
		 */

		// TODO : 이거 시간 얼마나 걸리는지 테스트 필요.
		ZLog.BeginProfile("BuildPlayerContent");
		{
			AddressableAssetSettings.BuildPlayerContent();
		}
		ZLog.EndProfile("BuildPlayerContent");
	}

    /// <summary>
    /// 'Update a Previous Build"와 같은 기능 + 카탈로그 비교 출력부분 결합. BuildBundle_Android() 단독 호출해도 기능상 문제 없음
    /// </summary>
    [MenuItem("ZGame/Bundles/Build Android", false, priority: 13)]
    static void BuildBundle_Android_CatalogCompare()
    {
        LoadAddresableCatalog((_listAssetBundleOld) =>
        {
            BuildBundle_Android(); 

            LoadAddresableCatalog((_listAssetBundleNew) =>
            {
                CompareAssetBundleList(_listAssetBundleOld, _listAssetBundleNew);
            });
        });
    }

    /// <summary>
    /// 'Update a Previous Build"와 같은 기능이라고 보면 됩니다.
    /// </summary>
    static void BuildBundle_Android()
	{
		var settings = new BundleBuildSettings();

		#region ========:: CommandLineArgs 처리 ::========

		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
		{
			// 커맨드 라인 인자값을 읽어오도록 한다.
			if (ZCommandLineReader.TryGetCustomArguments("-CIBUILD:", out var customArguments))
			{
				string customArgumentsInfo = $"{nameof(BuildAndroid)}() | Parsing CommandLineArgs Infos\n";
				foreach (var pair in customArguments)
				{
					customArgumentsInfo += $"Key : {pair.Key}, Value : {pair.Value}\n";
				}

				Debug.Log(customArgumentsInfo);
			}
			else
			{
				LogBuildError($"{nameof(BuildAndroid)}() | '-CIBUILD:' CommandLineArgs가 존재하지 않습니다!");
				return;
			}

			ApplyCustomArgumentsToBuildBundleSettings(customArguments, ref settings);
		}

        #endregion


        if (false == BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            LogBuildError($"{nameof(BuildBundle_Android)}() | Android 빌드 미지원상태.");
            return;
        }

        Debug.Log($"Current EditorUserBuildSettings | activeBuildTarget: {EditorUserBuildSettings.activeBuildTarget}, selectedBuildTargetGroup: {EditorUserBuildSettings.selectedBuildTargetGroup}");

		// 선택 사항으로 해줘야할듯. 플랫폼 변경은 시간이 오래 걸리니까!
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
		{
			if (!settings.IsAllowPlatformChange)
			{
				LogBuildError($"{nameof(BuildBundle_Android)}() | {BuildTargetGroup.Android} 플랫폼 변경 허용안함. Current Platform: {EditorUserBuildSettings.activeBuildTarget}");
				return;
			}

			// 플랫폼 변경 수행
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
		}
		EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;

		// 테스트: 셋팅 로드 
		var addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
		if (null == addressableSettings)
        {
			Debug.LogWarning($"AddressableAssetSettingsDefaultObject 기본거 안불러와짐.");

			addressableSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AddressableAssetSettingsDefaultObject.DefaultAssetPath);

			if (null != addressableSettings)
				Debug.LogWarning($"addressableSettings.AssetPath: {addressableSettings.AssetPath}");
		}

		Debug.LogWarning($"AddressableAssetSettingsDefaultObject.SettingsExists: {AddressableAssetSettingsDefaultObject.SettingsExists}");

		// SVN Revision 정보 수집 --------------------------------------------
		GetSVNRevision(Application.dataPath + "/../", eSVNRevsionType.BASE, ref settings.Revision);

		// 캐싱된 번들 정보 경로 가져오기. (addressables_content_state.bin 기본 경로)
		var path = ContentUpdateScript.GetContentStateDataPath(false);
		Debug.Log($"addressables_content_state.bin 기본 경로: {path}");
		if (!string.IsNullOrEmpty(path))
        {
            BackupOldAddressableBundles();

            Debug.Log($"RemoteCatalogBuildPath: {AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}");

            var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);

            StringBuilder resultSB = new StringBuilder();
            resultSB.AppendLine("[Build Addressable Result]--------------------");
            resultSB.AppendLine($"Duration: {result.Duration}");
            resultSB.AppendLine($"Error: {result.Error}");
            resultSB.AppendLine($"LocationCount: {result.LocationCount}");
            resultSB.AppendLine($"OutputPath: {result.OutputPath}");
            resultSB.AppendLine($"FileRegistry: {result.FileRegistry}");
            foreach (var s in result.FileRegistry.GetFilePaths())
            {
                resultSB.AppendLine($"\t {s}");
            }
            resultSB.AppendLine("END [Build Addressable Result]--------------------");          
            Debug.Log(resultSB.ToString());

            // 에러시 그냥 끝냄.
            if (!string.IsNullOrEmpty(result.Error))
            {
                LogBuildError($"[FAILURE] result: {result.Error}");
                SendLineBotMessage($"[Bundle FAILURE] result: {result.Error}", !string.IsNullOrEmpty(result.Error));
                return;
            }

            //-----------------------------------------------------------------------
            // FTP에 빌드 파일 올리기
            bool isFTPUploadSuccess = false;
            bool isCatalogSucccess = false;
            if (settings.DoUploadToFTP)
            {
                string username = "ftpUser";
                string password = "wpfh1225!";
                string ftpUrl = $"ftpUser@10.95.162.57:/fileServer/icarus/dev1_data/bundle/android/";
                string bundleOutputPath = $"./{AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}" + "/*"; //폴더 내용물 전부 포함시키기

                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(ZBuilder.BuildFolderPath, "./FTP_Upload.bat"), $"{username} {password} {bundleOutputPath} {ftpUrl}");
                var process = System.Diagnostics.Process.Start(procStartInfo);
                process.WaitForExit();

                Debug.LogWarning($"[UploadBundles to FTP] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");

                isFTPUploadSuccess = process.ExitCode == 0;

                // FTP업로드를 했다면, 번들 버전정보 파일들(*.bin, settings.json, link.xml...) SVN에 커밋하도록 한다.
                if (isFTPUploadSuccess)
                {
                    string catalogs_SavePath = $"{BuildFolderPath}/Assets/AddressableAssetsData/{UnityEngine.AddressableAssets.PlatformMappingService.GetPlatform()}/";

                    Debug.Log($"Catalogs SavePath: {catalogs_SavePath}");

                    // 안드로이드용 SVN경로
                    string svnUrl = $"https://svn.linegames.in/svn/zerogames/icarus/trunk/client/DevMain/Assets/AddressableAssetsData/Android/";

                    procStartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(BuildFolderPath, $"./SVN_Commit_Catalogs.bat"), $"{catalogs_SavePath} {svnUrl}");
                    procStartInfo.UseShellExecute = false;
                    process = System.Diagnostics.Process.Start(procStartInfo);
                    process.WaitForExit();

                    Debug.LogWarning($"[Commit Addressable Catalogs] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");

                    isCatalogSucccess = process.ExitCode == 0;
                }
            }

            if (settings.DoSendNotify)
            {
                // Line 메시지 전송
                string message = $"\n[*Android* AddressableBundle Build]";

                message += $"\nDuration: {result.Duration}";
                message += $"\nRevision: {settings.Revision}";
                if (settings.DoUploadToFTP)
                {
                    message += $"\nFTP Upload: {(isFTPUploadSuccess ? "성공" : "실패")}";

                    if (isFTPUploadSuccess)
                        message += $"\nCommit CatalogBin: {(isCatalogSucccess ? "성공" : "실패")}";
                }
                if (string.IsNullOrEmpty(result.Error))
                    message += $"\n[SUCCESS] : {result.Error}";
                else
                    message += $"\n[FAILURE] : {result.Error}";
                //message += $"\nOutputPath: {result.OutputPath}";

                SendLineBotMessage(message, !string.IsNullOrEmpty(result.Error));
            }
        }
        else
		{
			LogBuildError($"{nameof(BuildBundle_Android)}() | addressables_content_state.bin 파일 경로가 존재하지 않습니다!");
		}
	}

    private static void BackupOldAddressableBundles()
    {
        // 기존 번들 백업 및 삭제 ------------------

        string bundleExistingPath = $"./{AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}";
        if (Directory.Exists(bundleExistingPath))
        {
            string backupPath = bundleExistingPath.TrimEnd(Path.DirectorySeparatorChar);
            backupPath = backupPath + "_OLD";

            if (Directory.Exists(backupPath))
            {
                FileHelper.ClearFolder(backupPath);
                Directory.Delete(backupPath);
            }
            FileUtil.CopyFileOrDirectory(bundleExistingPath, backupPath);

            Debug.Log($"Old Bundle Backup : {bundleExistingPath} ---->> {backupPath}");
        }
        FileHelper.ClearFolder(bundleExistingPath);
    }

    /// <summary>
    /// 빌드 경로에 있는 Catalog를 로드한다. Catalog.json은 반드시 Catalog.hash 와 함께 있어야 한다. (어드레서블 예약 규칙). 로드가 끝나면 Addresable은 해당 카탈로그의 
    /// 사본을 Persistant 폴더에 저장하고 사용한다. 
    /// </summary>
    private static void LoadAddresableCatalog(UnityEngine.Events.UnityAction<List<IResourceLocation>> _eventLoadFinish)
	{
        AddressablesDataBuilderInput builderInput = new AddressablesDataBuilderInput(AddressableAssetSettingsDefaultObject.Settings);
        string fileName =  AddressableAssetSettingsDefaultObject.Settings.profileSettings.EvaluateString(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "/catalog_" + builderInput.PlayerVersion +".json");
        string catalogPath = $"{BuildFolderPath}{AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}{fileName}";

        Addressables.LoadContentCatalogAsync(catalogPath, true).Completed+= (AsyncOperationHandle<IResourceLocator> Catalog) =>
        {
            List<IResourceLocation> listBundle = ExtractAssetBundleList(Catalog.Result as ResourceLocationMap);
            Addressables.Release(Catalog);
            Addressables.ClearResourceLocators();
            _eventLoadFinish?.Invoke(listBundle); 
        }; 
    } 
    /// <summary>
    /// 리스트를 비교해서 각종 정보를 로그로 출력한다.
    /// </summary>
    private static void CompareAssetBundleList(List<IResourceLocation> _listAssetBundleOld, List<IResourceLocation> _listAssetBundleNew)
	{
        List<IResourceLocation> compareOld = new List<IResourceLocation>(_listAssetBundleOld);
        List<IResourceLocation> compareNew = new List<IResourceLocation>(_listAssetBundleNew);

        List<IResourceLocation> resultNoUpdate = new List<IResourceLocation>();
        List<IResourceLocation> resultAddVersion = new List<IResourceLocation>();
        List<IResourceLocation> resultDeleteVersion = new List<IResourceLocation>();

        for (int i = 0; i < compareOld.Count; i++)
		{
            AssetBundleRequestOptions assetBundleOld = compareOld[i].Data as AssetBundleRequestOptions;
            for (int j = 0; j < compareNew.Count; j++)
            {
                AssetBundleRequestOptions assetBundleNew = compareNew[j].Data as AssetBundleRequestOptions;

                if (assetBundleOld.Hash == assetBundleNew.Hash)  // 헤쉬가 같으므로 업데이트 되지 않는다.
                {
                    _listAssetBundleOld.Remove(compareOld[i]);
                    _listAssetBundleNew.Remove(compareNew[j]);
                    resultNoUpdate.Add(compareOld[i]);
                    break;
                } // 이름이 같을 경우 케시 버전이 다르게 저장된다. 
                else if (assetBundleOld.BundleName == assetBundleNew.BundleName) // 케시 버전이 다를 경우 쓰지 않는 파일이 누적되어 설치 크기가 증가한다. 수동으로 제거하도록 CPatcherAddressable에 기능이 구현되어 있다.
                {
                    _listAssetBundleOld.Remove(compareOld[i]);
                    _listAssetBundleNew.Remove(compareNew[j]);
                    resultDeleteVersion.Add(compareOld[i]);
                    resultAddVersion.Add(compareNew[j]);
                    break;
				}
            }
		}

        // Old에 남아 있는 파일은 사용하지 않으므로 제거될 것이다. 
        for (int i = 0; i < _listAssetBundleOld.Count; i++)
		{
            AssetBundleRequestOptions assetBundle = _listAssetBundleOld[i].Data as AssetBundleRequestOptions;
            string message = $"[catalog] Delete AssetBundle Name : {_listAssetBundleOld[i].PrimaryKey} / hash : {assetBundle.Hash}";
            Debug.Log(message);
        }
        // new에 남아 있는 파일은 새로 생성될 것이다.
        for (int i = 0; i < _listAssetBundleNew.Count; i++)
        {
            AssetBundleRequestOptions assetBundle = _listAssetBundleNew[i].Data as AssetBundleRequestOptions;
            string message = $"[catalog] New AssetBundle Name : {_listAssetBundleNew[i].PrimaryKey} / hash : {assetBundle.Hash}";
            Debug.Log(message);
        }
        // NoUpdate 다운로드가 되지 않을 파일들이다.
        for (int i = 0; i < resultNoUpdate.Count; i++)
        {
            AssetBundleRequestOptions assetBundle = resultNoUpdate[i].Data as AssetBundleRequestOptions;
            string message = $"[catalog] No Update AssetBundle Name : {resultNoUpdate[i].PrimaryKey} / hash : {assetBundle.Hash}";
            Debug.Log(message);
        }
        // 제거될 케시 버전이다. 
        for (int i = 0; i < resultDeleteVersion.Count; i++)
        {
            AssetBundleRequestOptions assetBundle = resultDeleteVersion[i].Data as AssetBundleRequestOptions;
            string message = $"[catalog] Remove Cache version AssetBundle Name : {resultDeleteVersion[i].PrimaryKey} / hash : {assetBundle.Hash}";
            Debug.Log(message);
        }
        // 추가될 케시 버전이다.
        for (int i = 0; i < resultAddVersion.Count; i++)
        {
            AssetBundleRequestOptions assetBundle = resultAddVersion[i].Data as AssetBundleRequestOptions;
            string message = $"[catalog] Add Cache version AssetBundle Name : {resultAddVersion[i].PrimaryKey} / hash : {assetBundle.Hash}";
            Debug.Log(message);
        }
    }

    /// <summary>
    /// 카탈로그 파일로 부터 에셋번들 리스트를 추출한다. _locationMap에는 에셋번들 내부 파일 목록도 있으므로 다른 방식으로 추출 할 수 있다.
    /// </summary>
    private static List<IResourceLocation> ExtractAssetBundleList(ResourceLocationMap _locationMap)
	{        
        List<IResourceLocation> listAssetBundle = new List<IResourceLocation>();
        if (_locationMap == null) return listAssetBundle;
        Dictionary<object, IList<IResourceLocation>>.Enumerator it = _locationMap.Locations.GetEnumerator();
        while (it.MoveNext())
        {
            IList<IResourceLocation> resLocation = it.Current.Value;

            if (resLocation.Count == 1)  // 카운트가 1일 경우가 에셋번들 파일이고 나머지는 디펜던시가 걸려있는 파일이다.
            {
                if (resLocation[0].ResourceType == typeof(IAssetBundleResource))
                {
                    AssetBundleRequestOptions assetBundle = resLocation[0].Data as AssetBundleRequestOptions;
                    if (assetBundle != null)
                    {
                        listAssetBundle.Add(resLocation[0]);
                    }
                }
            }
        }
        return listAssetBundle;
    }

    /// <summary>
    /// 'Update a Previous Build"와 같은 기능이라고 보면 됩니다.
    /// </summary>
    [MenuItem("ZGame/Bundles/Build iOS", false, priority: 13)]
    static void BuildBundle_iOS_CatalogCompare()
    {
        LoadAddresableCatalog((_listAssetBundleOld) =>
        {
            BuildBundle_iOS();
            LoadAddresableCatalog((_listAssetBundleNew) =>
            {
                CompareAssetBundleList(_listAssetBundleOld, _listAssetBundleNew);
            });
        });
    }


    static void BuildBundle_iOS()
	{
		var settings = new BundleBuildSettings();

        #region ========:: CommandLineArgs 처리 ::========

        if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
		{
			// 커맨드 라인 인자값을 읽어오도록 한다.
			if (ZCommandLineReader.TryGetCustomArguments("-CIBUILD:", out var customArguments))
			{
				string customArgumentsInfo = $"{nameof(BuildAndroid)}() | Parsing CommandLineArgs Infos\n";
				foreach (var pair in customArguments)
				{
					customArgumentsInfo += $"Key : {pair.Key}, Value : {pair.Value}\n";
				}

				Debug.Log(customArgumentsInfo);
			}
			else
			{
				LogBuildError($"{nameof(BuildAndroid)}() | '-CIBUILD:' CommandLineArgs가 존재하지 않습니다!");
				return;
			}

			ApplyCustomArgumentsToBuildBundleSettings(customArguments, ref settings);
		}

		#endregion


		if (false == BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
		{
			LogBuildError($"{nameof(BuildBundle_iOS)}() | iOS 빌드 미지원상태.");
			return;
		}

		Debug.Log($"Current EditorUserBuildSettings | activeBuildTarget: {EditorUserBuildSettings.activeBuildTarget}, selectedBuildTargetGroup: {EditorUserBuildSettings.selectedBuildTargetGroup}");

		// 선택 사항으로 해줘야할듯. 플랫폼 변경은 시간이 오래 걸리니까!
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
		{
			if (!settings.IsAllowPlatformChange)
			{
				LogBuildError($"{nameof(BuildBundle_iOS)}() | {BuildTargetGroup.iOS} 플랫폼 변경 허용안함. Current Platform: {EditorUserBuildSettings.activeBuildTarget}");
				return;
			}

			// 플랫폼 변경 수행
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
		}
		EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;

		// 테스트: 셋팅 로드 
		var addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);
		if (null == addressableSettings)
		{
			Debug.LogWarning($"AddressableAssetSettingsDefaultObject 기본거 안불러와짐.");

			addressableSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AddressableAssetSettingsDefaultObject.DefaultAssetPath);

			if (null != addressableSettings)
				Debug.LogWarning($"addressableSettings.AssetPath: {addressableSettings.AssetPath}");
		}

		Debug.LogWarning($"AddressableAssetSettingsDefaultObject.SettingsExists: {AddressableAssetSettingsDefaultObject.SettingsExists}");

		// SVN Revision 정보 수집 --------------------------------------------
		GetSVNRevision(Application.dataPath + "/../", eSVNRevsionType.BASE, ref settings.Revision);

        // 캐싱된 번들 정보 경로 가져오기. (addressables_content_state.bin 기본 경로)
        var path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log($"addressables_content_state.bin 기본 경로: {path}");
        if (!string.IsNullOrEmpty(path))
        {
            BackupOldAddressableBundles();

            Debug.Log($"RemoteCatalogBuildPath: {AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}");

            var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);

            StringBuilder resultSB = new StringBuilder();
            resultSB.AppendLine("[Build Addressable Result]--------------------");
            resultSB.AppendLine($"Duration: {result.Duration}");
            resultSB.AppendLine($"Error: {result.Error}");
            resultSB.AppendLine($"LocationCount: {result.LocationCount}");
            resultSB.AppendLine($"OutputPath: {result.OutputPath}");
            resultSB.AppendLine($"FileRegistry: {result.FileRegistry}");
            foreach (var s in result.FileRegistry.GetFilePaths())
            {
                resultSB.AppendLine($"\t {s}");
            }
            resultSB.AppendLine("END [Build Addressable Result]--------------------");

            Debug.Log(resultSB.ToString());

            // 에러시 그냥 끝냄.
            if (!string.IsNullOrEmpty(result.Error))
            {
                LogBuildError($"{nameof(BuildBundle_iOS)}() | [FAILURE] result: {result.Error}");
                SendLineBotMessage($"[Bundle FAILURE] result: {result.Error}", !string.IsNullOrEmpty(result.Error));
                return;
            }

            //-----------------------------------------------------------------------
            // FTP에 빌드 파일 올리기
            bool isFTPUploadSuccess = false;
            bool isCatalogSucccess = false;
            if (settings.DoUploadToFTP)
            {
                string username = "ftpUser";
                string password = "wpfh1225!";
                string ftpUrl = $"ftpUser@10.95.162.57:/fileServer/icarus/dev1_data/bundle/ios/";
                string bundleOutputPath = $"./{AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings)}" + "/*"; //폴더 내용물 전부 포함시키기

                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo();
                procStartInfo.FileName = "sh";
                procStartInfo.UseShellExecute = false;
                procStartInfo.Arguments = System.IO.Path.Combine(ZBuilder.BuildFolderPath, "FTP_Upload.sh") + $" {username} {password} {bundleOutputPath} {ftpUrl}";
                var process = System.Diagnostics.Process.Start(procStartInfo);
                process.WaitForExit();

                Debug.LogWarning($"[UploadBundles to FTP] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");

                isFTPUploadSuccess = process.ExitCode == 0;

                // FTP업로드를 했다면, 번들 버전정보 파일들(*.bin, settings.json, link.xml...) SVN에 커밋하도록 한다.
                if (isFTPUploadSuccess)
                {
                    string catalogs_SavePath = $"{BuildFolderPath}/Assets/AddressableAssetsData/{UnityEngine.AddressableAssets.PlatformMappingService.GetPlatform()}/";

                    Debug.Log($"Catalogs SavePath: {catalogs_SavePath}");

                    // 안드로이드용 SVN경로
                    string svnUrl = $"https://svn.linegames.in/svn/zerogames/icarus/trunk/client/DevMain/Assets/AddressableAssetsData/Android/";

                    procStartInfo = new System.Diagnostics.ProcessStartInfo("sh", $"{System.IO.Path.Combine(BuildFolderPath, $"./SVN_Commit_Catalogs.sh")} {catalogs_SavePath} {svnUrl}");
                    procStartInfo.UseShellExecute = false;
                    process = System.Diagnostics.Process.Start(procStartInfo);
                    process.WaitForExit();

                    Debug.LogWarning($"[Commit Addressable Catalogs] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");

                    isCatalogSucccess = process.ExitCode == 0;
                }
            }

            if (settings.DoSendNotify)
            {
                // Line 메시지 전송
                string message = $"\n[*iOS* AddressableBundle Build]";

                message += $"\nDuration: {result.Duration}";
                message += $"\nRevision: {settings.Revision}";
                if (settings.DoUploadToFTP)
                {
                    message += $"\nFTP Upload: {(isFTPUploadSuccess ? "성공" : "실패")}";

                    if (isFTPUploadSuccess)
                        message += $"\nCommit CatalogBin: {(isCatalogSucccess ? "성공" : "실패")}";
                }
                if (string.IsNullOrEmpty(result.Error))
                    message += $"\n[SUCCESS] : {result.Error}";
                else
                    message += $"\n[FAILURE] : {result.Error}";
                //message += $"\nOutputPath: {result.OutputPath}";

                SendLineBotMessage(message, !string.IsNullOrEmpty(result.Error));
            }
        }
        else
        {
            LogBuildError($"{nameof(BuildBundle_iOS)}() | addressables_content_state.bin 파일 경로가 존재하지 않습니다!");
        }
    }

	/// <summary>
	/// <see cref="Environment.CommandLine"/> 정보중 빌드 설정에 맞는 데이터 파싱해서 적용
	/// </summary>
	static void ApplyCustomArgumentsToBuildBundleSettings(Dictionary<string, string> customArgDic, ref BundleBuildSettings settings)
	{
		if (customArgDic.TryGetValue("ALLOW_PLATFORM_CHANGE", out var allowPlatformChangeStr))
		{
			settings.IsAllowPlatformChange = bool.Parse(allowPlatformChangeStr);
		}

		if (customArgDic.TryGetValue("FTP_UPLOAD", out var ftpUploadStr))
		{
			settings.DoUploadToFTP = bool.Parse(ftpUploadStr);
		}

		if (customArgDic.TryGetValue("SEND_NOTIFY", out var sendNotifyStr))
		{
			settings.DoSendNotify = bool.Parse(sendNotifyStr);
		}
	}
}
