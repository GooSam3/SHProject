using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DBArtifact;

/// <summary>
/// 아티팩트 재료 스크롤 컨트롤 베이스 클래스
/// 현재 중복되는 부분이 세군데이기 때문에 만듬  
/// </summary>
abstract public class UIFrameArtifactMaterialManagementBase : MonoBehaviour
{
    /// <summary>
    /// 하나의 슬롯을 채워넣기 위한 재료 데이터형 
    /// 등록하는 쪽, 등록되는 쪽 슬롯에 따라 변수의 사용 목적이 달라짐. 
    /// </summary>
    public class MaterialDataOnSlot
    {
        public uint tid;
        public E_ArtifactMaterialType matType;
        public byte matGrade;
        public uint oriCnt;
        public uint cntByContext;
        public string gradeSpriteName;

        public bool isChecked;

        public void CopyFrom(MaterialDataOnSlot source)
        {
            this.tid = source.tid;
            this.matType = source.matType;
            this.matGrade = source.matGrade;
            this.oriCnt = source.oriCnt;
            this.cntByContext = source.cntByContext;
            this.gradeSpriteName = source.gradeSpriteName;
            this.isChecked = false;
        }
        public void SetSingleCount()
        {
            oriCnt = cntByContext = 1;
        }
        public MaterialDataOnSlot() { }
        public MaterialDataOnSlot(MaterialDataOnSlot t) { CopyFrom(t); }
        public MaterialDataOnSlot(uint tid, E_ArtifactMaterialType matType, byte matGrade, string gradeSprite, uint cnt, bool isChecked)
        {
            this.tid = tid;
            this.matType = matType;
            this.matGrade = matGrade;
            this.oriCnt = this.cntByContext = cnt;
            this.isChecked = isChecked;
            this.gradeSpriteName = gradeSprite;
        }
    }

    public class MaterialSlot
    {
        public UIFrameArtifactResourceSlot slot;
        public MaterialDataOnSlot data;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollArtifactMaterialGridAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
    protected UIFrameArtifact FrameArtifact;
    #endregion

