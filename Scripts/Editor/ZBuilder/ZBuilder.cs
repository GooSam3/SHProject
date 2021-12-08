using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Android, iOS CI(Continious Intergration)빌드용
/// </summary>
/// <remarks>
/// TODO : 한사이클 제대로 빌드시, 코드 분리하기
/// </remarks>
public partial class ZBuilder
{
	static string _buildFolderPath;
	static public string BuildFolderPath
	{
		get
		{
			if (string.IsNullOrEmpty(_buildFolderPath))
				return Application.dataPath.Remove(Application.dataPath.Length - 6);
			else
				return _buildFolderPath;
		}
		set { _buildFolderPath = value; }
	}

	// Line 측으로부터 받은 앱패키지명들 ------------------------------------------------------
	static public readonly string GooglePackageName = "com.linegames.ie";
	static public readonly string OnestorePackageName = "com.linegames.ie.one";
	static public readonly string ApplePackageName = "com.linegames.ie";

	/// <summary>
	/// 빌드 결과물 업로드될 svn 저장소 주소
	/// </summary>
	static public readonly string SVN_APK_UPLOAD_URL = $"https://svn.linegames.in/svn/zerogames/icarus/distribution/";

	[Serializable]
	public class AppBuildSettings
	{
		/// <summary> [필수] 빌드시 사용될 NTSDK Config 데이터명 </summary>
		public string NTSDK_ConfigName = "DEV";
		/// <summary> [자동] <see cref="NTSDK_ConfigName"/>의 설정정보 할당됨 </summary>
		public NTCore.CommonSetupData NTSDK_SetupData = null;

		/// <summary>Bundle Identifier</summary>
		public string PackageName = "com.linegames.ie";
		///// <summary> App version </summary>
		//public string AppVersion = "0.0.0";
		///// <summary> NTSDK에서 사용하는 스토어정보 </summary>
		//public NTCore.StoreCD StoreCD = NTCore.StoreCD.GOOGLE_PLAY;
		/// <summary> <see cref="PlayerSettings.Android.bundleVersionCode"/> or <see cref="PlayerSettings.iOS.buildNumber"/> </summary>
		public int VersionCode = 1;
		public int Revision; //SVN Revision
		public List<string> CustomDefines = new List<string>();

		// 자동 설정 변수들 ---------------------
		public string OutputDirPath = "..\\";
		public string OutputFileName = "IcarusEternal";
		public string OutputFileExtension = "apk";

		// Unity PlayerSettings
		public List<string> DefineSymbols = new List<string>();
		public ScriptingImplementation ScriptingImplementation = ScriptingImplementation.IL2CPP;
		public Il2CppCompilerConfiguration Il2CppCompilerConfiguration = Il2CppCompilerConfiguration.Release;
		public StackTraceLogType StackTraceLogType = StackTraceLogType.ScriptOnly;

		public bool IsDevelopmentBuild = false;
		public bool DoBuildAndRun = false;
		/// <summary>지정된 SVN저장소로 빌드된 파일 업로드</summary>
		public bool DoCommitToSVN = false;
		/// <summary>지정된 FTP서버로 빌드된 파일 업로드</summary>
		public bool DoUploadToFTP = false;
		public bool DoSendNotify = false;

		public AppBuildSettings()
		{ }

		public string GetFullFileName()
		{
			return OutputFileName + "." + OutputFileExtension;
		}

		public override string ToString()
		{
			return base.ToString() + $"\nNTSDK_ConfigName: {NTSDK_ConfigName}" +
				$"\nPackageName: {PackageName}" +
				$"\nVersionCode: {VersionCode}" +
				$"\nRevision: {Revision}" +
				//$"\nDefineSymbols: {DefineSymbols.Aggregate((a, b) => a + ", " + b)}" +
				$"\nScriptingImplementation: {ScriptingImplementation}" +
				$"\nIl2CppCompilerConfiguration: {Il2CppCompilerConfiguration}" +
				$"\nStackTraceLogType: {StackTraceLogType}" +
				$"\nIsDevelopmentBuild: {IsDevelopmentBuild}" +
				$"\nDoCommitToSVN: {DoCommitToSVN}" +
				$"\nDoSendNotify: {DoSendNotify}";
		}
	}

	/// <summary> 회사내 QA 테스트용 </summary>
	static AppBuildSettings CreateAppBuildSetting_QA()
	{
		var setting = new AppBuildSettings();

		setting.NTSDK_ConfigName = "QA";
		setting.Revision = 0;
		setting.OutputDirPath = BuildFolderPath;
		setting.OutputFileName = "QA";
		setting.IsDevelopmentBuild = true;
		setting.DoSendNotify = true;
		setting.CustomDefines.AddRange(new string[] { "PLANAR3_URP", "DREAMTECK_SPLINES", "NTSDK_DEBUG_LOG", "ZLOG" });
		return setting;
	}

	static AppBuildSettings CreateAppBuildSetting_QA_OneStore()
	{
		var setting = CreateAppBuildSetting_QA();
		setting.NTSDK_ConfigName = "QA_ONE";

		return setting;
	}

