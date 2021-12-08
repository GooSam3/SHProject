using System;
using GameDB;
using System.Collections.Generic;
using WebNet;

public class TempleData
{
    public uint StageTid;
    public ulong ClearDts;
    public List<ushort> rewardGachaOpens = new List<ushort>();
}

public enum E_TempleInfoState
{
    /// <summary> 닫힘 </summary>
    Close,
    /// <summary> 입장한 상태 </summary>
    Enter,
    /// <summary> 클리어한 상태 </summary>
    Clear,
    /// <summary> 다시하기 - 클리어한 상태임 </summary>
    Replay,    
}

public class TempleInfoContainer : ContainerBase
{
    public List<TempleData> TempleStages = new List<TempleData>();

    private Action<TempleData> mEventAddStage;

    public void AddStage(TempleStage? stageInfo, bool bInvokeEvent = false)
    {
        TempleData data = GetStage(stageInfo.Value.StageTid);
        if (data == null)
        {
            data = new TempleData();
            TempleStages.Add(data);
        }

        data.StageTid = stageInfo.Value.StageTid;
        data.ClearDts = stageInfo.Value.ClearDt;

        data.rewardGachaOpens.Clear();
        for (int i = 0; i < stageInfo.Value.IsRewardGachaOpensLength; i++)
        {
            data.rewardGachaOpens.Add(stageInfo.Value.IsRewardGachaOpens(i));
        }

        if(true == bInvokeEvent)
        {
            mEventAddStage?.Invoke(data);
        }
    }

    public override void Clear()
    {        
        TempleStages.Clear();
    }

    public TempleData GetStage(uint stageTid)
    {
        var data = TempleStages.Find(obj => obj.StageTid == stageTid);
        return data;
    }

    public E_TempleInfoState GetTempleStartType(uint stageTid)
    {
        // 0 닫힘, 1 입장 2 다시하기 3. 클리어
        var StageCashData = TempleStages.Find(obj => obj.StageTid == stageTid);
        // 1회 입장 했을때 서버에서 넘어옴
        if (StageCashData != null)
        {
            if (DBStage.TryGet(stageTid, out var stageTable))
            {
                if (DBTemple.TryGet(stageTable.LinkTempleID, out var tempelTable))
                {
                    if (StageCashData.ClearDts > 0) // 클리어 서버시간 넘어옴
                    {
                        if (tempelTable.RegistrationUI == E_RegistrationUI.Registration)
                        {
                            if (tempelTable.Replay == E_Replay.Replay) return E_TempleInfoState.Replay;// 리플레이
                            else return E_TempleInfoState.Clear;  //클리어
                        }
                        else return E_TempleInfoState.Clear;// 클리어
                    }
                    else
                    {
                        // 클리어 안됬을시 재 진입 
                        if (tempelTable.RegistrationUI == E_RegistrationUI.Registration) return E_TempleInfoState.Enter; // 입장
                        else return E_TempleInfoState.Clear; // 클리어
                    }
                }
                else return E_TempleInfoState.Close;  // 잠김
            }
            else return E_TempleInfoState.Close;  // 잠김
        }
        else return E_TempleInfoState.Close; // 잠김
    }

    public void DoAddEventAddStage(Action<TempleData> action)
    {
        DoRemoveEventAddStage(action);
        mEventAddStage += action;
    }

    public void DoRemoveEventAddStage(Action<TempleData> action)
    {
        mEventAddStage -= action;
    }
}
