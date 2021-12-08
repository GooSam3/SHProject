using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using System;

/// <summary> ZPawn AI </summary>
public class EntityComponentAI : EntityComponentBase<ZPawn>
{    
    public BehaviourTreeOwner BTOwner { get; private set; }
    public ZPawnBlackboard Blackboard { get; private set; }

    public bool IsRunning { get { return BTOwner?.isRunning ?? false; } }

    public E_PawnAIType CurrentAIType { get; private set; } = E_PawnAIType.None;

    /// <summary> 해당 타입의 AI 셋팅 </summary>
    public virtual void StartAI(E_PawnAIType type)
    {
        if (type == CurrentAIType)
            return;

        //enum 하나로 처리하기 위한 예외처리
        if(Owner.IsMyPc)
        {
            if (E_PawnAIType.AutoBattle > type)
                return;
        }
        else
        {
            if (E_PawnAIType.AutoBattle <= type)
                return;
        }

        StopAI(true);

        CurrentAIType = type;

        StartAI(GetAssetName(type));
    }

    /// <summary> 해당 asset의 Ai 셋팅 </summary>
    private void StartAI(string assetName)
    {        
        BTOwner = Owner.gameObject.GetOrAddComponent<BehaviourTreeOwner>();
        Blackboard = Owner.gameObject.GetOrAddComponent<ZPawnBlackboard>();

        BTOwner.blackboard = Blackboard;

        BTOwner.firstActivation = NodeCanvas.Framework.GraphOwner.FirstActivation.Async;
        BTOwner.enableAction = NodeCanvas.Framework.GraphOwner.EnableAction.DoNothing;
        BTOwner.updateMode = Graph.UpdateMode.NormalUpdate;
                
        ZResourceManager.Instance.Load<BehaviourTree>(assetName, (addressableName, treeAsset) =>
        {
            if (Owner == null) return;

            if (null != treeAsset)
            {
                BTOwner.StartBehaviour(treeAsset);
                mEventStartAI?.Invoke(CurrentAIType);
            }
                
        });
    }

    public void StopAI(E_PawnAIType aiType)
    {
        if (false == IsCurrentAI(aiType))
		{
            mEventStopAI?.Invoke(aiType);
            return;
        }

        StopAI(false);
    }

    public bool IsCurrentAI(E_PawnAIType aiType)
    {
        return IsCurrentAI(GetAssetName(aiType));
    }

    protected bool IsCurrentAI(string aiAssetName)
    {
        if (null == BTOwner)
            return false;
        if (null == BTOwner.graph)
            return false;

        return BTOwner.graph.name.Equals(aiAssetName);
    }

    protected string GetAssetName(E_PawnAIType aiType)
    {
        return $"BT_Pawn_{aiType}";
    }

    /// <summary> AI 정지 </summary>
    protected virtual void StopAI(bool bNextAI)
    {
        BTOwner?.StopBehaviour();

        if(CurrentAIType != E_PawnAIType.None)
            mEventStopAI?.Invoke(CurrentAIType);

        CurrentAIType = E_PawnAIType.None;
    }

    #region ===== :: Event :: =====
    private Action<E_PawnAIType> mEventStartAI;
    private Action<E_PawnAIType> mEventStopAI;

    public void DoAddEventStartAI(Action<E_PawnAIType> action)
    {
        DoRemoveEventStartAI(action);
        mEventStartAI += action;
    }

    public void DoRemoveEventStartAI(Action<E_PawnAIType> action)
    {
        mEventStartAI -= action;
    }

    public void DoAddEventStopAI(Action<E_PawnAIType> action)
    {
        DoRemoveEventStopAI(action);
        mEventStopAI += action;
    }

    public void DoRemoveEventStopAI(Action<E_PawnAIType> action)
    {
        mEventStopAI -= action;
    }

    #endregion
}
