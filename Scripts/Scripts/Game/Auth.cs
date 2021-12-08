//#define BUILD_QA
//#define BUILD_LIVE

using System.Collections.Generic;
using TinyJSON;


/// <summary>
/// 공인된 주소 정보들
/// </summary>
/// <remarks>
/// 서버 접속 주소
/// 각종 데이터 다운로드/업로드 주소들
///		- TableData, Bundle, Image 등등
/// </remarks>
public class Auth
{
	#region	for LineGames SDK

	/// <summary>
	/// 통합툴에 설정한 서버 주소
	/// </summary>
	public static string ServerAddr;
	/// <summary>
	/// 통합툴에 설정한 패치 주소
	/// </summary>
	public static string PatchAddr;
	/// <summary> 각종 카테고리별 다운로드 받을 Url 모음</summary>
	public static AssetURLInfo AssetUrlShortcut = new AssetURLInfo();

	/// <summary> 다운로드 데이터 저장 폴더 </summary>
	public static string DataPath
	{
		get
		{
			return UnityEngine.Application.persistentDataPath;
		}
	}

	/// <summary>
	/// 각종 에셋들 다운로드/업로드 주소 모음
	/// </summary>
	public class AssetURLInfo
	{
		/// <summary> 에셋 종류들이 모아져 있는 저장소의 루트 주소 </summary>
		public string BaseUrl { get; private set; }
		public string AssetUrl { get; private set; }
		public string BinUrl { get; private set; }
		/// <summary> 로딩 이미지 다운로드용 </summary>
		public string LoadingUrl { get; private set; }
		/// <summary> Test 다운로드용 </summary>
		public string TestUrl { get; private set; }

        /// <summary> 패킷 정보 다운로드용 </summary>
        public string PacketInfoUrl { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_baseUrl"> 다운로드(파일서버) 주소 최상위 경로 </param>
        public void Setup(string _baseUrl)
		{
			ZLog.Log(ZLogChannel.Default, $"{nameof(AssetURLInfo)}.{nameof(AssetURLInfo.Setup)} | BaseUrl: {_baseUrl}");

			BaseUrl = _baseUrl;

#if UNITY_ANDROID && !UNITY_EDITOR
            AssetUrl = $"{_baseUrl}/bundle/android/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
			AssetUrl = $"{_baseUrl}/bundle/pc/";
#else
			AssetUrl = $"{_baseUrl}/bundle/android/";
#endif

			BinUrl = $"{_baseUrl}/bin/";
			TestUrl = $"{_baseUrl}/test/";
			LoadingUrl = $"{_baseUrl}/loading/";

            PacketInfoUrl = $"{_baseUrl}/packet/";

            ZLog.Log(ZLogChannel.Default, $"{nameof(BaseUrl)} : {BaseUrl}" +
				$"\n{nameof(AssetUrl)} : {AssetUrl}" +
				$"\n{nameof(BinUrl)} : {BinUrl}" +
				$"\n{nameof(LoadingUrl)} : {LoadingUrl}" +
                $"\n{nameof(PacketInfoUrl)} : {PacketInfoUrl}");

            ZNetPacketConfig.CheckLastestInfo();
        }
	}

	#endregion
}