    #region Properties 
    public List<MaterialDataOnSlot> DataList { get { return Data; } }
    public int DataTotalCount { get { return Data.Count; } }
    public int DataEmptyCount
    {
        get
        {
            int cnt = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].cntByContext == 0)
                    cnt++;
            }
            return cnt;
        }
    }
    public int DataExistCount
    {
        get
        {
            int cnt = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].cntByContext > 0)
                    cnt++;
            }
            return cnt;
        }
    }

    public bool IsAllSlotSet
    {
        get
        {
            int cnt = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].cntByContext > 0)
                    cnt++;
            }
            return Data.Count == cnt;
        }
    }
    #endregion

    #region Protected Fields
    protected List<MaterialDataOnSlot> Data;
    #endregion

    #region Abstract Methods
    abstract protected void OnSlotClicked(int index, MaterialDataOnSlot data);
    #endregion

    #region Protected Methods

    /// <summary>
    /// 데이터 리스트 Resize , 내용은 건들지 않음 
    /// </summary>
    protected void ResizeDataList(List<MaterialDataOnSlot> list, int desiredCnt)
    {
        if (list.Count == desiredCnt)
            return;

        if (list.Count < desiredCnt)
        {
            int addCnt = desiredCnt - list.Count;

            for (int i = 0; i < addCnt; i++)
            {
                list.Add(new MaterialDataOnSlot());
            }
        }
        else
        {
            int removeCnt = list.Count - desiredCnt;
            list.RemoveRange(desiredCnt, removeCnt);
        }
    }

    public void CopyData(
        List<MaterialDataOnSlot> source
        , List<MaterialDataOnSlot> dest)
    {
        ResizeDataList(dest, source.Count);

        for (int i = 0; i < source.Count; i++)
        {
            var created = new MaterialDataOnSlot(source[i]);
            dest[i] = created;
        }
    }

    //protected void SubtractCntByTid(List<MaterialDataOnSlot> list, uint cnt, uint tid)
    //{
    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (list[i] != null
    //            && list[i].tid == tid)
    //        {
    //            if (list[i].cntByContext - (int)cnt <= 0)
    //                list[i].cntByContext = 0;
    //            else
    //            {
    //                list[i].cntByContext -= cnt;
    //            }

    //            return;
    //        }
    //    }
    //}

    //public void SubtractByList(
    //    List<MaterialDataOnSlot> listSubtracted
    //    , List<MaterialDataOnSlot> factor)
    //{
    //    for (int i = 0; i < factor.Count; i++)
    //    {
    //        if (factor[i] != null)
    //        {
    //            SubtractCntByTid(listSubtracted, factor[i].cntByContext, factor[i].tid);
    //        }
    //    }
    //}

    protected void AssignEmptyList(int desiredCnt)
    {
        ClearList();
        ResizeDataList(Data, desiredCnt);
    }

    protected void AssignEmptyList(ref List<MaterialDataOnSlot> list, int desiredCnt)
    {
        if (list != null)
            list.Clear();
        else list = new List<MaterialDataOnSlot>(desiredCnt);

        if (list.Count != desiredCnt)
            ResizeDataList(list, desiredCnt);
    }

    /// <summary>
    /// Source 에서 Dest 로 재료 데이터 하나를 이동시킨다. 
    /// </summary>
    protected bool MoveData(
        bool isDestCounting
        , List<MaterialDataOnSlot> source
        , List<MaterialDataOnSlot> dest
        , E_ArtifactMaterialType type
        , uint grade)
    {
        var targetSource = source.Find(t =>
        t.cntByContext > 0 && t.matType == type && t.matGrade == grade);

        if (targetSource == null)
            return false;

        for (int i = 0; i < dest.Count; i++)
        {
            if (dest[i].matType == type &&
                dest[i].matGrade == grade)
            {
                if (isDestCounting)
                {
                    if (dest[i].cntByContext == 0)
                    {
                        dest[i].tid = targetSource.tid;
                    }

                    dest[i].cntByContext++;
                    targetSource.cntByContext--;

                    return true;
                }
                else
                {
                    if (dest[i].cntByContext == 0)
                    {
                        dest[i].cntByContext++;
                        dest[i].tid = targetSource.tid;
                        targetSource.cntByContext--;
                        return true;
                    }
                }
            }

            //if (isDestCounting)
            //{
            //if (dest[i].matType == type
            //    && dest[i].matGrade == grade)
            //{

            //    dest[i].cntByContext++;
            //    targetSource.cntByContext--;

            //if (sourceAutoNull
            //    && targetSource.cntByContext == 0)
            //{
            //    source[source.IndexOf(targetSource)] = null;
            //}

            //  return true;
            //}
            //}
            //else
            //{
            //   if (dest[i] == null)
            //    {
            //targetSource.cntByContext--;
            //dest[i] = new MaterialDataOnSlot(targetSource);
            //dest[i].SetSingleCount();
            //return true;
            //   }
            // }
        }

        return false;
    }
    #endregion

    #region Public Methods
    virtual public void Initialize(UIFrameArtifact artifact, bool showCount, bool disableOnCountZero)
    {
        FrameArtifact = artifact;
        Data = new List<MaterialDataOnSlot>(6);
        ScrollAdapter.AddOnClicked(OnSlotClicked);
        ScrollAdapter.Initialize(showCount, disableOnCountZero);
    }

    virtual public void Release()
    {
        ClearList();
        RefreshScroll();
    }

    public void ForeachData(Action<MaterialDataOnSlot> data)
    {
        for (int i = 0; i < Data.Count; i++)
        {
            data?.Invoke(Data[i]);
        }
    }
    public int GetExistCountByGrade(E_ArtifactMaterialType matType, uint grade)
    {
        int cnt = 0;

        for (int i = 0; i < Data.Count; i++)
        {
            if (Data[i].cntByContext > 0 
                &&Data[i].matType == matType
                && Data[i].matGrade == grade)
                cnt++;
        }

        return cnt;
    }

    //public uint GetDataCount(uint tid)
    //{
    //    var target = Data.Find(t => t.tid == tid);
    //    return target != null ? target.cntByContext : 0;
    //}

    //virtual public void ResetDataListByMaterialData(E_PetType type, byte grade)
    //{
    //    var data = DBPet.GetDataByTypeAndGrade(type, grade);

    //    if(data != null)
    //    {
    //        int requiredMaterialCount = data.
    //    }
    //}

    public int GetCurRequiredCnt(List<MaterialType> matList, E_ArtifactMaterialType type, byte grade)
    {
        if (matList == null)
            return 0;

        int cnt = 0;
        for (int i = 0; i < matList.Count; i++)
        {
            if (matList[i].type == type &&
                matList[i].grade == grade)
            {
                cnt++;
            }
        }

        return cnt;
    }

    public bool IsEmptySlotExist()
    {
        for (int i = 0; i < Data.Count; i++)
        {
            if (Data[i].cntByContext == 0)
            {
                return true;
            }
        }

        return false;
    }

    public virtual void ClearList()
    {
        if (Data != null)
            Data.Clear();
    }

    //virtual public void MoveData(
    //    List<MaterialDataOnSlot> target
    //    , E_ArtifactMaterialType type
    //    , uint grade)
    //{
    //    MoveData(true, Data, target, type, grade);
    //}

    virtual public void SetData_First(MaterialDataOnSlot data)
    {
        if (Data.Count > 0)
        {
            Data[0] = data;
        }
        else
        {
            Data.Add(data);
        }
    }

    virtual public void InsertData(MaterialDataOnSlot data, int index)
    {
        if ((Data.Count - 1 >= index) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "not enough array size allocated, make sure of it");
            return;
        }

        Data[index] = data;
    }

    //virtual public void RemoveData(int index)
    //{
    //    Data.RemoveAt(index);
    //}

    /// <summary>
    /// 빈 데이터 슬롯이 존재하는지 체크함 
    /// 존재할시 가장 앞에있는 빈 슬롯 인덱스 세팅
    /// /// </summary>
    //virtual public bool IsAvailableSlotExist(ref int availableIndex)
    //{
    //    return false;
    //}

    /// <summary>
    /// 현재 어댑터 데이터 Notify 
    /// </summary>
    //virtual public void ApplyScrollData()
    //{
    //    ScrollAdapter.ApplyData();
    //}

    /// <summary>
    /// 전체 ScrollData 를 Refresh 
    /// </summary>
    virtual public void RefreshScroll()
    {
        ScrollAdapter.RefreshData(Data);
    }

    /// <summary>
    /// 펫타입 Converting 해서 비교용 
    /// </summary>
    public bool IsSamePetType(E_PetType petType, E_ArtifactMaterialType matType)
    {
        if (petType == E_PetType.Pet && matType == E_ArtifactMaterialType.Pet)
            return true;
        if (petType == E_PetType.Vehicle && matType == E_ArtifactMaterialType.Vehicle)
            return true;

        return false;
    }
    #endregion

    #region Overrides 
    #endregion

    #region Private Methods
    #endregion
}
