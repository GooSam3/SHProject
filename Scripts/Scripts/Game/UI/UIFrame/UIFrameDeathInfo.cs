using GameDB;
using UnityEngine;
using uTools;
using ZNet.Data;

/// <summary> 사망시 팝업 </summary>
public class UIFrameDeathInfo : ZUIFrameBase
{
    [SerializeField]
    private GameObject ObjPopup;
    [SerializeField]
    private uTweenAlpha TweenBackground;
    [SerializeField]
    private ZText AttackerNameText;
    [SerializeField]
    private ZText ExpText;
        
    public void Init(uint attackerEntityId)
    {
        if(UIManager.Instance.Find(out UIPopupSystem system))
		{
            system.Close();
		}

        ObjPopup.SetActive(false);
        TweenBackground.onFinished.AddListener(OnFadeFinish);
        
        if (ZPawnManager.Instance.TryGetEntity(attackerEntityId, out var pawn))
        {
            if(pawn.EntityType == E_UnitType.Character)
            {
                AttackerNameText.text = pawn.EntityData.Name; 
            }
            else
            {
                AttackerNameText.text = $"NPC:{pawn.EntityData.Name}";
            }
        }
        else
        {
            AttackerNameText.text = "-";
        }
        
        //exp 업데이트 타이밍이 ui open 전인지 후인지 알수없음. 선처리 후 데이터 받으면 갱신
        SetExp(Me.CurCharData.PreExp, Me.CurCharData.Exp);
        Me.CurCharData.ExpUpdated += OnSetExp;
    }

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        UIManager.Instance.TopMost<UIFrameDeathInfo>(true);
	}

	private void OnFadeFinish()
    {
        ObjPopup.SetActive(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
        TweenBackground.onFinished.RemoveListener(OnFadeFinish);
        Me.CurCharData.ExpUpdated -= OnSetExp;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        TweenBackground.onFinished.RemoveListener(OnFadeFinish);
        Me.CurCharData.ExpUpdated -= OnSetExp;
    }

    public void OnSetExp(ulong preExp, ulong newExp, bool isMonsterKill)
    {
        Me.CurCharData.ExpUpdated -= OnSetExp;
        SetExp(preExp, newExp);
    }

    private void SetExp(ulong preExp, ulong newExp)
    {
        ulong offset = preExp > newExp ? preExp - newExp : 0;

        if(0 != offset)
        {
            float expRate = DBLevel.GetExpRate(offset, Me.CurCharData.Level, Me.CurCharData.TID);
            ExpText.text = $"{offset}({expRate * 100f}%)";
        }
        else
        {
            ExpText.text = "-";
        }
    }

    /// <summary> 부활 버튼 </summary>
    public void OnResurrection()
    {
        if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.BossWar)
        {
            if (DBPortal.TryGet(ZGameModeManager.Instance.Table.DeathPortal, out Portal_Table portal))
            {
                ZGameManager.Instance.TryEnterBossWarCamp(portal.PortalID, portal.UseItemID, () => Me.CurCharData.BossWarContainer.Location = BossWarContainer.E_Location.Camp);

                UIManager.Instance.Close<UIFrameDeathInfo>(true);
                return;
            }
        }

        //현재 스테이지를 못 찾으면 마을로 보내자
        uint portalTid = DBConfig.Town_Portal_ID;
        if(DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if(0 < stageTable.DeathPortal)
            {
                portalTid = stageTable.DeathPortal;
            }
            else
            {
                ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})의 DeathPortal이 셋팅되지 않았다.");
            }
        }
        else
        {
            ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})를 찾을 수 없다.");
        }



        ZGameManager.Instance.TryEnterStage(portalTid, false, 0, 0);
        UIManager.Instance.Close<UIFrameDeathInfo>(true);
    }
}