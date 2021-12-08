using GameDB;
using System.Collections.Generic;
using UnityEngine;

abstract public class UINameTagBase : CUIWidgetNameTagBase
{
    [SerializeField] protected  bool            RayCastTarget = true;
    [SerializeField] private    float           ChatDuration = 3f;
    [SerializeField] protected  ZImage          Emoticon = null;
    [SerializeField] protected  ZText           Chatting = null;
    [SerializeField] private    GameObject       ChatRoot = null;
    [SerializeField] private    ZText           TagName = null;
    [SerializeField] private    List<GameObject> FocusList = new List<GameObject>();
    [SerializeField] private    GameObject      PlayerInfo = null;

    protected E_ModelSocket mFollowSocket;
    protected E_ModelSocket mRaycastSocket;

    private bool  mUpdateChattingEmoji = false;
    private float mUpdateCurrent = 0;

    protected ZPawn mFollowPawn = null;
    private uint mEntityID = 0;
  
    private uint mEffectTID = 0;
    private ZEffectComponent mSpawnEffect = null;

    protected override void OnUnityStart()
    {
        base.OnUnityStart();
    }

    protected override void OnUnityDestroy()
    {
        base.OnUnityDestroy();
    }
 
    //-------------------------------------------------------------------
    public void DoNameTagInitialize(ZPawn _followPawn, E_ModelSocket _followSocket, E_ModelSocket _raycastTarget)
	{
        if (TagName != null)
        {
            TagName.text = _followPawn.PawnData.Name;
        }

        mFollowPawn = _followPawn;
        mFollowSocket = _followSocket;
        mRaycastSocket = _raycastTarget;
        mUpdateChattingEmoji = false;
        mUpdateCurrent = 0;
        mEntityID = _followPawn.EntityId;
        EffectNameTagDetach();
        SetNameTagModel();
        SetNameTagColorNormal();
        DoUIWidgetFocus(false);

        OnNameTagInitialize(mFollowPawn);
    }

    public void DoNameTagRefreshTarget()
	{
        OnNameTagRefreshTarget();
	}

    public void DoNameTagPawnDie()
	{
        EffectNameTagDetach();
        SetNameTagShowHide(false);
        OnNameTagPawnDie();
    }

    //--------------------------------------------------------------------
    protected void SetNameTagColorPK()
    {
        TagName.color = ResourceSetManager.Instance.SettingRes.Palette.PkNameColor;
    }

    protected void SetNameTagColorEnemyPC()
	{
        TagName.color = Color.red;
    }

    protected void SetNameTagColorTendency(int _tendencyValue)
	{
        if (_tendencyValue > 0)
        {
            float tendencyRatio = _tendencyValue / (float)DBConfig.MaxGoodTendency;
            TagName.color = Color.Lerp(
                ResourceSetManager.Instance.SettingRes.Palette.NormalTendencyNameColor,
                ResourceSetManager.Instance.SettingRes.Palette.GoodTendencyNameColor,
                tendencyRatio);
        }
        else
        {
            float tendencyRatio = _tendencyValue / (float)DBConfig.MaxEvilTendency;
            TagName.color = Color.Lerp(
                ResourceSetManager.Instance.SettingRes.Palette.NormalTendencyNameColor,
                ResourceSetManager.Instance.SettingRes.Palette.EvilTendencyNameColor,
                tendencyRatio);
        }
    }

    protected void SetNameTagColorNormal()
	{
        TagName.color = Color.white;
	}

    protected void SetNameTagShowHide(bool _show)
	{
        TagName.transform.parent.gameObject.SetActive(_show);
        TagName.gameObject.SetActive(_show);
	}

    public void SetNameTagChattingMessage(string _message, Color _messageColor, float duration = 3f)
	{
        ChatRoot.gameObject.SetActive(true);
        var tween = ChatRoot.GetComponent<uTools.uTweenAlpha>();
        if (null != tween)
            tween.duration = duration;

        ChatDuration = duration;
        Chatting.text = _message;
        Chatting.color = _messageColor;
        StartUpdateChatEmoji();
    }

    public void HideNameTagChattingMessage()
    {
        EndUpdateChatEmoji();
    }

    protected void SetNameTagEmoji(string _emojiSprite)
	{
        Sprite emojiSprite = ZManagerUIPreset.Instance.GetSprite(_emojiSprite);
        if (emojiSprite != null)
		{
            Emoticon.gameObject.SetActive(true);
            Emoticon.sprite = emojiSprite;
            StartUpdateChatEmoji();
        }
    }

    protected void SetNameTag(string _name, Color _color)
	{
        TagName.text = _name;
        TagName.color = _color;
	}

