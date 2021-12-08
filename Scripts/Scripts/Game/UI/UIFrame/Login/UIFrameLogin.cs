using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIFrameLogin : ZUIFrameBase
{
	#region External
	public UILoginIntroVideo IntroVideo { get; private set; } = null;
	public UILoginPlatform Platform { get; private set; } = null;
	public UILoginNetwork Network { get; private set; } = null;
	public UILoginPopupServerList PopupServerList { get; private set; } = null;
	public UILoginPopupServerWait PopupServerWait { get; private set; } = null;
	#endregion

	#region Variable
	[field: SerializeField] public GameObject LoginBody { get; private set; } = null;

	/// <summary>서버 선택 후 로그인 시도하는 버튼</summary>
	[field: SerializeField] public ZButton StartBoardBtn { get; private set; } = null;
	/// <summary>서버 리스트 요청 시도하는 버튼</summary>
	[field: SerializeField] public ZButton StartLoginBtn { get; private set; } = null;

	[field: SerializeField] public Text SelectServerName { get; private set; } = null;

	#region Intro Video
	[field: SerializeField] public VideoPlayer VideoPlayer { get; private set; } = null;
	#endregion

	#region Server List
	public List<UILoginServerListItem> ServerList = new List<UILoginServerListItem>();

	[field: SerializeField] public GameObject ServerListPopupObject { get; private set; } = null;

	[field: SerializeField] public UILoginEnum.E_ServerListTab SelectServerTab { get; private set; } = UILoginEnum.E_ServerListTab.All;
	[field: SerializeField] public ScrollRect ServerListScroll { get; private set; } = null;
	#endregion

	#region Server Wait
	[field: SerializeField] public GameObject ServerWaitPopupObject { get; private set; } = null;
	[field: SerializeField] public Text ServerWaitCount { get; private set; } = null;
	#endregion

	#region Platform
	[field: SerializeField] public GameObject PlatformLoginGroupObject { get; private set; } = null;
	[field: SerializeField] public ZButton AppleBtn { get; private set; } = null;
	[field: SerializeField] public ZButton FacebookBtn { get; private set; } = null;
	[field: SerializeField] public ZButton GoogleBtn { get; private set; } = null;
	[field: SerializeField] public ZButton GuestBtn { get; private set; } = null;
	[field: SerializeField] public ZButton LogoutBtn { get; private set; } = null;
	[field: SerializeField] public Text NoPlatformEnabledText { get; private set; } = null;
	[field: SerializeField] public InputField GuestIDField { get; private set; } = null;
	[field: SerializeField] public GameObject LogoutPromptObject { get; private set; } = null;

	[field: SerializeField] public Text LogoutPromptStatus { get; private set; } = null;

	/// <summary> 커스텀 로그인용ID (null이면 현재 디바이스 고유ID로 생성됨) </summary>
	[field: SerializeField] public string CustomAccountID { get; private set; } = string.Empty;

	//[field: SerializeField] public Coroutine ChangePlatformRoutine { get; private set; } = null;
	#endregion
	#endregion


	protected override void OnInitialize()
	{
		base.OnInitialize();

		IntroVideo = gameObject.AddComponent<UILoginIntroVideo>();
		Network = gameObject.AddComponent<UILoginNetwork>();
		Platform = gameObject.AddComponent<UILoginPlatform>();
		PopupServerList = gameObject.AddComponent<UILoginPopupServerList>();
		PopupServerWait = gameObject.AddComponent<UILoginPopupServerWait>();

		IntroVideo.Initialize(this);
		Network.Initialize(this);
		Platform.Initialize(this);
		PopupServerList.Initialize(this);
		PopupServerWait.Initialize(this);

		StartBoardBtn.SetListener(Network.CheckAccount);
		StartLoginBtn.SetListener(Network.OnConnectServer);

		LoginBody.SetActive(false);

		if (NoPlatformEnabledText != null)
			NoPlatformEnabledText.gameObject.SetActive(false);

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UILoginServerListItem), delegate {
			AudioManager.Instance.Play(DBSound.BGM_MainTitleID);

			#region Account Id Setting
			CustomAccountID = GuestIDField.text = PlayerPrefs.GetString($"{Application.dataPath}_AccountId");
			GuestIDField.onValueChanged.AddListener(OnGuestIDFieldChanged);
			#endregion
		});
	}

	private void OnGuestIDFieldChanged(string txt)
	{
		CustomAccountID = txt;
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UILoginServerListItem));
	}

	#region Input
	/// <summary>서버 리스트 팝업 Open or Close</summary>
	/// <param name="_open"> Open 여부</param>
	public void OnClickOpenServerListPopup(bool _open)
	{
		if(PopupServerList != null)
			PopupServerList.SwitchServerList(_open);
	}

	/// <summary>UUID 값 입력</summary>
	/// <param name="_id"></param>
	public void OnClickEndEditAccountId() 
	{
		CustomAccountID = GuestIDField.text;
	}

	public void OnClickLogoutPromptBtn()
	{
		if (Platform != null)
			Platform.ChangeAccountProcess();
	}

	public void OnClickWaitServerConnectCancel()
    {
		if (PopupServerWait != null)
			PopupServerWait.SwitchWaitPopup(false);
	}
	#endregion

	#region Data
	public void SetLogoutPromptStatusText(string _status) { LogoutPromptStatus.text = _status; }
	public void SetSelectServerTab(UILoginEnum.E_ServerListTab _tabIdx) { SelectServerTab = _tabIdx; }
	//public void SetPlatformRoutine(Coroutine _routine) { ChangePlatformRoutine = _routine; }
	public void SetServerWaitCountText(string _count) { ServerWaitCount.text = _count; }
	#endregion

	protected virtual void Initialize(ZUIFrameBase _frame) { }
}