	// LineGames에 QA빌드 전달시 사용
	static AppBuildSettings CreateAppBuildSetting_QA_Line()
	{
		var setting = new AppBuildSettings();

		setting.NTSDK_ConfigName = "QA_Line";
		setting.Revision = 0;
		setting.OutputDirPath = BuildFolderPath;
		setting.IsDevelopmentBuild = false;
		setting.DoSendNotify = true;
		setting.DoCommitToSVN = true;
		setting.CustomDefines.AddRange(new string[] { "PLANAR3_URP", "DREAMTECK_SPLINES", "NTSDK_DEBUG_LOG", "!ZLOG" });
		return setting;
	}

	static AppBuildSettings CreateAppBuildSetting_QA_Line_OneStore()
	{
		var setting = CreateAppBuildSetting_QA_Line();
		setting.NTSDK_ConfigName = "QA_Line_ONE";

		return setting;
	}

	static AppBuildSettings CreateAppBuildSetting_DEV()
	{
		var setting = new AppBuildSettings();

		setting.NTSDK_ConfigName = "DEV";
		setting.Revision = 0;
		setting.OutputDirPath = BuildFolderPath;
		setting.IsDevelopmentBuild = true;
		setting.CustomDefines.AddRange(new string[] { "PLANAR3_URP", "DREAMTECK_SPLINES", "NTSDK_DEBUG_LOG", "ZLOG" });
		return setting;
	}

	/// <summary> OneStore 용 빌드 세팅 </summary>
	static AppBuildSettings CreateAppBuildSetting_DEV_OneStore()
	{
		var setting = new AppBuildSettings();

		setting.NTSDK_ConfigName = "DEV_ONE";
		setting.Revision = 0;
		setting.OutputDirPath = BuildFolderPath;
		setting.IsDevelopmentBuild = true;
		setting.CustomDefines.AddRange(new string[] { "PLANAR3_URP", "DREAMTECK_SPLINES", "NTSDK_DEBUG_LOG", "ZLOG" });
		return setting;
	}

	#region ================:: 안드로이드(AOS)  ::================================================================

	[MenuItem("ZGame/AppBuild/Build Android_QA", false, priority: 12)]
	static void BuildAndroidQA()
	{
		BuildAndroid(CreateAppBuildSetting_QA());
	}

	[MenuItem("ZGame/AppBuild/Build Android_QA_OneStore", false, priority: 12)]
	static void BuildAndroidQA_OneStore()
	{
		BuildAndroid(CreateAppBuildSetting_QA_OneStore());
	}

	[MenuItem("ZGame/AppBuild/Build Android_QALine", false, priority: 12)]
	static void BuildAndroidQA_Line()
	{
		BuildAndroid(CreateAppBuildSetting_QA_Line());
	}

	[MenuItem("ZGame/AppBuild/Build Android_QALine_OneStore", false, priority: 12)]
	static void BuildAndroidQA_Line_OneStore()
	{
		BuildAndroid(CreateAppBuildSetting_QA_Line_OneStore());
	}

	/// <summary> 배치모드용 </summary>
	[MenuItem("ZGame/AppBuild/Build Android_DEV", false, priority: 12)]
	static void BuildAndroid()
	{
		BuildAndroid(null);
	}

