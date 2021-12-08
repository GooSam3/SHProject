using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebNet;
using WebSocketSharp;
using ZNet;
using ZNet.Data;

public class UIFrameCharacterSelect : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private GameObject SelectWindow = null;
    [SerializeField] private GameObject CreateWindow = null;
    [SerializeField] private ScrollRect CharSelectListScroll = null;
    [SerializeField] private ScrollRect CharCreateListScroll = null;
    [SerializeField] private GameObject NickNamePopup = null;
    [SerializeField] private InputField InputNickName = null;
    [SerializeField] private ZToggle[] AttributeBtn = new ZToggle[ZUIConstant.CHARACTER_ATTRIBUTE_TYPE_COUNT];
    [SerializeField] private GameObject[] AttributeEffect = new GameObject[ZUIConstant.CHARACTER_ATTRIBUTE_TYPE_COUNT];
    [SerializeField] private GameObject[] AttributeEffectCreate = new GameObject[ZUIConstant.CHARACTER_ATTRIBUTE_TYPE_COUNT];
    [SerializeField] private Image AttributeIcon = null;
    [SerializeField] private Image CharClassIcon = null;
    [SerializeField] private Text AttributeLevel = null;
    [SerializeField] private GameObject AttributePopup = null;
    [SerializeField] private GameObject ClassInfoPopup = null;
    [SerializeField] private GameObject Info = null;
    [SerializeField] private Button CreateButton = null;
    [SerializeField] private Text[] StatusPower = new Text[ZUIConstant.CHARACTER_STATUS_COUNT];
    [SerializeField] private GameObject GuildInfoObj = null;
    [SerializeField] private Image GuildIcon = null;
    [SerializeField] private Text GuildLevel = null;
    [SerializeField] private Button DeleteBtn = null;
    [SerializeField] private Button DeleteRollbackBtn = null;
    [SerializeField] private ZToggleGroup CreateBtnGroup = null;
    [SerializeField] private ZToggleGroup SelectBtnGroup = null;
    #endregion

    #region System Variable
    [SerializeField] private List<UICharacterSelectListItem> CharSelectList = new List<UICharacterSelectListItem>();
    [SerializeField] private List<UICharacterCreateListItem> CharCreateList = new List<UICharacterCreateListItem>();
    [SerializeField] private ulong CreateCharId = 0;
    public E_UnitAttributeType SelectAttribute { get; private set; } = E_UnitAttributeType.Fire;
    public int SelectSlot { get; private set; } = 0;
    public int CreateSlot { get; private set; } = 0;
    #endregion

    #region Another Variable
    [SerializeField] private Dictionary<uint, string> m_dicMMOServer = new Dictionary<uint, string>();
    [SerializeField] private ZPawnLobbyPc LobbyPc = null;
	private GameObject PCDirectorGO = null;
	private PlayableDirector PCDirector = null;
	private uint DirectorPcTid = 0;

	/// <summary>선택창용 카메라 환경</summary>
	private GameObject SelectRoot;
	/// <summary>생성창용 카메라 환경</summary>
	private GameObject CreationRoot;
	private GameObject GlobalRoot;
	#endregion


	protected override void OnInitialize()
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICharacterSelectListItem), delegate {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICharacterCreateListItem), delegate {
                Initialize();
            });
        });
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICharacterSelectListItem));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UICharacterCreateListItem));
    }

    /// <summary> 캐릭터 생성, 선택 리스트 초기화 및 세팅 </summary>
	private void Initialize()
    {
		AudioManager.Instance.Play(DBSound.BGM_CharacterSelect);
		
		// 모드별 환경용 (Creation 씬상 객체들)
		Scene scene = SceneManager.GetActiveScene();
		if (scene.isLoaded)
		{
			var gmaeObjs = new List<GameObject>();
			scene.GetRootGameObjects(gmaeObjs);

			SelectRoot = gmaeObjs.Find(go => go.name == "Select_Root");
			CreationRoot = gmaeObjs.Find(go => go.name == "Creation_Root");
			GlobalRoot = gmaeObjs.Find(go => go.name == "Global_Root");
		}
		
		m_dicMMOServer.Clear();

        ClearCreateSlotList();
        ClearSelectSlotList();

        // 이미 생성한 캐릭터 리스트 출력
        ZWebManager.Instance.WebGame.REQ_CharacterList((recvPacket, resCharList) =>
        {
            int cnt = 0;
            UIManager.Instance.Close<UIFrameLoadingScreen>();
            foreach (var pair in Me.CurUserData.DicChar)
            {
                UICharacterSelectListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICharacterSelectListItem)).GetComponent<UICharacterSelectListItem>();
                
                if(obj != null)
                {
					void callBack()
					{
						Me.CharID = pair.Value.ID;
						m_dicMMOServer.Clear();
						uint tid = UICommon.GetCharTid(DBCharacter.Get(pair.Value.TID).CharacterTextID, DBCharacter.GetAttributeType(pair.Value.TID));

						uint changeTid = 0;
                        var charData = DBCharacter.Get(tid);
                        E_UnitAttributeType attributeType = charData.AttributeType;

                        //강림 시간이 남아있을 경우에만 강림출력해줌
                        if (Me.CurCharData.CurrentMainChange != 0)
						{
							changeTid = Me.CurCharData.MainChange;
                            var targetChangeData = DBChange.Get(changeTid);

                            if(targetChangeData != null)
                            {
                                attributeType = targetChangeData.AttributeType;
                            }
                        }

						SetLobbyCharacter(tid, changeTid);

                        /// 강림체가 존재하는 경우 강림체의 속성을 출력한다
						AttributeIcon.sprite = UICommon.GetCharAttributeSprite(attributeType);
                        uint attributeLevel = 0;

                        if (resCharList.Attribute.HasValue)
                        {
                            for (int i = 0; i < resCharList.Attribute.Value.AttributeTidsLength; i++)
                            {
                                DBAttribute.GetAttributeByID(resCharList.Attribute.Value.AttributeTids(i), out var attributeData);

                                if(attributeData != null 
                                && attributeData.AttributeType == attributeType)
                                {
                                    attributeLevel = attributeData.AttributeLevel;
                                    break;
                                }
                            }
                        }

                        AttributeLevel.text =  string.Format(DBLocale.GetText("Attribute_Level"), attributeLevel);

                        DrawAttributeEffect(pair.Value.TID);

						// 길드 정보
						GuildInfoObj.gameObject.SetActive(pair.Value.GuildId != 0);
						if (pair.Value.GuildId != 0)
						{
							GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(pair.Value.GuildMarkTid));
							GuildLevel.text = pair.Value.GuildGrade.ToString();
						}
					}

					obj.transform.SetParent(CharSelectListScroll.content.transform, false);
                    obj.Initialize(cnt, pair.Value.ID, pair.Value.TID, pair.Value.LastStageTID, pair.Value.Nickname, pair.Value.Level, false, pair.Value.LastLogoutDt, pair.Value.DeleteDt, callBack);
                    CharSelectList.Add(obj);
                }

                cnt++;
            }

            // 마지막 접속한 캐릭이 최상위로 가도록 Sort
            CharSelectList = CharSelectList.OrderByDescending(item => item.LastLogoutDt).ToList();

            if (CreateSlot != 0)
            {
                SelectSlot = GetCharSlotCount(CreateCharId);
                CreateSlot = 0;
                CreateCharId = 0;
            }

            for (int i = 0; i < CharSelectList.Count; i++)
            {
                CharSelectList[i].gameObject.transform.SetSiblingIndex(i);
                CharSelectList[i].SetSlotIdx(i);
            }     

            // 생성 가능한 리스트 출력
            if (Me.CurUserData.DicChar.Count < DBConfig.Max_Character_Slot_Count)
            {
                for (int i = 0; i < DBConfig.Max_Character_Slot_Count - Me.CurUserData.DicChar.Count; i++)
                {
                    UICharacterSelectListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICharacterSelectListItem)).GetComponent<UICharacterSelectListItem>();

                    if(obj != null)
                    {
						void callBack()
						{
							ChangeUIState(true);
							SelectCreateCharacter(0);
						}

						obj.transform.SetParent(CharSelectListScroll.content.transform, false);                      
                        obj.Initialize(CharSelectList.Count, 0, 0, 0, string.Empty, 0, i >= Me.MaxCharCnt - Me.CurUserData.DicChar.Count, 0, 0, callBack);
                        CharSelectList.Add(obj);
                    }
                }
            }

            ChangeUIState(Me.CurUserData.DicChar.Count == 0);

            uint[] charList = new uint[] { 100000, 300000, 400000, 200000 };

            for (int i = 0; i < ZUIConstant.CHARACTER_CLASS_COUNT; i++)
            {
                UICharacterCreateListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UICharacterCreateListItem)).GetComponent<UICharacterCreateListItem>();

                if(obj != null)
                {
                    obj.transform.SetParent(CharCreateListScroll.content.transform, false);
                    obj.Initialize(i, charList[i]);

                    CharCreateList.Add(obj);
                }
            }
            DoInitilizeMenual();
            SelectCharacter();

            if (Me.CurUserData.DicChar.Count == 0)
                SelectCreateCharacter(0);
        });
    }

    public void RegisterCreateToggle(ZToggle _toggle)
    {
        _toggle.group = CreateBtnGroup;
        _toggle.ZToggleGroup = CreateBtnGroup;
    }

    public void RegisterSelectToggle(ZToggle _toggle)
    {
        _toggle.group = SelectBtnGroup;
        _toggle.ZToggleGroup = SelectBtnGroup;
    }

    private void TurnGlobalLight(bool isOn)
	{
		if (null != GlobalRoot)
			GlobalRoot.GetComponentInChildren<Light>().enabled = isOn;
	}

    /// <summary>캐릭터 선택 슬롯 선택 </summary>
    private void SelectCharacter()
    {
        RefreshSelectSlot(E_CharacterSelectState.Select);

        for (int i = 0; i < CharSelectList.Count; i++)
            if (CharSelectList[i].SlotIndex == SelectSlot) 
                CharSelectList[i].SelectButton(); 
    }

    /// <summary>캐릭터 생성 슬롯 선택</summary>
    /// <param name = "_slotIdx"> 슬롯 Idx </param>
    private void SelectCreateCharacter(int _slotIdx)
    {
        RefreshSelectSlot(E_CharacterSelectState.Create);

        for (int i = 0; i < CharCreateList.Count; i++)
            if (CharCreateList[i].SlotIndex == _slotIdx)
            {
                CharCreateList[i].SelectCharacter();
                SetCharStatus(CharCreateList[i].CharTid);
            }
    }

    /// <summary>캐릭터 선택 슬롯 Idx 설정 </summary>
    /// <param name="_selectState">설정하려는 캐릭터 창</param>
    /// <param name="_idx">슬롯 Idx</param>
    public void SetSelectSlot(E_CharacterSelectState _selectState, int _idx)
	{
        switch(_selectState)
		{
            case E_CharacterSelectState.Select: SelectSlot = _idx; break;
            case E_CharacterSelectState.Create: CreateSlot = _idx; break;
		}
	}

    /// <summary>캐릭터 삭제 버튼 활성화 / 비활성화 </summary>
    /// <param name = "_active"> 활성화 여부 </param>
    public void ActiveDeleteButton(bool _active)
    {
        DeleteBtn.gameObject.SetActive(_active);
    }

    /// <summary>캐릭터 삭제 복구 버튼 활성화 / 비활성화 </summary>
    /// <param name = "_active"> 활성화 여부 </param>
    public void ActiveDeleteRollbackButton(bool _active)
    {
        DeleteRollbackBtn.gameObject.SetActive(_active);
    }

    /// <summary>캐릭터 모델 파일 출력 </summary>
    /// <param name = "_characterTid"> 캐릭터 Tid </param>
    /// <param name = "_changeTid"> 변경할 캐릭터 Tid </param>
    private void SetLobbyCharacter(uint _characterTid, uint _changeTid)
    {
        if (null != LobbyPc)
        {
            LobbyPc.SetLobbyCharacter(_characterTid, _changeTid);
            LobbyPc.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            LobbyPc = ZPawnLobbyPc.CreateLobbyPc(_characterTid, _changeTid);
		}
    }

	/// <summary>
	/// 생성창용 캐릭터 생성.
	/// </summary>
	/// <param name="_characterTid"></param>
	private void SetCreateCharacter(uint _characterTid)
	{
		if (DirectorPcTid != _characterTid)
		{
			ClearPCDirector();

			DirectorPcTid = _characterTid;

			var charTable = DBCharacter.Get(_characterTid);			
			if (string.IsNullOrEmpty(charTable.CreationDirector))
			{
				ZLog.Log(ZLogChannel.UI, $"{_characterTid}용 생성 연출은 아직 없어서 기존 캐릭터 로드");

				// 연출없는 캐릭터 때문에 로비용 캐릭터 표시하도록 함.
				TurnGlobalLight(true);
				SetLobbyCharacter(_characterTid, 0);
			}
			else
			{
				ClearLobbyPc();

				ZResourceManager.Instance.Load(charTable.CreationDirector, (string _resName, GameObject _timeLineGO) =>
				{
					if (_timeLineGO == null)
					{
						UICommon.OpenSystemPopup_One(ZUIString.ERROR,
								"타임라인 파일 로드 실패.", ZUIString.LOCALE_OK_BUTTON);
						return;
					}

					// 생성창이 아니거나, 파괴된 상태라면 skip
					if (!CreationRoot.activeSelf || this == null)
					{
						ClearPCDirector();
						return;
					}

					TurnGlobalLight(false);

					PCDirectorGO = Instantiate(_timeLineGO);
					PCDirector = PCDirectorGO.GetComponentInChildren<PlayableDirector>();
					if (null != PCDirector && !PCDirector.playOnAwake)
					{
						PCDirector.Play();
					}
				});
			}
		}
	}

    /// <summary>캐릭터 선택 후 인게임 접속</summary>
    public void Connect()
    {
        if(CharSelectList[SelectSlot].DeleteDt > 0)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("삭제중인 캐릭터로 접속할 수 없습니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate { 
                    _popup.Close(); } });});
            return;
        }

        if (0 < CharSelectList[SelectSlot].StageTid)
        {
            Me.CharID = CharSelectList[SelectSlot].CharId;

            ZGameOption.Instance.LoadCharacterOption();
            PlayerPrefs.SetInt("SelectMMOChannelId", (int)0);
            ZGameManager.Instance.FSM.ChangeState(E_GameState.InGame);
        }
    }

    /// <summary>캐릭터 삭제</summary>
    public void Delete()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
            _popup.Open("캐릭터 삭제", 
                DBLocale.GetText("Delete_Character_Warn_Message"), 
                new string[] {  ZUIString.LOCALE_CANCEL_BUTTON , DBLocale.GetText("Delete_Button") }, 
                new Action[] {
                    delegate { _popup.Close(); } ,
                    delegate { DeletaCharacter(); _popup.Close(); }
                });

            _popup.SetButtonBG(1, "bt_box_red");
        });
    }

    /// <summary>서버에게 캐릭터 삭제 요청</summary>
    private void DeletaCharacter()
    {
        ZWebManager.Instance.WebGame.REQ_CharDelete(Me.UserID, CharSelectList[SelectSlot].CharId, NTCore.CommonAPI.DomainType != NTCore.DomainType.Real, (recvPacket, resCharList) =>
        {
            Initialize();
        }, OnError);
    }

    /// <summary>캐릭터 삭제 철회/summary>
    public void DeleteRollback()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
            _popup.Open(ZUIString.WARRING, DBLocale.GetText("Delete_Character_Cancel_Warn_Message"), new string[] { ZUIString.LOCALE_OK_BUTTON, ZUIString.LOCALE_CANCEL_BUTTON }, new Action[] { delegate { DeletaCharacterRollback(); _popup.Close(); }, delegate { _popup.Close(); } });
        });
    }

    /// <summary>서버에게 캐릭터 삭제 철회 요청</summary>
    private void DeletaCharacterRollback()
    {
        ZWebManager.Instance.WebGame.REQ_CharDeleteRollBack(Me.UserID, CharSelectList[SelectSlot].CharId, (recvPacket, resCharList) =>
        {
            Initialize();
        }, OnError);
    }

    /// <summary>캐릭터 생성</summary>
    public void Create()
    {
        if (InputNickName.text.IsNullOrEmpty())
        {
            ActiveNickNamePopup(true);
            return;
        }

        if(NTCommon.StringUtil.ValidateCommonName(InputNickName.text) == false)
		{
            UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                _popup.Open(ZUIString.WARRING, "닉네임에 특수 문자를 포함할 수 없습니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
                    _popup.Close();
                    NickNamePopup.SetActive(true);
                    InputNickName.text = string.Empty; } });
            });
            return;
        }
      
        string newCharName = InputNickName.text;
		newCharName = newCharName.Replace(".", ""); // Remove period

		if (newCharName.Length < DBConfig.NickName_Length_Min ||
			newCharName.Length > DBConfig.NickName_Length_Max)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                _popup.Open(ZUIString.ERROR, "닉네임 길이가 잘못되었습니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate { 
                    _popup.Close(); 
                    NickNamePopup.SetActive(true); 
                    InputNickName.text = string.Empty; } });});
            return;
        }

        CreateButton.interactable = false;

        ZWebManager.Instance.WebGame.REQ_CreateCharacter(GetCharTid(CharCreateList[CreateSlot - 1].CharTextId), newCharName, (recvPacket, resCharCreate) =>
		{
            CreateCharId = resCharCreate.CharId;

            ActiveNickNamePopup(false);
            CreateButton.interactable = true;
            InputNickName.text = string.Empty;
            SelectAttribute = E_UnitAttributeType.Fire;
            
            Initialize();
		}, OnError);
	}

    /// <summary> 선택 / 생성창 활성화 변경</summary>
    /// <param name = "_create"> 생성창 활성화 / 비활성화 여부 </param>
    private void ChangeUIState(bool _create)
    {
		if (_create)
			SelectAttribute = E_UnitAttributeType.Fire;
		else
			ClearPCDirector();

        CreateWindow.SetActive(_create);
        SelectWindow.SetActive(!_create);

        SwitchAttributeInfo(!_create);

        SelectRoot?.SetActive(!_create);
		CreationRoot?.SetActive(_create);

		TurnGlobalLight(!_create);

        if (_create && CharCreateList.Count > 0)
            CharCreateList[0].SelectToggleAction();
	}

    /// <summary>캐릭터 생성창에서 선택창으로 이동 </summary>
	public void BackSelect()
    {
        ChangeUIState(false);

        ClearPCDirector();

        if (Me.CurUserData.DicChar.Count > 0)
        {
            SelectSlot = 0;
            SelectCharacter();
        }
        else
        {
            ClearLobbyPc();
            ZWebManager.Instance.DisconnectAll();
            ZGameManager.Instance.FSM.ChangeState(E_GameState.Login);
        }
        
    }

    /// <summary>로비 3D 모델 캐릭터 제거</summary>
	private void ClearLobbyPc()
	{
		if (LobbyPc != null)
		{
			Destroy(LobbyPc.gameObject);
			LobbyPc = null;
		}
	}

	private void ClearPCDirector()
	{
		DirectorPcTid = 0;

		if (null != PCDirectorGO)
		{
			Destroy(PCDirectorGO.gameObject);
			PCDirectorGO = null;
			PCDirector = null;
		}
	}

    private int GetCharSlotCount(ulong _charId)
	{
        int count = 0;
        foreach (var value in Me.CurUserData.DicChar)
		{
            if (Me.CurUserData.DicChar.TryGetValue(_charId, out CharacterData data))
                return count;

            count++;
        }

        return count;
    }

    /// <summary> 캐릭터 선택창에서 로그인 창으로 이동 </summary>
    public void BackLogin()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
            _popup.Open(ZUIString.INFO,
                DBLocale.GetText("Move_Server_Selection"),
                new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
                new Action[] {
                    delegate { _popup.Close(); },
                    delegate { _popup.Close(); BackLoginAction(); }
                });
        });
    }

    void BackLoginAction()
    {
        ClearLobbyPc();
        ClearPCDirector();

        ZWebManager.Instance.DisconnectAll();
        ZGameManager.Instance.FSM.ChangeState(E_GameState.Login);
    }

    /// <summary>닉네임 입력창 오픈 </summary>
    /// <param name = "_open"> 닉네임 입력창 활성화 / 비활성화 </param>
    public void ActiveNickNamePopup(bool _open)
    {
 		NickNamePopup.SetActive(_open);

        InputNickName.text = string.Empty;
	}

    /// <summary>캐릭터의 속성 이펙트 출력</summary>
    /// <param name = "_charTid"> 캐릭터 Tid </param>
    private void DrawAttributeEffect(uint _charTid)
    {
        E_UnitAttributeType type = DBCharacter.GetAttributeType(_charTid);

        if(type != 0)
            AttributeBtn[(int)type - 1].SelectToggle();

        for (int i = 0; i < AttributeEffect.Length; i++)
            AttributeEffect[i].SetActive(i == Convert.ToInt32(type -1));
    }

    /// <summary>닉네임 생성창 오픈</summary>
    public void OpenCreateNickname()
    {
        ActiveNickNamePopup(true);
    }

    /// <summary>캐릭터 관련 서버 에러 수신 후 팝업 표시</summary>
    private void OnError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    {       
        if(_errorType != ZWebCommunicator.E_ErrorType.Receive)
		{
            ZLog.LogError(ZLogChannel.WebSocket, $"[{_errorType}]");
            return;
		}
        
        string btnName = ZUIString.LOCALE_OK_BUTTON;
        string errorMsg = DBLocale.GetText(_recvPacket.ErrCode.ToString());
        switch (_recvPacket.ErrCode)
        {
            case ERROR.DUPLICATE_NICKNAME:                
                UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                    _popup.Open(ZUIString.ERROR, errorMsg, new string[] { btnName }, new Action[] { delegate { 
                        _popup.Close(); 
                        ActiveNickNamePopup(true); 
                        InputNickName.text = string.Empty; 
                        CreateButton.interactable = true; } });});
                break;

            case ERROR.LOW_NICK:                
                UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                    _popup.Open(ZUIString.ERROR, errorMsg, new string[] { btnName }, new Action[] { delegate { 
                        _popup.Close(); 
                        ActiveNickNamePopup(true); 
                        InputNickName.text = string.Empty;
                        CreateButton.interactable = true; } });});
                break;

            case ERROR.CHARACTER_DELETE_LEVEL:                
                UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                    _popup.Open(ZUIString.ERROR, errorMsg, new string[] { btnName }, new Action[] { delegate { 
                        _popup.Close(); } });});
                break;

            case ERROR.MAX_CHARACTER:                
                UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
                    _popup.Open(ZUIString.ERROR, errorMsg, new string[] { btnName }, new Action[] { delegate { 
                        _popup.Close(); } });});
                break;

            default:
				ZWebManager.Instance.ProcessErrorPacket(_errorType, _reqPacket, _recvPacket, false);
                break;
        }
    }

    /// <summary> 캐릭터 선택 슬롯 리스트 초기화 </summary>
    private void ClearSelectSlotList()
    {
        for (int i = 0; i < CharSelectList.Count; i++)
            Destroy(CharSelectList[i].gameObject);

        CharSelectList.Clear();
    }

    /// <summary> 캐릭터 생성 슬롯 리스트 초기화</summary>
    private void ClearCreateSlotList()
    {
        for (int i = 0; i < CharCreateList.Count; i++)
            Destroy(CharCreateList[i].gameObject);

        CharCreateList.Clear();
    }

    /// <summary>캐릭터 Tid 검색 후 반환</summary>
    private uint GetCharTid(string _charTid)
    {
        uint tid = 0;

        List<Character_Table> table = DBCharacter.UsableCharTableList;

        for (int i = 0; i < table.Count; i++)
        {
            if (table[i].CharacterTextID == _charTid && table[i].AttributeType == SelectAttribute)
            {
                tid = table[i].CharacterID;
                return tid;
            }
        }

        return tid;
    }

    /// <summary> 캐릭터 초기 스테이터스 수치 출력</summary>
    public void SetCharStatus(uint _charTid)
    {
        if (ZUIConstant.CHARACTER_STATUS_COUNT == StatusPower.Length)
        {
            List<Character_Table> table = DBCharacter.UsableCharTableList;

            for(int i = 0; i < table.Count; i++)
            {
                if(table[i].CharacterID == _charTid && table[i].AttributeType == SelectAttribute)
                {
                    StatusPower[(int)E_Stat.STR].text = table[i].Strength.ToString();
                    StatusPower[(int)E_Stat.DEX].text = table[i].Dexterity.ToString();
                    StatusPower[(int)E_Stat.INT].text = table[i].Intellect.ToString();
                    StatusPower[(int)E_Stat.WIS].text = table[i].Wisdom.ToString();
                    StatusPower[(int)E_Stat.VIT].text = table[i].Vitality.ToString();
                }
            }
        }
    }

    /// <summary> 캐릭터 속성 선택</summary>
    public void SelectCharAttribute(int _attribute)
    {
        SelectAttribute = (E_UnitAttributeType)_attribute;

        if (SelectAttribute != 0)
            AttributeBtn[(int)SelectAttribute - 1].SelectToggle();

        for (int i = 0; i < AttributeEffectCreate.Length; i++)
            AttributeEffectCreate[i].SetActive(i == _attribute-1);

        uint tid = UICommon.GetCharTid(DBCharacter.Get(CharCreateList[CreateSlot - 1].CharTid).CharacterTextID, SelectAttribute);
		SetCreateCharacter(tid);
        SetCharStatus(tid);
    }

    /// <summary>캐릭터 생성창 클래스 마크 변경</summary>
    public void ChangeClassIcon(Sprite _icon)
    {
        CharClassIcon.sprite = _icon;
    }

    /// <summary>속성 정보창 활성 / 비활성화</summary>
    /// <param name = "_on"> 속성 정보창 활성화 / 비활성화 </param>
    public void SwitchAttributeInfo(bool _on)
    {
        AttributePopup.SetActive(_on);
        ClassInfoPopup.SetActive(!_on);
        Info.SetActive(!_on);
    }

    /// <summary>캐릭터 회전</summary>
    public void RotateCharacter(UnityEngine.EventSystems.BaseEventData _eventData)
    {
        EntityComponentAnimation_Animator anim = LobbyPc.gameObject.transform.GetComponentInChildren<EntityComponentAnimation_Animator>();
        
        if (anim.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            UICommon.RotateObjectDrag(_eventData, LobbyPc.gameObject);

		if (null != PCDirector && PCDirector.time > PCDirector.duration)
		{
			UICommon.RotateObjectDrag(_eventData, PCDirectorGO.gameObject);
		}
    }

    /// <summary>캐릭터 리스트 창 아이콘 갱신</summary>
    /// <param name = "_state"> 선택창 / 생성창 여부 </param>
    public void RefreshSelectSlot(E_CharacterSelectState _state)
    {
        switch(_state)
        {
            case E_CharacterSelectState.Select:
                for (int i = 0; i < CharSelectList.Count; i++)
                    CharSelectList[i].SetIcon(false);
                break;

            case E_CharacterSelectState.Create:
                for (int i = 0; i < CharCreateList.Count; i++)
                    CharCreateList[i].SetIcon(false);
                break;
        }
    }

#if UNITY_EDITOR
	/// <summary> 서버 개발자 요청 기능</summary>
	private void OnGUI()
    {
        //TODO :: 테스트용 MMO 채널 선택 입장.
        if (CharSelectList.Count == 0)
            return;

        if (0 < CharSelectList[SelectSlot].StageTid && 0 < CharSelectList[SelectSlot].CharId)
        {
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
			using (var h1 = new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				using (var v1 = new GUILayout.VerticalScope("box"))
				{
					GUILayout.Label($"MMO Channel List");
					if (GUILayout.Button($"Get MMO Channel", GUILayout.Height(30f)))
					{
						m_dicMMOServer.Clear();
						ZWebManager.Instance.WebGame.REQ_MMOChannelList(CharSelectList[SelectSlot].StageTid, (res) =>
						{
							for (int i = 0; i < res.ServerListLength; ++i)
							{
								var server = res.ServerList(i).Value;
								m_dicMMOServer.Add(server.ChannelId, server.ActualServerAddr);
							}
						});
					}

					if (0 < m_dicMMOServer.Count)
					{
						foreach (var server in m_dicMMOServer)
						{
							if (GUILayout.Button($"{server.Key} {server.Value}", GUILayout.Height(30f)))
							{
								ZNet.Data.Me.CharID = CharSelectList[SelectSlot].CharId;
								PlayerPrefs.SetInt("SelectMMOChannelId", (int)server.Key);
                                ZGameOption.Instance.LoadCharacterOption();
								ZGameManager.Instance.FSM.ChangeState(E_GameState.InGame);
							}
						}
					}
				}

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndArea();
		}
    }
#endif
}