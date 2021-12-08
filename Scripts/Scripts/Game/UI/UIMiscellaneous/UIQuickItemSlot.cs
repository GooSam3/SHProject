using DG.Tweening;
using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using uTools;
using ZDefine;
using ZNet.Data;

public class UIQuickItemSlot : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    public enum QuickEffectType
    {
        Select,
        Up,
        Auto
    }

    #region SerializeField Variable
    /// <summary>슬롯 아이콘</summary>
    [SerializeField] private Image SlotIcon = null;
    /// <summary>슬롯 아이템 등급 보드</summary>
    //[SerializeField] private Image SlotGradeBoard = null;
    /// <summary>슬롯 아이템 강화 카운트</summary>
    [SerializeField] private Text SlotGradeNum = null;
    /// <summary>슬롯 아이템 카운트</summary>
    [SerializeField] private Text SlotItemCount = null;
    /// <summary>슬롯 아이템 잼 카운트</summary>
    [SerializeField] private Text SlotGemCount = null;
    /// <summary>슬롯 아이템 잼</summary>
    [SerializeField] private GameObject SlotGem = null;
    /// <summary>슬롯 아이템 장금</summary>
    [SerializeField] private GameObject SlotLock = null;
    /// <summary>슬롯 직업 블럭</summary>
    [SerializeField] private GameObject SlotBlockRed = null;
    /// <summary>슬롯 아이템 장착</summary>
    [SerializeField] private GameObject SlotAlarmEquied = null;
    /// <summary>슬롯 오토 UI</summary>
    [SerializeField] private GameObject SlotAutoActive = null;
    /// <summary>슬롯 임팩트</summary>
    [SerializeField] private List<GameObject> SlotEffects = new List<GameObject>();
    /// <summary>슬롯 게이지 임팩트</summary>
    [SerializeField] private Image SlotGaugeEffect = null;

    [SerializeField] private Image SlotGaugeBg = null;
    /// <summary>슬롯 쿨타임</summary>
    [SerializeField] private uTweenSlider SlotCoolTime = null;
    /// <summary>오토 영역</summary>
    [SerializeField] public List<GameObject> AutoGroup = new List<GameObject>();
    /// <summary>오토 Effect Cliping 용</summary>
    [SerializeField] public List<UIShaderClipingUpdater> ListClipingEffect = new List<UIShaderClipingUpdater>();
    #endregion

    #region System Variable
    public QuickSlotScrollData Data;
    private int DataIdx;
    Vector3 TouchPosition;

    public ulong GlobalCoolTimeUTS = 0;
    #endregion

    private bool check = true;

    public void Initialize(QuickSlotScrollData _data, int _idx)
	{
        Data = _data;
        DataIdx = _idx;

        if (!UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
            return;

        //Cliping Effect
        //다른 방식 생각해볼것..
        if (check)
        {
            RectTransform viewPort = _quick.ScrollAdapter[Data.ContinerIdx].Parameters.Viewport;
            for (int i = 0; i < ListClipingEffect.Count; i++)
            {
                ListClipingEffect[i].SetClipingTransfrom(viewPort);
                ListClipingEffect[i].DoUIWidgetInitialize(_quick);
            }

            check = false;
        }

        if (Data.QuickSlotInfos[DataIdx] == null)
        {
            ReSetSlotUI();

            SlotAutoActive.SetActive(false);
            for (int i = 0; i < SlotEffects.Count; i++)
                SlotEffects[i].SetActive(false);
            return;
        }

        SetSlotInfo();

    }

    private void ReSetSlotUI()
    {
        SlotIcon.gameObject.SetActive(false);
        //SlotGradeBoard.gameObject.SetActive(false);

        SlotGradeNum.text = string.Empty;
        SlotItemCount.text = string.Empty;
        SlotGemCount.text = string.Empty;

        SlotGem.SetActive(false);
        SlotLock.SetActive(false);
        SlotBlockRed.SetActive(false);
        SlotAlarmEquied.SetActive(false);
        SlotCoolTime.gameObject.SetActive(false);
    }

    private void SetSlotInfo()
    {
        switch (Data.QuickSlotInfos[DataIdx].SlotType)
        {
            case QuickSlotType.TYPE_ITEM:
                {
                    var table = DBItem.GetItem(Data.QuickSlotInfos[DataIdx].TableID);
                    SlotIcon.sprite = UICommon.GetItemIconSprite(Data.QuickSlotInfos[DataIdx].TableID);
                    //SlotGradeBoard.sprite = UICommon.GetItemGradeSprite(Data.QuickSlotInfos[DataIdx].TableID);
                    SlotGradeNum.text = table.Step == 0 ? string.Empty : table.Step.ToString();
                    Data.QuickSlotInfos[DataIdx].Count = Data.QuickSlotInfos[DataIdx].GetItemCount(Data.QuickSlotInfos[DataIdx].SlotType, Data.QuickSlotInfos[DataIdx].TableID);
                    SlotItemCount.text = table.ItemID == 4800 || table.ItemStackType == E_ItemStackType.Not ? string.Empty : Data.QuickSlotInfos[DataIdx].Count.ToString();

                    ZItem item = Me.CurCharData.GetItemData(Data.QuickSlotInfos[DataIdx].UniqueID, NetItemType.TYPE_EQUIP);
                    SlotIcon.gameObject.SetActive(SlotIcon.sprite);
                    //SlotGradeBoard.gameObject.SetActive(SlotGradeBoard.sprite);

                    if (item != null && UICommon.GetEquipSocketCheck(item.Sockets))
                        SlotGemCount.text = UICommon.GetEquipSocketCount(item.Sockets).ToString();

                    SlotGem.SetActive(item != null && UICommon.GetEquipSocketCheck(item.Sockets));
                    SlotLock.SetActive(item != null && item.IsLock);
                    SlotAlarmEquied.SetActive(item != null && item.slot_idx != 0);

                    //아이템 쿨타임 갱신
                    //if (Me.CurCharData.GetItemData(Data.QuickSlotInfos[DataIdx].UniqueID, NetItemType.TYPE_STACK) != null)
                    //{
                    //    ulong useTime = Me.CurCharData.GetItemData(Data.QuickSlotInfos[DataIdx].UniqueID, NetItemType.TYPE_STACK).UseTime;
                    //    if (useTime != 0)
                    //    {
                    //        float remainCooltime = (DBItem.GetItem(Data.QuickSlotInfos[DataIdx].TableID).CoolTime) - ((TimeManager.NowMs - useTime) / 1000);
                    //        if (remainCooltime > 0)
                    //            EndCoolTime(DBItem.GetItem(Data.QuickSlotInfos[DataIdx].TableID).CoolTime, remainCooltime);
                    //    }
                    //}
				}
                break;

            case QuickSlotType.TYPE_SKILL:
                {
                    SlotIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBSkill.Get(Data.QuickSlotInfos[DataIdx].TableID).IconID);
                    SlotItemCount.text = string.Empty;

                    SlotIcon.gameObject.SetActive(Data.QuickSlotInfos[DataIdx].SlotType == QuickSlotType.TYPE_SKILL);
                    //SlotGradeBoard.gameObject.SetActive(Data.QuickSlotInfos[DataIdx].SlotType != QuickSlotType.TYPE_SKILL);
                    SlotGem.SetActive(Data.QuickSlotInfos[DataIdx].SlotType != QuickSlotType.TYPE_SKILL);
                    SlotLock.SetActive(Data.QuickSlotInfos[DataIdx].SlotType != QuickSlotType.TYPE_SKILL);
                    SlotAlarmEquied.SetActive(Data.QuickSlotInfos[DataIdx].SlotType != QuickSlotType.TYPE_SKILL);
                }
                break;
        }

        // 오토 설정은 공통
        Data.QuickSlotInfos[DataIdx].bAuto = Me.CurCharData.GetQuickSlotInfo(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx).bAuto;
        SlotAutoActive.SetActive(Data.QuickSlotInfos[DataIdx].bAuto);
        SlotEffects[(int)QuickEffectType.Auto].SetActive(Data.QuickSlotInfos[DataIdx].bAuto);
    }

    /// <summary>slot 클릭 액션 콜백</summary>
    public void Click()
    {
        object selectObject = null;

        if(UIManager.Instance.Find(out UIFrameSkill _skill) && _skill.gameObject.activeSelf)
            selectObject = _skill.GetSelectObject();
        else if (UIManager.Instance.Find(out UIFrameInventory _inventory) && _inventory.gameObject.activeSelf)
            selectObject = _inventory.GetSelectObject();

        if (selectObject != null)
        {
            bool bIsAuto = false;
            QuickSlotInfo info = Me.CurCharData.GetQuickSlotInfo(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx);

            if (info != null)
            {
               RemoveQuickSlotObject(info);
            }

            // selectObject가 스킬인 경우
            if (selectObject is uint SkillTid)
			{
				if (DBSkill.Get(SkillTid).QuickSlotType == E_QuickSlotType.NotQuickSlot)
				{
					UICommon.SetNoticeMessage("퀵슬롯에 등록할 수 없는 스킬입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);
					return;
				}

                var list = GetSameQuickSlotInfoList(SkillTid, QuickSlotType.TYPE_SKILL);

                if (list != null && list.Count > 0)
                {
                    bIsAuto = list[0].bAuto;
                }

                // 퀵슬롯 데이터 갱신
                Me.CurCharData.UpdateQuickSlotItem(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx, QuickSlotType.TYPE_SKILL, 0, SkillTid, bIsAuto);

				ZWebManager.Instance.WebGame.REQ_SetCharacterOption((Data.ContinerIdx == 0 ?
					WebNet.E_CharacterOptionKey.QuickSlot_Set1 :
					WebNet.E_CharacterOptionKey.QuickSlot_Set2),
					Me.CurCharData.GetQuickSlotValue(Data.ContinerIdx),
					(recvPacket, recvMsgPacket) =>
					{
						UIManager.Instance.Find<UISubHUDQuickSlot>().ReSetAllSlot();
						ZPawnMyPc myEntity = ZPawnManager.Instance.MyEntity;
						if (myEntity != null)
						{
							SkillInfo skillInfo = myEntity.SkillSystem.GetSkills().Find(skill => skill.SkillId == SkillTid);
							if (skillInfo != null)
							{
								float remainTime = myEntity.IsAutoPlay ? skillInfo.RemainCoolTime : skillInfo.RemainCustomCoolTime;
								EndCoolTime(remainTime * TimeHelper.Unit_MsToSec);
							}
						}
						ActiveSlotUpEffect();
					});
			}
            // selectObject가 아이템인 경우.
            else if (selectObject is ZItem) 
			{
				ZItem selectItem = selectObject as ZItem;

				if (DBItem.GetItem(selectItem.item_tid).QuickSlotType == E_QuickSlotType.NotQuickSlot)
				{
					UICommon.SetNoticeMessage("퀵슬롯에 등록할 수 없는 아이템입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.SubNotice);
					return;
				}

                var list = GetSameQuickSlotInfoList(selectItem.item_tid, QuickSlotType.TYPE_ITEM);

                if (list != null && list.Count > 0)
                {
                    bIsAuto = list[0].bAuto;
                }

                // 퀵슬롯 데이터 갱신
                Me.CurCharData.UpdateQuickSlotItem(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx, QuickSlotType.TYPE_ITEM, selectItem.item_id, selectItem.item_tid, bIsAuto);
				ZWebManager.Instance.WebGame.REQ_SetCharacterOption((Data.ContinerIdx == 0 ?
					WebNet.E_CharacterOptionKey.QuickSlot_Set1 :
					WebNet.E_CharacterOptionKey.QuickSlot_Set2),
					Me.CurCharData.GetQuickSlotValue(Data.ContinerIdx),
					(recvPacket, recvMsgPacket) =>
					{
						UIManager.Instance.Find<UISubHUDQuickSlot>().ReSetAllSlot();
						ActiveSlotUpEffect();
					});
			}
		}
        else
        {
            if (Data.QuickSlotInfos[DataIdx] == null /*|| (Data.QuickSlotInfos[DataIdx].bAuto && Data.QuickSlotInfos[DataIdx].SlotType == QuickSlotType.TYPE_ITEM)*/)
                return;
            
            if (UIManager.Instance.Find<UISubHUDQuickSlot>().IsQuickSlotResetMode)
            {
                QuickSlotReset();
                return;
            }

            switch (Data.QuickSlotInfos[DataIdx].SlotType)
            {
                case QuickSlotType.TYPE_ITEM:
                    {
                        if (GlobalCoolTimeUTS + (ulong)(1 * 1000) > TimeManager.NowMs)
                        {
                            // to do : 상황에 따라 너무 많이 호출되서 주석
                            //UICommon.SetNoticeMessage("글로벌 쿨타임입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                        }
                        else
                            UIManager.Instance.Find<UISubHUDQuickSlot>().UseItem(Data.QuickSlotInfos[DataIdx], ActiveSlotUpEffect);
                    }
                    break;
                case QuickSlotType.TYPE_SKILL:
                    {
                        var myEntity = ZPawnManager.Instance.MyEntity;
                        if (null == myEntity)
                            return;

                        var skillSystem = myEntity.SkillSystem;
                        if (!skillSystem.TryGetSkillInfo(Data.QuickSlotInfos[DataIdx].TableID, out var skillInfo))
                            return;

                        var targetPawn = myEntity.GetTarget();
                        if (null == targetPawn && (skillInfo.TargetType == E_TargetType.Enemmy || skillInfo.TargetType == E_TargetType.Summoner))
                        {
                            myEntity.ShowSkillErrorMessage(E_SkillSystemError.NotExistTarget);
                            return;
                        }

                        var error = myEntity.UseSkillBySkillId(Data.QuickSlotInfos[DataIdx].TableID);

                        ActiveSlotUpEffect();
                    }
                    break;
            }
        }
    }

    /// <summary>쿨타임 오브젝트 해제 및 다시 쿨타임 갱신용 함수</summary>
    /// <param name="_coolTime">쿨타임</param>
    /// <param name="_remainTime">쿨타임 이 다돌지 못하고 리프레쉬 될경우 남은 쿨타임</param>
    public void EndCoolTime(float _coolTime, float _remainTime = 0)
    {
        SlotCoolTime.gameObject.SetActive(false);

        UpdateCoolTime(_coolTime, _remainTime);
    }


    /// <summary>쿨타임 활성화 함수</summary>
    /// <param name="_coolTime">쿨타임</param>
    /// <param name="_remainTime">쿨타임 이 다돌지 못하고 리프레쉬 될경우 남은 쿨타임</param>
    public void UpdateCoolTime(float _coolTime, float _remainTime = 0)
    {
        if (_coolTime > 0)
        {
            SlotCoolTime.gameObject.SetActive(true);
            SlotCoolTime.enabled = true;

            if (_remainTime != 0)
            {
                SlotCoolTime.duration = _remainTime;
                SlotCoolTime.from = _remainTime / _coolTime;
            }
            else
                SlotCoolTime.duration = _coolTime;

            SlotCoolTime.ResetToBeginning();
            SlotCoolTime.Play(true);
        }
    }

    /// <summary>퀵 슬롯 클릭시 임팩트 활성화 함수</summary>
    private void ActiveSlotUpEffect()
    {
        SlotEffects[(int)QuickEffectType.Up].SetActive(true);

        Invoke(nameof(DisabledSlotUpEffect), 0.5f);
    }

    private void DisabledSlotUpEffect()
    {
        SlotEffects[(int)QuickEffectType.Up].SetActive(false);
    }

    /// <summary>퀵슬롯 선택이팩트 활성화</summary>
    /// <param name="_active">활성화 여부</param>
    public void ActiveSelectEffect(bool _active)
    {
        SlotEffects[(int)QuickEffectType.Select].SetActive(_active);
    }

    private void QuickSlotReset()
    {
        if (Data.QuickSlotInfos[DataIdx] == null)
            return;

        Me.CurCharData.RemoveQuickSlotData(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx);

        WebNet.E_CharacterOptionKey key = Data.ContinerIdx == 0 ? WebNet.E_CharacterOptionKey.QuickSlot_Set1 : WebNet.E_CharacterOptionKey.QuickSlot_Set2;
        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(key, Me.CurCharData.GetQuickSlotValue(Data.ContinerIdx), (recvPacket, recvMsgPacket) =>
        {
            UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[Data.ContinerIdx].SetScrollData(Data.ContinerIdx);
        });
    }

    private void QuickSlotAllReset()
	{
		Me.CurCharData.RemoveAllQuickSlotData();

        // 1번 슬롯 갱신
        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set1, Me.CurCharData.GetQuickSlotValue(0), (recvPacket, recvMsgPacket) =>
        {
            UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[0].SetScrollData(0);
        });
        // 2번 슬롯 갱신
        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set2, Me.CurCharData.GetQuickSlotValue(1), (recvPacket, recvMsgPacket) =>
        {
            UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[1].SetScrollData(1);
        });
    }

    /// <summary>퀵슬롯 오토 게이지 활성화 이벤트 콜백</summary>
	public void OnPointerDown(PointerEventData eventData)
    {
        //오토 게이지 활성화 여부.
        if (Data.QuickSlotInfos[DataIdx] == null)
            return;

        var data = Data.QuickSlotInfos[DataIdx];

        if(data.SlotType == QuickSlotType.TYPE_ITEM)
		{
            var itemData = DBItem.GetItem(Data.QuickSlotInfos[DataIdx].TableID);

            if (null == itemData || itemData.QuickSlotAutoType == E_QuickSlotAutoType.Not)
                return;
        }   
        else
		{
            var skillData = DBSkill.Get(Data.QuickSlotInfos[DataIdx].TableID);

            if (null == skillData || skillData.QuickSlotAutoType == E_QuickSlotAutoType.Not)
                return;
        }        

        TouchPosition = eventData.position;
        SwitchAutoSetUI(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SwitchAutoSetUI(false);
        TouchPosition = Vector3.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SwitchAutoSetUI(false);

        if(Vector3.Distance(TouchPosition, eventData.position) == 0)
            OnCloseAutoUI();
    }

    /// <summary>퀵슬롯 오토 UI 활성화 함수</summary>
    private void SwitchAutoSetUI(bool _autoSet)
    {
        SlotGaugeEffect.DOKill();
        SlotGaugeBg.gameObject.SetActive(_autoSet);
        if (_autoSet)
		{
            SlotGaugeEffect.DOFillAmount(1f, 0.8f).SetEase(Ease.InCubic);
            
            Invoke(nameof(OpenAutoUI), 0.8f);
        }
        else
		{
            SlotGaugeEffect.fillAmount = 0;

            CancelInvoke(nameof(OpenAutoUI));
        }
    }

    private void OpenAutoUI()
    {
        AutoGroup[0].SetActive(true);
        AutoGroup[1].SetActive(!Data.QuickSlotInfos[DataIdx].bAuto);
        AutoGroup[2].SetActive(Data.QuickSlotInfos[DataIdx].bAuto);

        TimeInvoker.Instance.RequestInvoke(delegate {
            if(AutoGroup[0].gameObject.activeSelf)
                OnCloseAutoUI();
        }, 0.5f);
        
        if (UIManager.Instance.Find(out UIFrameOption optionui) && optionui.Show && UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
            _quick.ScrollAdapter[Data.ContinerIdx].FocusBG.SetActive(true);
    }

    public void OnCloseAutoUI()
    {
        for (int i = 0; i < AutoGroup.Count; i++)
            AutoGroup[i].SetActive(false);

        if(UIManager.Instance.Find(out UIFrameOption optionui) && optionui.Show && UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
            _quick.ScrollAdapter[Data.ContinerIdx].FocusBG.SetActive(false);

        SwitchAutoSetUI(false);
    }

    /// <summary>퀵슬롯 AutoOnOff 이벤트 함수</summary>
    public void AutoOnOffEvent(bool _bAuto)
    {
        QuickSlotInfo info = Me.CurCharData.GetQuickSlotInfo(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx);
  
        foreach (var quickItem in Me.CurCharData.QuickSlotSet1Dic)
            if (quickItem.Value.TableID == info.TableID && quickItem.Value.SlotType == info.SlotType)
                quickItem.Value.bAuto = _bAuto;

        foreach (var quickItem in Me.CurCharData.QuickSlotSet2Dic)
            if (quickItem.Value.TableID == info.TableID && quickItem.Value.SlotType == info.SlotType)
                quickItem.Value.bAuto = _bAuto;

        if (_bAuto)
            ZPawnManager.Instance.MyEntity.GetAutoSkillController().AddAutoSkill(Data.QuickSlotInfos[DataIdx].TableID);
        else
            ZPawnManager.Instance.MyEntity.GetAutoSkillController().RemoveAutoSkill(Data.QuickSlotInfos[DataIdx].TableID);

        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set1, Me.CurCharData.GetQuickSlotValue(0),
            (recvPacket1, recvMsgPacket1) => {
                ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.QuickSlot_Set2, Me.CurCharData.GetQuickSlotValue(1),
                   (recvPacket2, recvMsgPacket2) => {
                       if(UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
                           _quick.RefreshAllSlot();

                       OnCloseAutoUI();
                   });
            });
    }

    private List<QuickSlotInfo> GetSameQuickSlotInfoList()
    {
        QuickSlotInfo info = Me.CurCharData.GetQuickSlotInfo(Data.ContinerIdx, Data.HoldIdx * 4 + DataIdx);

        if (info == null)
        {
            return null;
        }

        var quickList = new List<QuickSlotInfo>();

        foreach(var quickItem in Me.CurCharData.QuickSlotSet1Dic)
        {
            if(quickItem.Value.TableID == info.TableID && quickItem.Value.SlotType == info.SlotType)
            {
                quickList.Add(quickItem.Value);
            }
        }

        foreach(var quickItem in Me.CurCharData.QuickSlotSet2Dic)
        {
            if (quickItem.Value.TableID == info.TableID && quickItem.Value.SlotType == info.SlotType)
            {
                quickList.Add(quickItem.Value);
            }
        }

        return quickList;
    }

    private List<QuickSlotInfo> GetSameQuickSlotInfoList(uint tableTid, QuickSlotType type)
    {
        var quickList = new List<QuickSlotInfo>();

        foreach (var quickItem in Me.CurCharData.QuickSlotSet1Dic)
        {
            if (quickItem.Value.TableID == tableTid && quickItem.Value.SlotType == type)
            {
                quickList.Add(quickItem.Value);
            }
        }

        foreach (var quickItem in Me.CurCharData.QuickSlotSet2Dic)
        {
            if (quickItem.Value.TableID == tableTid && quickItem.Value.SlotType == type)
            {
                quickList.Add(quickItem.Value);
            }
        }

        return quickList;
    }

    private void RemoveQuickSlotObject(QuickSlotInfo info)
    {
        var quickList = GetSameQuickSlotInfoList();

        if(quickList == null)
        {
            return;
        }

        if(info.SlotType == QuickSlotType.TYPE_SKILL)
        {
            if(quickList.Count == 1)
            {
                ZPawnManager.Instance.MyEntity.GetAutoSkillController().RemoveAutoSkill(info.TableID);
            }
        }
        else
        {
            if(quickList.Count == 1)
            {
                info.bAuto = false;
            }
        }

        quickList.Clear();
    }
}