	[MenuItem("ZGame/AppBuild/Build Android_DEV_OneStore", false, priority: 12)]
	static void BuildOneStore()
	{
		BuildAndroid(CreateAppBuildSetting_DEV_OneStore());
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_BuildSettings"></param>
	static void BuildAndroid(AppBuildSettings _BuildSettings = null)
	{
		/*
		 * 기본 빌드 셋팅 데이터
		 */
		if (null == _BuildSettings)
		{
			_BuildSettings = CreateAppBuildSetting_DEV();
		}

		//----------------------------------------------------------------
		// CommandLineArgs 처리
		//----------------------------------------------------------------
		if (!ParseAgumentsToBuildSettings(ref _BuildSettings))
		{
			return;
		}

		//----------------------------------------------------------------
		// NTSDK 설정
		//----------------------------------------------------------------
		if (!ChangeNTSetupData(ref _BuildSettings))
		{
			return;
		}

		#region ========|| 빌드 플랫폼 체크 ||========

		if (false == BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
		{
			LogBuildError($"{nameof(BuildAndroid)}() | Android 빌드 미지원상태.");
			return;
		}

		// 기본 Texture override는 ETC2
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
		{
			EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
			EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality16Bit;
		}
		else
		{
			Debug.Log($"{nameof(BuildAndroid)}() | MobileTextureSubtarget.ETC2, AndroidETC2Fallback.Quality32BitDownscaled는 BatchMode가 아니라서 현재 설정값({EditorUserBuildSettings.androidBuildSubtarget}, {EditorUserBuildSettings.androidETC2Fallback})대로 빌드진행.");
		}
																								 // 강제 플랫폼 전환
		if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
		{
			EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
		}

		EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
		EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
		#endregion

		//-----------------------------------------------------------------------
		// Editor 설정 --------------------------------------------

		//퀄리티 강제 설정
		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			if (QualitySettings.names[i] == E_Quality.VeryHigh.ToString())
			{
				QualitySettings.SetQualityLevel(i, true);
				break;
			}
		}

		//-----------------------------------------------------------------------
		// Edit ProjectSettings 설정 --------------------------------------------

		// 패키지명 설정
		AdjustPackageName(_BuildSettings);
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, _BuildSettings.PackageName);
		// NTSDK에서 설정한 ClientVersion으로 Overwrite
		PlayerSettings.bundleVersion = _BuildSettings.NTSDK_SetupData.clientVersion;
		PlayerSettings.use32BitDisplayBuffer = true;
		PlayerSettings.Android.bundleVersionCode = _BuildSettings.VersionCode;

		EditPlayerSettings_Android();
		{
			// 개발빌드가 아니라면 Master로!
			if (!_BuildSettings.IsDevelopmentBuild)
				_BuildSettings.Il2CppCompilerConfiguration = Il2CppCompilerConfiguration.Master;

			EditPlayerSettings_Configuration(EditorUserBuildSettings.selectedBuildTargetGroup, _BuildSettings.ScriptingImplementation, _BuildSettings.Il2CppCompilerConfiguration);
		}
		EditPlayerSettings_Optimization(EditorUserBuildSettings.selectedBuildTargetGroup, _BuildSettings.ScriptingImplementation);
		EditPlayerSettings_StackTrace(_BuildSettings.StackTraceLogType);

		// Symbol Defines 설정 --------------------------------------------
		ApplyDefineSymbols(_BuildSettings);

		// SVN Revision 정보 수집 --------------------------------------------
		GetSVNRevision(Application.dataPath + "/../", eSVNRevsionType.BASE, ref _BuildSettings.Revision);

		// APK파일명 편집 --------------------------------------------
		AdjustOutputFileName(_BuildSettings);

		//-----------------------------------------------------------------------
		// 빌드 수행 -------------------------------------------------------------

		// 빌드 결과에 따른 처리 --------------------------------------------
		// 1. 보안 솔루션 적용
		// 2. 업로드 APK?
		// 3. 빌드 결과 아림 (LineMessage)

		if (!BuildPipeline.isBuildingPlayer)
		{
			bool bResult = BuildPlayer(_BuildSettings, BuildTarget.Android);

			bool successCommitToSVN = false;
			if (bResult)
			{
				//-----------------------------------------------------------------------
				// SVN에 빌드 파일 올리기
				if (_BuildSettings.DoCommitToSVN)
				{
					string svnUrl = $"{SVN_APK_UPLOAD_URL}{_BuildSettings.NTSDK_SetupData.DomainType}/";
					string outputFileNameWithExt = _BuildSettings.GetFullFileName();

					System.Diagnostics.ProcessStartInfo procStartInfo= new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(BuildFolderPath, $"./SVN_Upload.bat"), $"{outputFileNameWithExt} {svnUrl}");
					procStartInfo.UseShellExecute = false;
					var process = System.Diagnostics.Process.Start(procStartInfo);
					process.WaitForExit();

					successCommitToSVN = process.ExitCode == 0;

					Debug.LogWarning($"[Commit APK] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");
				}

				// TODO: 회사기기아니면 저장소 접근이 불가능하기 때문에 사용하지 않음. 외부 공유장소 생기면 처리
				////-----------------------------------------------------------------------
				//// FTP에 빌드 파일 올리기
				//if (_BuildSettings.DoUploadToFTP)
				//{
				//	string username = "ftpUser";
				//	string password = "wpfh1225!";
				//	string ftpUrl = $"ftpUser@10.95.162.57:/fileServer/icarus/apk/{_BuildSettings.NTSDK_SetupData.DomainType}";

				//	System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(System.IO.Path.Combine(ZBuilder.BuildFolderPath, "./FTP_Upload.bat"), $"{username} {password} {_BuildSettings.GetFullFileName()} {ftpUrl}");
				//	procStartInfo.UseShellExecute = false;
				//	var process = System.Diagnostics.Process.Start(procStartInfo);
				//	process.WaitForExit();

				//	Debug.LogWarning($"[UploadAPK to FTP] ExitCode: {process.ExitCode}, Command : {procStartInfo.FileName} + {procStartInfo.Arguments}");
				//}
			}

			if (_BuildSettings.DoSendNotify)
			{
				string message = $"\n[Android Build]";

				message += $"\nOutput: {(bResult ? _BuildSettings.GetFullFileName() : "실패")}";
				if (_BuildSettings.DoCommitToSVN)
				{
					message += successCommitToSVN ? $"\nSVN: {SVN_APK_UPLOAD_URL}{_BuildSettings.NTSDK_SetupData.DomainType}/{_BuildSettings.GetFullFileName()}" : "*fail CommitSVN*";
				}

				// TODO : FTP 업로드 했냐에 따라 메시지 추가해주기.
				//_BuildSettings.DoUploadToFTP

				SendLineBotMessage(message, bResult);
			}
		}
		else
		{
			LogBuildError($"{nameof(BuildAndroid)}() : 이미 빌드중입니다.");
		}
	}

	#endregion //android build

	#region ================:: 아이폰(IOS)  ::================================================================

	[MenuItem("ZGame/AppBuild/Build iOS_DEV", false, priority: 13)]
	static void BuildIOS()
	{
		BuildIOS(null);
	}

	static void BuildIOS(AppBuildSettings _BuildSettings = null)
	{
		/*
		 * 기본 빌드 셋팅 데이터
		 */
		if (null == _BuildSettings)
			_BuildSettings = CreateAppBuildSetting_DEV();

		//----------------------------------------------------------------
		// CommandLineArgs 처리
		//----------------------------------------------------------------
		if (!ParseAgumentsToBuildSettings(ref _BuildSettings))
		{
			return;
		}

		//----------------------------------------------------------------
		// NTSDK 설정
		//----------------------------------------------------------------
		if (!ChangeNTSetupData(ref _BuildSettings))
		{
			return;
		}


		#region ========|| 빌드 플랫폼 체크 ||========

		if (false == BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
		{
			LogBuildError($"{nameof(BuildIOS)}() | {BuildTarget.iOS} 빌드 미지원상태.");
			return;
		}

		// 이거 셋팅에서 필요한건가?
		EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Debug;
		Debug.Log($"CIBUILD | iOSBuildConfigType: {EditorUserBuildSettings.iOSBuildConfigType}");


		// 강제 플랫폼 전환
		if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
		{
			EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;
		}

		//-----------------------------------------------------------------------
		// Edit ProjectSettings 설정 --------------------------------------------

		// 패키지명 설정
		AdjustPackageName(_BuildSettings);
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, _BuildSettings.PackageName);
		// NTSDK에서 설정한 ClientVersion으로 Overwrite
		PlayerSettings.bundleVersion = _BuildSettings.NTSDK_SetupData.clientVersion;
		PlayerSettings.use32BitDisplayBuffer = true;
		PlayerSettings.iOS.buildNumber = _BuildSettings.VersionCode.ToString();

		//
		{
			// 개발빌드가 아니라면 Master로!
			if (!_BuildSettings.IsDevelopmentBuild)
				_BuildSettings.Il2CppCompilerConfiguration = Il2CppCompilerConfiguration.Master;

			EditPlayerSettings_Configuration(EditorUserBuildSettings.selectedBuildTargetGroup, _BuildSettings.ScriptingImplementation, _BuildSettings.Il2CppCompilerConfiguration);
		}
		EditPlayerSettings_Optimization(EditorUserBuildSettings.selectedBuildTargetGroup, _BuildSettings.ScriptingImplementation);
		EditPlayerSettings_StackTrace(_BuildSettings.StackTraceLogType);

		// Symbol Defines 설정 --------------------------------------------
		ApplyDefineSymbols(_BuildSettings);

		// SVN Revision 정보 수집 --------------------------------------------
		GetSVNRevision(Application.dataPath + "/../", eSVNRevsionType.BASE, ref _BuildSettings.Revision);

		// APK파일명 편집 --------------------------------------------
		AdjustOutputFileName(_BuildSettings);

		//PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.FastButNoExceptions

		#endregion

		Debug.LogError("BuildIOS 미완성 | 작업필요!!");
	}

	#endregion //ios build

	/// <summary>
	/// <see cref="Environment.GetCommandLineArgs"/>를 빌드셋팅 데이터로 파싱해준다. 배치모드일때만 작동.
	/// </summary>
	private static bool ParseAgumentsToBuildSettings(ref AppBuildSettings _BuildSettings)
	{
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
		{
			// 커맨드 라인 인자값을 읽어오도록 한다.
			if (ZCommandLineReader.TryGetCustomArguments("-CIBUILD:", out var customArguments))
			{
				string customArgumentsInfo = $"CIBUILD | Parsing CommandLineArgs Infos\n";
				foreach (var pair in customArguments)
				{
					customArgumentsInfo += $"Key : {pair.Key}, Value : {pair.Value}\n";
				}

				Debug.Log(customArgumentsInfo);
			}
			else
			{
				LogBuildError($"CIBUILD | '-CIBUILD:' CommandLineArgs가 존재하지 않습니다!");
				return false;
			}

			ApplyCustomArgumentsToBuildSettings(customArguments, ref _BuildSettings);
		}
		else
		{
			Debug.LogWarning($"Batchmode 빌드가 아니라서 ZBuilder.CreateAppBuildSetting_Default() 셋팅값으로 빌드합니다!");
		}

		return true;
	}

	/// <summary>
	/// 현재 SDK환경을 <see cref="AppBuildSettings.NTSDK_ConfigName"/>으로 변경시킨다.
	/// </summary>
	private static bool ChangeNTSetupData(ref AppBuildSettings _BuildSettings)
	{
		if (null == NTCore.ConfigListData.Instance)
		{
			LogBuildError($"CIBUILD | NTSDK가 존재하지 않거나, ConfigListData가 존재하지 않습니다.");
			return false;
		}

		// 빌드하고자 하는 SDK설정정보가 존재해야 빌드 가능!
		var configListData = NTCore.ConfigListData.Instance;
		if (false == configListData.SavedConfigPreset.TryGetValue(_BuildSettings.NTSDK_ConfigName, out var sdkConfigData))
		{
			LogBuildError($"CIBUILD | NTSDK ConfigList에 [{_BuildSettings.NTSDK_ConfigName}] 설정이 존재하지 않습니다!");
			return false;
		}

		// 빌드에 사용될 SDK설정으로 변경
		configListData.ChangeConfig(_BuildSettings.NTSDK_ConfigName);
		// 현재 NTSDK 설정 데이터 가져오기.
		_BuildSettings.NTSDK_SetupData = NTCore.CommonSetupData.Instance;

		//================================================================================
		// 설정 덮어씌우기
		//_BuildSettings.NTSDK_SetupData.clientVersion = _BuildSettings.AppVersion;
		//_BuildSettings.NTSDK_SetupData.storeCD = _BuildSettings.StoreCD;

		//EditorUtility.SetDirty(_BuildSettings.NTSDK_SetupData);

		return true;
	}

	/// <summary> 
	/// 실제 빌드 & 결과물 생성 (<see cref="BuildPipeline.BuildPlayer(BuildPlayerOptions)"/> 수행)
	/// </summary>
	/// <returns>빌드 성공 or 실패</returns>
	static bool BuildPlayer(AppBuildSettings _appBuildSetting, BuildTarget _buildTarget)
	{
		if (!Directory.Exists(_appBuildSetting.OutputDirPath))
			Directory.CreateDirectory(_appBuildSetting.OutputDirPath);

		// 빌드에서 Revision정보 사용을 위한 처리
		System.IO.File.WriteAllText(Application.dataPath + "/Resources/RevisionVer.txt", string.Format("r{0}", _appBuildSetting.Revision));
		UnityEditor.AssetDatabase.ImportAsset("Assets/Resources/RevisionVer.txt");
		AssetDatabase.SaveAssets();
		
		// Build options --------------------------------------------
		BuildOptions buildOption = BuildOptions.None;
		if (_appBuildSetting.IsDevelopmentBuild)
		{
			buildOption |= BuildOptions.Development;
			buildOption |= BuildOptions.CompressWithLz4;
			//buildOption |= BuildOptions.AllowDebugging; // 빌드시 에러나서 뺌. 쓰지도 않고..
		}
		else
		{
			// 릴리즈 빌드용 압축
			buildOption |= BuildOptions.CompressWithLz4HC;
		}
		if (_appBuildSetting.DoBuildAndRun)
			buildOption |= BuildOptions.AutoRunPlayer;

		// Build 결과물 경로 설정 --------------------------------------------
		string outputFullPath = Path.Combine(_appBuildSetting.OutputDirPath, _appBuildSetting.GetFullFileName());

		Debug.Log($"[BUILD] BuildOptions: {buildOption}, OutputPath: {outputFullPath}");

		// 활성화된 Scene만 빌드에 포함되도록 설정 --------------------------------------------
		string[] scenesToBuild = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
		Debug.Log("Scenes to build : " + scenesToBuild.Aggregate((a, b) => a + ", " + b));

		Debug.Log(_appBuildSetting.ToString());

		//================================================================
		// 빌드!!
		//================================================================
		BuildReport buildReport = BuildPipeline.BuildPlayer(scenesToBuild, outputFullPath, _buildTarget, buildOption);
		switch(buildReport.summary.result)
		{
			case BuildResult.Succeeded:
				{
					string reportStr = string.Empty;
					foreach (var step in buildReport.steps)
					{
						reportStr += $"Step Name : {step.name}, Duration : {step.duration}, Depth : {step.depth}\n";
						foreach (var msg in step.messages)
						{
							reportStr += $"{msg.content}\n";
						}
					}

					Debug.Log($"[BUILD] Report: {reportStr}");
					Debug.Log($"[BUILD] Succeeded! | OutputPath: {outputFullPath}, TotalTime: {buildReport.summary.totalTime}, TotalSize: {(buildReport.summary.totalSize / 1024).ToString("N0")} KBytes");

					RevealOutputInFinder(outputFullPath);
				}
				break;
			default:
				{
					Debug.Log($"[BUILD] {buildReport.summary.result}");
				}
				break;
		}

		return buildReport.summary.result == BuildResult.Succeeded;
	}

	/// <summary>
	/// <see cref="Environment.CommandLine"/> 정보중 빌드 설정에 맞는 데이터 파싱해서 적용
	/// </summary>
	static void ApplyCustomArgumentsToBuildSettings(Dictionary<string, string> customArgDic, ref AppBuildSettings buildSettings)
	{
		if (customArgDic.TryGetValue("NTSDK_CONFIG", out var ntsdkConfigName))
		{
			buildSettings.NTSDK_ConfigName = ntsdkConfigName.Trim();
		}

		if (customArgDic.TryGetValue("BUNDLE_VERSION", out var bundleVersionStr))
		{
			buildSettings.VersionCode = int.Parse(bundleVersionStr);
		}
		else
			buildSettings.VersionCode = PlayerSettings.Android.bundleVersionCode;

		if (customArgDic.TryGetValue("DEFINE_SYMBOLS", out var defineSymbolsStr))
		{
			List<string> customDefinedList = new List<string>();
			string[] symbolsStringArray = defineSymbolsStr.Split(new char[] { '&' }, StringSplitOptions.None);
			for (int i = 0; i < symbolsStringArray.Length; i++)
			{
				if (!string.IsNullOrEmpty(symbolsStringArray[i]) && symbolsStringArray[i] != "")
				{
					Debug.Log($"In CustomDefine : {symbolsStringArray[i]}");
					customDefinedList.Add(symbolsStringArray[i]);
				}
			}

			buildSettings.CustomDefines = customDefinedList;
		}

		if (customArgDic.TryGetValue("DEVELOPMENT_BUILD", out var devBuildStr))
		{
			buildSettings.IsDevelopmentBuild = bool.Parse(devBuildStr);
		}

		if (customArgDic.TryGetValue("STACKTRACE_LOGTYPE", out var stackTraceLogStr))
		{
			if (!System.Enum.TryParse<StackTraceLogType>(stackTraceLogStr, true, out buildSettings.StackTraceLogType))
			{
				Debug.LogWarning($"CustomArgument[STACKTRACE_LOG_TYPE] 파싱 불가로 \" {buildSettings.StackTraceLogType}\" 로 강제 설정됨.");
			}
		}

		if (customArgDic.TryGetValue("SVN_UPLOAD", out var svnUploadStr))
		{
			buildSettings.DoCommitToSVN = bool.Parse(svnUploadStr);
		}

		if (customArgDic.TryGetValue("FTP_UPLOAD", out var ftpUploadStr))
		{
			buildSettings.DoUploadToFTP = bool.Parse(ftpUploadStr);
		}

		if (customArgDic.TryGetValue("SEND_NOTIFY", out var sendNotifyStr))
		{
			buildSettings.DoSendNotify = bool.Parse(sendNotifyStr);
		}
	}

	/// <summary> 전처리기문 프로젝트에 적용 </summary>
	/// <param name="_setting"></param>
	static void ApplyDefineSymbols(AppBuildSettings _setting)
	{
		var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

		var oldDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
		List<string> newSymbolList = new List<string>(oldDefineSymbols.Split(',', ';', '\n', '\r'));

		// 전처리기 목록에 추가
		foreach (var s in _setting.CustomDefines.Where(x => x.IndexOf("!") != 0 && !newSymbolList.Contains(x)))
		{
			newSymbolList.Add(s);
		}

		// 전처리기 목록에서 빼기
		foreach (var s in _setting.CustomDefines.Where(x => x.IndexOf("!") == 0 && newSymbolList.Contains(x.Substring(1))))
		{
			newSymbolList.Remove(s.Substring(1));
		}

		string symbols = newSymbolList.Count == 0 ? "" : newSymbolList.Aggregate((a, b) => a + ";" + b);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);

		Debug.LogFormat($"{nameof(ApplyDefineSymbols)}() | DefineSymbol is updated : {oldDefineSymbols} -> {symbols}");
	}

	/// <summary>
	/// NTSDK의 스토어설정에 따라 패키지명 적용
	/// </summary>
	public static bool AdjustPackageName(AppBuildSettings _setting)
	{
		switch (_setting.NTSDK_SetupData.storeCD)
		{
			case NTCore.StoreCD.GOOGLE_PLAY:
				{
					_setting.PackageName = GooglePackageName;
				}
				break;
			case NTCore.StoreCD.APPLE_APP_STORE:
				{
					_setting.PackageName = ApplePackageName;
				}
				break;
			case NTCore.StoreCD.ONESTORE:
				{
					_setting.PackageName = OnestorePackageName;
				}
				break;
			default:
				_setting.PackageName = Application.identifier;
				break;
		}

		return true;
	}

	/// <summary> 알맞은 빌드 파일명을 조합해준다. </summary>
	public static bool AdjustOutputFileName(AppBuildSettings _setting)
	{
		//라인 권장: [마켓구분] _[게임코드] _[버전규칙명]_[REAL / QA] _[빌드전달일자]
		//string combinedStr = $"{_setting.NTSDK_SetupData.storeCD}_{_setting.NTSDK_SetupData.gameCode}_v{_setting.NTSDK_SetupData.clientVersion}.{_setting.VersionCode.ToString()}_{_setting.NTSDK_SetupData.DomainType}_{System.DateTime.Now.Year.ToString()}{System.DateTime.Now.Month.ToString("00")}{System.DateTime.Now.Day.ToString("00")}_r{_setting.Revision.ToString()}";

		//제로 스타일
		string combinedStr = $"{_setting.NTSDK_SetupData.gameCode}_{_setting.NTSDK_SetupData.DomainType}_{_setting.NTSDK_SetupData.storeCD}_{_setting.NTSDK_SetupData.clientVersion}_v{_setting.VersionCode.ToString()}_{System.DateTime.Now.Year.ToString()}{System.DateTime.Now.Month.ToString("00")}{System.DateTime.Now.Day.ToString("00")}_r{_setting.Revision}{(_setting.IsDevelopmentBuild ? "_Debug" : string.Empty)}";

		_setting.OutputFileName = combinedStr;

		Debug.Log($"AdjustOutputFileName() | OutputFileName: {combinedStr}");
		
		return false;
	}

	static void LogBuildError(object message)
	{
		Debug.LogError($"[FAIL] {message}");
		
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
			EditorApplication.Exit(1);
	}

	/// <summary> 안드로이드 전용 PlayerSettings 설정 적용 </summary>
	public static void EditPlayerSettings_Android()
	{
		PlayerSettings.Android.startInFullscreen = true;

		PlayerSettings.Android.androidTVCompatibility = false;
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
		PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

		PlayerSettings.Android.androidIsGame = true;
		PlayerSettings.Android.disableDepthAndStencilBuffers = false;
		PlayerSettings.Android.ARCoreEnabled = false;
		PlayerSettings.Android.showActivityIndicatorOnLoading = AndroidShowActivityIndicatorOnLoading.DontShow;

		// Keystore 설정 하기 (이거 어디서 읽어오게 해야되겠군)
		PlayerSettings.Android.useCustomKeystore = true;
		PlayerSettings.Android.keystoreName = "ie.jks";
		PlayerSettings.Android.keystorePass = "DlzkDlxjsjf13@$";
		PlayerSettings.Android.keyaliasName = "linegames";
		PlayerSettings.Android.keyaliasPass = "LineGames7!";
	}

	/// <summary> PlayerSettings에 Other Settings에 Configuration 섹션 </summary>
	public static void EditPlayerSettings_Configuration(BuildTargetGroup targetGroup, ScriptingImplementation inScriptingImplementation = ScriptingImplementation.IL2CPP, Il2CppCompilerConfiguration inCompilerConfig = Il2CppCompilerConfiguration.Debug)
	{
		PlayerSettings.SetApiCompatibilityLevel(targetGroup, ApiCompatibilityLevel.NET_4_6);
		// Devcat.EnumDictionary 사용해서 추가됨.
		PlayerSettings.allowUnsafeCode = true;

		if (inScriptingImplementation == ScriptingImplementation.IL2CPP)
		{
			PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.IL2CPP);
			PlayerSettings.SetIl2CppCompilerConfiguration(targetGroup, inCompilerConfig);

			// 앱플레이어들 때문에 ARMv7 추가함.
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

		}
		else
		{
			PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.Mono2x);
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
		}

		Debug.Log($"ProjectArchitectureSettings() | Scripting Backendl: {PlayerSettings.GetScriptingBackend(targetGroup)} 설정");
		Debug.Log($"ProjectArchitectureSettings() | Managed Stripping Level: {PlayerSettings.GetManagedStrippingLevel(targetGroup)} 설정");
		if (inScriptingImplementation == ScriptingImplementation.IL2CPP)
			Debug.Log($"ProjectArchitectureSettings() | C++ Compiler Configuration: {PlayerSettings.GetIl2CppCompilerConfiguration(targetGroup)} 설정");
		Debug.Log($"ProjectArchitectureSettings() | Target Architectures: {PlayerSettings.Android.targetArchitectures}"); 
		//Debug.Log($"ProjectArchitectureSettings() | Target Architectures: {PlayerSettings.GetArchitecture(targetGroup)} 설정 (0 - None, 1 - ARM64, 2 - Universal.)");

		// LG V30에서 UI앞에 모델나오면 크래시나서 끔
		//PlayerSettings.gpuSkinning = false;
	}

	/// <summary> PlayerSettings에 Other Settings에 Optimization 섹션 </summary>
	public static void EditPlayerSettings_Optimization(BuildTargetGroup targetGroup, ScriptingImplementation inScriptingImplementation = ScriptingImplementation.IL2CPP)
	{
		if (inScriptingImplementation == ScriptingImplementation.IL2CPP)
		{
			PlayerSettings.SetManagedStrippingLevel(targetGroup, ManagedStrippingLevel.High);
		}
		else
		{
			PlayerSettings.SetManagedStrippingLevel(targetGroup, ManagedStrippingLevel.Disabled);
		}

		PlayerSettings.stripEngineCode = true;
		PlayerSettings.enableInternalProfiler = false;
		// Optimize Mesh Data
		PlayerSettings.stripUnusedMeshComponents = true;
	}

	/// <summary>
	/// Assert(Full), Exception(ScriptOnly) 제외하고 로그 출력시, <see cref="StackTraceLogType"/> 설정에 맞는 StackTrace 출력
	/// </summary>
	public static void EditPlayerSettings_StackTrace(StackTraceLogType stackTraceLog)
	{
		foreach (LogType logType in Enum.GetValues(typeof(LogType)))
		{
			// 예외처리는 스택 호출 무조건 보이도록 하자.
			switch (logType)
			{
				case LogType.Assert:
					PlayerSettings.SetStackTraceLogType(logType, StackTraceLogType.Full);
					break;
				case LogType.Exception:
					PlayerSettings.SetStackTraceLogType(logType, StackTraceLogType.ScriptOnly);
					break;
				default:
					PlayerSettings.SetStackTraceLogType(logType, stackTraceLog);
					break;
			}

			Debug.Log($"EditPlayerSettings_StackTrace() | {logType}: {PlayerSettings.GetStackTraceLogType(logType)}");
		}
	}

	#region ================|| Subversion Control ||================

	public enum eSVNRevsionType
	{
		/// <summary>latest in repository</summary>
		HEAD,
		/// <summary>base rev of item's working copy</summary>
		BASE,
		/// <summary>last commit at or before BASE</summary>
		COMMITTED,
		/// <summary>revision just before COMMITTED</summary>
		PREV,
	}

	/// <summary>
	/// 해당 폴더의 HEAD 값 읽기. SVN.exe 파일 있는 PC에서만 실행됨.
	/// </summary>
	public static void GetSVNRevision(string rootPath, eSVNRevsionType revisionType, ref int foundRevision)
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

		System.Diagnostics.Process process = null;

		try
		{
			foundRevision = 0;
			System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("svn", $"info {rootPath} --revision {revisionType}");
			procStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			procStartInfo.RedirectStandardOutput = true;
			procStartInfo.RedirectStandardError = true;
			procStartInfo.UseShellExecute = false;
			// 한글 로그를 위한 설정
			//procStartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
			//procStartInfo.StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

			process = System.Diagnostics.Process.Start(procStartInfo);

			string resultStr = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogError($"GetSVNRevision() error | {error}");
			}

			foreach (var strLine in resultStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (string.IsNullOrEmpty(strLine))
					continue;

				if (!strLine.StartsWith("Last Changed Rev:") && !strLine.StartsWith("마지막 수정 리비전:"))
					continue;

				if (int.TryParse(strLine.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1], out foundRevision))
					break;
			}

			process?.Close();
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"svn이 존재하는지, 환경변수에 등록되어 있는지 확인 필요! Message: {e.Message}");
		}
		finally
		{
			process?.Close();
		}
