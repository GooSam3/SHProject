using UnityEngine;

/// <summary>
/// 게임 시작시 설정가능한 셋팅 값들 모음
/// </summary>
/// <remarks>
/// 테스트시 개발자별로 자기 전용 프리셋 만들어 두기 용도로 사용하면 좋을듯.
/// </remarks>
[System.Serializable]
public abstract class GameStarterDataBase<OWNER_TYPE> : ScriptableObject
{
	/// <summary>
	/// 
	/// </summary>
	public OWNER_TYPE Owner { get; private set; }

	/// <summary>
	/// 빌드에 따른 최초 정보 획득 주소
	/// </summary>
	[System.Obsolete("LineSDK로 대체될 예정. ZGameManager.ServerAddr로 대체")]
	public string AuthUrl
	{
		get
		{
#if BUILD_LIVE
			return mNewAuthUrl_LIVE ?? mAuthUrl_LIVE;
#elif BUILD_QA
			return mNewAuthUrl_QA?? mAuthUrl_QA;
#else
			return mNewAuthUrl_DEV ?? mAuthUrl_DEV;
#endif
		}
	}

	/// <summary> 현재 빌드 형식 이름 </summary>
	[System.Obsolete("LineSDK로 대체될 예정")]
	public string BuildTypeName
	{
		get
		{
#if BUILD_LIVE
			return "LIVE";
#elif BUILD_QA
			return "QA";
#else
			return "DEV";
#endif
		}
	}

	[Header("최초 정보 획득을 위한 접근 URL")]
	[SerializeField]
	private string mAuthUrl_DEV = "http://10.95.162.57:81/icarus/";
	[SerializeField]
	private string mAuthUrl_QA = "http://3.34.251.70:81/icarus/";
	[SerializeField]
	private string mAuthUrl_LIVE = "http://3.34.251.70:81/icarus/";

	[System.NonSerialized]
	private string mNewAuthUrl_DEV = null;
	[System.NonSerialized]
	private string mNewAuthUrl_QA = null;
	[System.NonSerialized]
	private string mNewAuthUrl_LIVE = null;

	/// <summary> Prefab으로 존재하는 매니저 객체들 미리 생성할지 여부 (Resources폴더에 존재하는 것들) </summary>
	public bool ReadyManagers = true;

	/// <summary> 치트기능 작동 여부 </summary>
	public bool UsableCheat = true;

	/// <summary>
	/// </summary>
	public virtual void DoStart(OWNER_TYPE _owner)
	{
		this.Owner = _owner;
		this.OnStart();
	}

	/// <summary> 시작 처리 구현부 </summary>
	protected abstract void OnStart();

	/// <summary>
	/// 강제로 Auth주소 수정 (NTSDK를 위해 만듬)
	/// </summary>
	[System.Obsolete("LineSDK로 대체될 예정")]
	public void ForceChangeAuthURL(string newAuthUrl)
	{
#if BUILD_LIVE
		mNewAuthUrl_LIVE = newAuthUrl;
#elif BUILD_QA
		mNewAuthUrl_QA = newAuthUrl;
#else
		mNewAuthUrl_DEV = newAuthUrl;
#endif
	}
}