    protected void SetNameTagNameOnOff(bool _on)
	{
        PlayerInfo.SetActive(_on);
	}

    protected void EffectNameTagAttach(uint _effectTID)
	{
        if (mFollowPawn.IsDead) return;

        EffectNameTagDetach();
        mEffectTID = _effectTID;
        mFollowPawn.DoAddEventLoadedModel(HandleNameTagAttachEffect);
    }

    protected void EffectNameTagDetach()
	{
        if (mSpawnEffect)
        {
            mSpawnEffect.Despawn(false);
            mSpawnEffect = null;
        }
    }
	//-------------------------------------------------------------------
	protected override void OnUIWidgetFocus(bool _on)
	{
		base.OnUIWidgetFocus(_on);
        for (int i = 0; i < FocusList.Count; i++)
        {
            if (FocusList[i] != null)
                FocusList[i].SetActive(_on);
        }
    }

	protected override void OnNameTagRemove()
	{
		base.OnNameTagRemove();
        if (mFollowPawn != null)
		{
            mFollowPawn.DoRemoveEventLoadedModel(HandleNameTagChangeModel);            
        }
        EffectNameTagDetach();
    }

    protected override void OnNameTagUpdate()
	{
       if (mUpdateChattingEmoji)
		{
            mUpdateCurrent += Time.deltaTime;
            if (mUpdateCurrent >= ChatDuration)
			{
                EndUpdateChatEmoji();
			}
		}

    }
    //-------------------------------------------------------------------
    private void StartUpdateChatEmoji()
	{
        mUpdateChattingEmoji = true;
        mUpdateCurrent = 0;
	}

    private void EndUpdateChatEmoji()
	{
        mUpdateChattingEmoji = false;
        Emoticon.gameObject.SetActive(false);
        ChatRoot.gameObject.SetActive(false);
    }

    private void SetNameTagModel()
	{
        // 호출시점에서 더미를 입력하고 로드가 완료되면 다시 갱신한다.
        mFollowPawn.DoAddEventLoadedModel(HandleNameTagChangeModel); // 로딩되거나 모델이 바뀌었을 경우 출력 트랜스폼을 변경 

        Transform rayCast = RayCastTarget ? mFollowPawn.GetSocket(mRaycastSocket) : null;
        ChangeFollowTarget(mFollowPawn.gameObject, mFollowPawn.GetSocket(mFollowSocket), rayCast);
    }

    //-------------------------------------------------------------------
    public void HandleNameTagInteraction()
	{
        ZPawnMyPc myPC = ZPawnManager.Instance.MyEntity;
        if (myPC)
		{
            Zero.IInteractable interactable = mFollowPawn as Zero.IInteractable;
            if (interactable != null)
			{
                myPC.StartAIForTalkNpc(ZGameModeManager.Instance.StageTid, mFollowPawn.Position, mFollowPawn.TableId);
			}          
        }
	}


    private void HandleNameTagAttachEffect()
	{
        if (mFollowPawn == null)
		{
            EffectNameTagDetach();
            return;
		}

        mFollowPawn.DoRemoveEventLoadedModel(HandleNameTagAttachEffect);

        ZEffectManager.Instance.SpawnEffect(mEffectTID, mFollowPawn.GetSocket(mFollowSocket), mFollowPawn.transform.rotation, -1, 1, (effectComp) =>
        {
            mSpawnEffect = effectComp;
        }); 
    }

    protected void HandleNameTagChangeModel()
	{
        if (mFollowPawn == null || mFollowPawn.gameObject == null)
		{
            EffectNameTagDetach();
            return;
        }

        Transform rayCast = RayCastTarget ? mFollowPawn.GetSocket(mRaycastSocket) : null;
        ChangeFollowTarget(mFollowPawn.gameObject, mFollowPawn.GetSocket(mFollowSocket), rayCast);
        OnNameTagRefreshTarget();
        OnNameTagModelLoaded();
    }

    protected void ChangeFollowTarget(GameObject target, Transform followTransform, Transform raycastTransform)
    {        
        NameTagFollowTarget(target, followTransform, raycastTransform, UnityConstants.Layers.OnlyIncluding(UnityConstants.Layers.Default, UnityConstants.Layers.Floor, UnityConstants.Layers.IgnoreCollision));
    }

    //-------------------------------------------------------------------
    protected virtual void OnNameTagInitialize(ZPawn _followPawn) { }
    protected virtual void OnNameTagRefreshTarget() { }
    protected virtual void OnNameTagPawnDie() { }
    protected virtual void OnNameTagModelLoaded() { }
     
}
 