#else
#endif
	}

	public static void UpdateSVNInfo(string rootPath, System.Action OnUpdateEnd)
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		try
		{
			System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("svn");
			procStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			procStartInfo.UseShellExecute = false;
			procStartInfo.Arguments = string.Format("update {0}", rootPath);
			procStartInfo.RedirectStandardOutput = true;
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo = procStartInfo;
			process.EnableRaisingEvents = true;
			process.Exited += new EventHandler((obj, evt) =>
			{
				Debug.Log("proc_ Exited " + evt);
				OnUpdateEnd.Invoke();
			});
			process.Start();
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"svn이 존재하는지, 환경변수에 등록되어 있는지 확인 필요! Message: {e.Message}");
		}
#endif
	}
	#endregion

	#region ================|| Line Message Sender ||================

	public static void SendLineBotMessage(string Message, bool bSucc = true)
	{
		List<ValueTuple<int, int>> succList = new List<(int, int)>() { (1, 14), (1, 13), (2, 22), (2, 179) };
		List<ValueTuple<int, int>> failList = new List<(int, int)>() { (1, 105), (1, 109), (1, 111), (2, 25), (2, 39) };

		if (bSucc)
		{
			var sticker = succList[UnityEngine.Random.Range(0, succList.Count)];

			SendLineMessage(Message, sticker.Item1, sticker.Item2);
		}
		else
		{
			var sticker = failList[UnityEngine.Random.Range(0, failList.Count)];

			SendLineMessage(Message, sticker.Item1, sticker.Item2);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="htmlText"></param>
	/// <param name="stickerPackNo"></param>
	/// <param name="StickerNo"></param>
	/// <remarks>https://notify-bot.line.me/my/ 에서 token생성 필요</remarks>
	public static void SendLineMessage(string htmlText, int stickerPackNo = 0, int StickerNo = 0)
	{
		// [제로] 이카루스 - 클라이언트 :  Bearer 9TVnWPfPLf1BGjrX1bXilQZWcFgLpglWK58x401OwNM
		// 프로그램(클라+서버) :  Bearer lbtZJobWl9MlbKfmE7bh0AwmJ6HX9fXTJQFjNcFV307

		List<string> lineAccessTokens = new List<string>()
		{
			"Bearer 9TVnWPfPLf1BGjrX1bXilQZWcFgLpglWK58x401OwNM",
			"Bearer lbtZJobWl9MlbKfmE7bh0AwmJ6HX9fXTJQFjNcFV307",
		};

		foreach (var token in lineAccessTokens)
		{
			WWWForm form = new WWWForm();

			//if (stickerPackNo != 0 && StickerNo != 0)
			//{
			//	form.AddField("stickerPackageId", stickerPackNo);
			//	form.AddField("stickerId", StickerNo);
			//}

			form.AddField("message", htmlText);
			// 라인 | [클라이언트_알림방]에 보내고있음
			UnityWebRequest www = UnityWebRequest.Post("https://notify-api.line.me/api/notify", form);

			www.SetRequestHeader("Authorization", token);

			var async = www.SendWebRequest();

			// 강제 대기
			while (!async.isDone) ;
		}
	}

	#endregion
	
	/// <summary> BatchMode가 아니라면 해당경로 탐색기 띄우기 </summary>
	public static void RevealOutputInFinder(string path)
	{
		if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
			return;

		var parent = Path.GetDirectoryName(path);
		EditorUtility.RevealInFinder(
			(Directory.Exists(path) || File.Exists(path)) ? path :
			(Directory.Exists(parent) || File.Exists(parent)) ? parent :
			Environment.CurrentDirectory.Replace('\\', '/')
		);
	}
}