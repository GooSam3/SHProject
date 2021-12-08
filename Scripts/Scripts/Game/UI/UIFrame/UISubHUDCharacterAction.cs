using GameDB;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISubHUDCharacterAction : ZUIFrameBase
{
	[SerializeField] private Image PotionIcon;
	[SerializeField] private Text PotionCnt;
	[SerializeField] private GameObject AttackEff;
	[SerializeField] private GameObject AutoEff;
	[SerializeField] private GameObject TargetEff;
    [SerializeField] private GameObject TargetUnlockEff;
    [SerializeField] private GameObject SearchTargetEff;
    [SerializeField] private GameObject ObjPartyTarget;
    [SerializeField] private GameObject ObjPartyTargetEff;
    [SerializeField] private GameObject ObjTargetUnLock;
    [SerializeField] private GameObject GatherGauge;
    [SerializeField] private GameObject ScanTargetButton;
    [SerializeField] private ZSlider GatherSlider;
    [SerializeField] private Text GatherRemainTime;
    [SerializeField] private Text GatherObjectName;

    private Coroutine TargetEffRoutine;

    protected override void OnUnityStart()
    {
        base.OnUnityStart();

        ZGameOption.Instance.OnOptionChanged += UpdatePotionInfo;
    }

    protected override void OnUnityDestroy()
    {
        base.OnUnityDestroy();

        ZGameOption.Instance.OnOptionChanged -= UpdatePotionInfo;
        
        if(ZPartyManager.hasInstance)
        {
            ZPartyManager.Instance.DoRemoveEventUpdateParty(HandleUpdateParty);
            ZPartyManager.Instance.DoRemoveEventChangePartyTargetEntityId(HandleChangePartyTargetEntityId);
        }

        if(ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventCreateMyEntity(AddUpdateEvent);
            ZPawnManager.Instance.MyEntity?.DoRemoveEventChangeTarget(HandleUpdateTarget);
            ZPawnManager.Instance.MyEntity?.DoRemoveEventGatherInterFace(ShowGatherInterface);
        }
    }

    protected override void OnUnityEnable()
	{
        SetScanTargetButtonByGameMode();
        InvokeRepeating(nameof(UpdateEffects), 0.5f, 0.5f);
		InvokeRepeating(nameof(UpdateSearchTargetEffect), 0.5f, 0.5f);

        
	}

	protected override void OnUnityDisable()
	{
		CancelInvoke(nameof(UpdateEffects));
		CancelInvoke(nameof(UpdateSearchTargetEffect));
	}

	protected override void OnRemove()
	{
		base.OnRemove();
    }

	public void Init()
    {
        ObjPartyTarget.SetActive(false);
        ObjPartyTargetEff.SetActive(false);
        ObjTargetUnLock.SetActive(false);

        AttackEff.SetActive(false);
        AutoEff.SetActive(false);
        TargetEff.SetActive(false);
        SearchTargetEff.SetActive(false);

        SetPotionInfo();
        SetDelegate();
    }

    /// <summary> Vehicle 관련 이벤트 등록(쿨타임) </summary>
    private void SetDelegate()
    {
        //추후 캐릭터 재생성시 등록되도록
        ZPawnManager.Instance.DoAddEventCreateMyEntity(AddUpdateEvent);
        ZPartyManager.Instance.DoAddEventUpdateParty(HandleUpdateParty);
        ZPartyManager.Instance.DoAddEventChangePartyTargetEntityId(HandleChangePartyTargetEntityId);
    }

    /// <summary> 캐릭터 생성시 필요한 이벤트 등록 </summary>
    private void AddUpdateEvent()
    {
        if (null == ZPawnManager.Instance.MyEntity)
        {
            return;
        }

        ZPawnManager.Instance.MyEntity.DoAddEventChangeTarget(HandleUpdateTarget);
        ZPawnManager.Instance.MyEntity.DoAddEventGatherInterface(ShowGatherInterface);
        UpdateTargetUnLockUI();
    }

    private void HandleUpdateTarget(uint preTargetEntityId, uint targetEntityId)
    {
        UpdateTargetUnLockUI();        
    }

    private void UpdateTargetUnLockUI()
    {
        ObjTargetUnLock.SetActive(null != ZPawnManager.Instance.MyEntity.GetTarget());
    }

    /// <summary> 파티 갱신시 </summary>
    private void HandleUpdateParty()
    {
        ObjPartyTarget.SetActive(ZPartyManager.Instance.IsParty);        
        ObjPartyTargetEff.SetActive(0 < ZPartyManager.Instance.PartyTargetEntityId);
    }

    /// <summary> 파티 타겟 갱신시 </summary>
    private void HandleChangePartyTargetEntityId(uint entityId)
    {
        ObjPartyTargetEff.SetActive(0 < entityId);
    }

    private void UpdatePotionInfo(ZGameOption.OptionKey key)
    {
        if(key == ZGameOption.OptionKey.Option_HP_Potion_Priority)
        {
            SetPotionInfo();
        }
    }

    private void SetScanTargetButtonByGameMode()
    {
        ScanTargetButton.SetActive(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Field);
    }

	public void SetPotionInfo()
    {
        uint potionId = ZGameOption.Instance.HP_PotionUsePriority == ZGameOption.HPPotionUsePriority.NORMAL ? DBConfig.HPPotion_Normal_ItemID : DBConfig.HPPotion_High_ID;

        var potion = ZNet.Data.Me.CurCharData.InvenList.Find(item => item.item_tid == potionId && item.netType == NetItemType.TYPE_STACK);

        PotionIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItem(potionId).IconID);

        if (potion != null)
		{
			PotionCnt.text = potion.cnt.ToString();
		}
        else
        {
            PotionCnt.text = "0";
        }

        //[박윤성] 포션 소모됏을때 옵션값에 따른 알람처리
        if(potion != null && ZGameOption.Instance.AlramHPPotion >= potion.cnt)
        {
            string noticeStr = string.Format(DBLocale.GetText("MessageC_HPPotion_NoHave"), DBLocale.GetText(DBItem.GetItemName(potionId)));
            UICommon.SetNoticeMessage(noticeStr, Color.red, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
        }

		PotionIcon.gameObject.SetActive(true);
	}
	public void SelectAttack()
    {
		if (!(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity))
			return;

        myEntity.StartDefaultAI();

        // TODO : 캐릭터 시스템 작업자 확인후, 수정바람

        if (myEntity.IsAttacking)
		{
            //myEntity.SetTarget(null);
            myEntity.StopMove();
			myEntity.ChangeState(E_EntityState.Empty);
		}
		else
		{
			myEntity.UseNormalAttack();
		}
	}

    public void SelectTarget()
    {
        if (null == ZPawnManager.Instance.MyEntity)
            return;

        var target = ZPawnTargetHelper.SearchTargetByTargetPriority(ZPawnManager.Instance.MyEntity, ZPawnManager.Instance.MyEntity.Position, DBConfig.SearchTargetRange);

        //TargetEff.gameObject.SetActive(target != null);
        if (null == target)
        {
            return;
        }

        ZPawnManager.Instance.MyEntity.SetTarget(target);
	}

    /// <summary> 파티 타겟 버튼 클릭 </summary>
    public void OnClickPartyTargetTarget()
    {
        if (null == ZPawnManager.Instance.MyEntity)
            return;

        ZPawnManager.Instance.MyEntity.SetTargetByPartyTarget();
    }

    public void UnLockTarget()
    {
		if (!(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity))
			return;

        if(!ZGameOption.Instance.bUseTargetCancel)
        {
            return;
        }

		myEntity.SetTarget(null);
		// TODO : 공격중이던거 자동으로 안멈춰서 여기서 멈추게함.
		myEntity.ChangeState(E_EntityState.Empty);
	}

    public void UsePotion()
    {
        UIManager.Instance.Open<UIFrameOption>();

        //var potionData = ZNet.Data.Me.CurCharData.InvenList.Find(item => DBItem.GetItemType(item.item_tid) == E_ItemType.HPPotion && item.netType == NetItemType.TYPE_STACK);

        //if(potionData != null)
        //    ZWebManager.Instance.WebGame.UseItemAction(potionData, false, null);
	}

	public void SelectSearch()
	{
		if (!(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity))
			return;

		myEntity.SetTargetList();
	}

	public void SelectAuto()
    {
		if (!(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity))
			return;

        myEntity.StartDefaultAI();

        myEntity.IsAutoPlay = !myEntity.IsAutoPlay;
    }

    IEnumerator E_OneShotActivate(GameObject targetGo, float showDuration)
	{
		targetGo?.SetActive(false);
		targetGo?.SetActive(true);

		yield return new WaitForSeconds(showDuration);

		targetGo?.SetActive(false);
	}

	/// <summary> 일정시간마다 계속 체크해서 이펙트를 갱신시켜준다. </summary>
	private void UpdateEffects()
	{
		if (ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity)
		{
			AttackEff.gameObject.SetActive(myEntity.IsAttacking && null != myEntity.GetTarget());
            AutoEff.gameObject.SetActive(myEntity.IsAutoPlay);
		}
	}

	private void UpdateSearchTargetEffect()
    {
		if(ZPawnManager.Instance.MyEntity is ZPawnMyPc myEntity)
        {
			bool bIsActive = myEntity.TargetSearchList != null && myEntity.TargetSearchList.Count > 0;

			SearchTargetEff.gameObject.SetActive(bIsActive);
        }
    }

    /// <summary> 임시 펫 탑승 버튼 </summary>
    public void OnClickRiding()
    {
        uint vehicleTid = ZNet.Data.Me.CurCharData.MainVehicle;
        ulong endCoolTime = ZNet.Data.Me.CurCharData.VehicleEndCoolTime;

        if (0 >= vehicleTid)
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("Not_Enter_Vehicle_Text"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        if (endCoolTime > TimeManager.NowSec)
        {
            UICommon.SetNoticeMessage($"아직 탑승할 수 없다.(남은시간 : {endCoolTime - TimeManager.NowSec} 초)", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        //탑승중이면 내리자
        if (true == ZPawnManager.Instance.MyEntity.IsRiding)
        {
            vehicleTid = 0;
        }
        else if(ZPawnManager.Instance.MyEntity.IsSkillAction)
        {
            //공격중이거나 탑승불가 상태면 패스
            UICommon.SetNoticeMessage($"스킬 사용중에 탑승할 수 없다.", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }
        else if(ZPawnManager.Instance.MyEntity.IsMezState(E_ConditionControl.NotRide))
        {
            //공격중이거나 탑승불가 상태면 패스
            UICommon.SetNoticeMessage($"탑승할 수 없는 상태다.", Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        ZMmoManager.Instance.Field.REQ_RideVehicle(ZPawnManager.Instance.MyEntityId, vehicleTid);
    }

    /// <summary> 채집 인터페이스 /// </summary>
    float gauge = 0f;
    float remainTime = 0f;

    private void ShowGatherInterface(float _duration, ZObject gatherObj)
    {
        if (_duration == 0)
        {
            CancelObjectGathering();
            return;
        }

        GatherGauge.SetActive(true);

        if(DBObject.TryGet(gatherObj.TableId, out var obj))
		{
            GatherObjectName.text = string.Format(DBLocale.GetText("GatheringCast_Text"), DBLocale.GetText(obj.ObjectTextID));

        }

        remainTime = _duration;

        gauge = GatherSlider.maxValue / _duration;

        GatherSlider.value = 0f;

        //StartCoroutine("ChangeGatherSlider");

        var tweener = uTools.uTweenSlider.Begin(GatherSlider, remainTime,0,0,1f);
        tweener.OnValueChange = (sliderValue)=>{
            GatherRemainTime.text = (remainTime - (remainTime* sliderValue)).ToString("F1");
        };

        Invoke("OffGatherInterface", _duration);
        Invoke("ObjectGatheringEnd", _duration + 0.5f);
    }

    private void CancelObjectGathering()
    {
        CancelInvoke("OffGatherInterface");
        CancelInvoke("ObjectGatheringEnd");
        //StopCoroutine("ChangeGatherSlider");

        var sldier = GatherSlider.GetComponent<uTools.uTweenSlider>();
        if (sldier != null)
            Destroy(sldier);

        OffGatherInterface();
        ObjectGatheringEnd();
    }

    private void OffGatherInterface()
    {
        GatherGauge.SetActive(false);
    }

    private void ObjectGatheringEnd()
    {
        ZPawnManager.Instance.MyEntity.ObjectGatheringEnd();
    }

    /*IEnumerator ChangeGatherSlider()
    {
        while (remainTime > 0)
        {
            GatherSlider.value += gauge * Time.deltaTime;
            remainTime -= Time.deltaTime;
            GatherRemainTime.text = remainTime.ToString("F1");

            yield return null;
        }
    }*/
}