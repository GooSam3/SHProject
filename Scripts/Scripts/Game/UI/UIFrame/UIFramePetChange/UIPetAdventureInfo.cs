using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

public class UIPetAdventureInfo : MonoBehaviour
{
    [SerializeField] private GameObject objInfo;

    [SerializeField] private GameObject objDetail;
    [SerializeField] private GameObject objInprogress;

    [SerializeField] private Text txtInprogress;
    [SerializeField] private Text txtTitle;
    [SerializeField] private Image imgIcon;

    [SerializeField] private Text txtRemainTime;

    [SerializeField] private UIItemScrollAdapter osaRewardClear;
    [SerializeField] private UIItemScrollAdapter osaRewardExtra;

    //---detail
    [SerializeField] private Text txtNoticeCondition;
    [SerializeField] private PetAdventureRegistList registPetList;

    [SerializeField] private ZPointCatch imgRegistinputListener;

    [SerializeField] private ZButton btnAdventure;

    //---processViewer
    [SerializeField] private PetAdventureProgressViewer progressViewer;
    [SerializeField] private PetAdventureRegistList registPetListInProgress;

    [SerializeField] private ZButton objReward;
    [SerializeField] private ZButton objCancel;

    [SerializeField] private ZButton objResetCooltime;

    [SerializeField] private Text txtRemainTimeProgressView;

    [SerializeField] private Text txtProgressRewardCnt;

    //!!!!!!!!!긴급상황!!!!!
    [SerializeField] private GameObject objPopup;
    [SerializeField] private UIPopupItemInfo popupInfo;
    public OSA_AdventureData Data { get; private set; }

    public void Initialize()
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemSlot), obj=>
        {
            osaRewardClear.Initialize(OnClickItemSlot);
            osaRewardExtra.Initialize(OnClickItemSlot);

            ZPoolManager.Instance.Return(obj);
        });

        registPetList.Initialize();
        progressViewer.Initialize();
    }

    public void SetInfoPanel(OSA_AdventureData _data)
    {
        Data = _data;

        bool isInProgress = Data.advData.status == E_PetAdvStatus.Start;

        txtTitle.text = DBLocale.GetText(Data.table.AdventureNameText);
        imgIcon.sprite = UICommon.GetSprite(Data.table.AdventureIcon);
        txtInprogress.gameObject.SetActive(isInProgress);

        imgRegistinputListener.raycastTarget = Data.advData.status == E_PetAdvStatus.Wait;

        objDetail.SetActive(!isInProgress);
        objInprogress.SetActive(isInProgress);
        BlockInteractible(false);

        if (isInProgress)
        {
            SetInprogress();
        }
        else
        {
            SetInfoDetail();
        }

        objInfo.SetActive(true);

        RefreshRemainTime((long)Data.advData.EndDt - (long)TimeManager.NowSec);
    }

    // 매초 갱신해야될거
    public void RefreshRemainTime(long remainTime)
    {
        if (Data == null)
            return;

        switch (Data.advData.status)
        {
            case E_PetAdvStatus.Wait://할거없음 (쿨타임에서 넘어옴)
                if (objResetCooltime.gameObject.activeSelf)
                    objResetCooltime.gameObject.SetActive(false);

                txtRemainTime.gameObject.SetActive(false);
                break;
            case E_PetAdvStatus.Start://진행중 시간, 보상갯수
                bool isInProgress = remainTime > 0;
                txtRemainTime.gameObject.SetActive(isInProgress);
                txtRemainTimeProgressView.gameObject.SetActive(isInProgress);

                int curReward = (int)Data.advData.rewardCnt;
                if (isInProgress)
                {
                    var dt = Mathf.Clamp(Data.advData.StartDt + 300, Data.advData.StartDt, Data.advData.EndDt);

                    curReward = Mathf.CeilToInt(Data.advData.rewardCnt * ((Data.advData.EndDt - dt + 1) / (Data.advData.EndDt - Data.advData.StartDt)));

                    txtInprogress.text = DBLocale.GetText("PetAdventure_Leaving");
                }
                else
                    txtInprogress.text = DBLocale.GetText("PetAdventur_End");

                txtProgressRewardCnt.text = $"x{curReward}";

                objReward.gameObject.SetActive(!isInProgress);
                objCancel.gameObject.SetActive(isInProgress);

                break;
            case E_PetAdvStatus.Reward://쿨타임
            case E_PetAdvStatus.Cancel://쿨타임
                txtRemainTime.gameObject.SetActive(remainTime > 0);

                if (objResetCooltime.gameObject.activeSelf==false)
                    objResetCooltime.gameObject.SetActive(true);
                break;
        }


        if (remainTime <= 0)
            remainTime = 0;

        if (txtRemainTimeProgressView.gameObject.activeSelf)
        {
            txtRemainTimeProgressView.text = TimeHelper.GetRemainTime((ulong)remainTime);
        }

        if (txtRemainTime.gameObject.activeSelf)
        {
            txtRemainTime.text = TimeHelper.GetRemainTime((ulong)remainTime);
        }
    }

    // 탭 전환
    public void Release()
    {
        Data = null;

        objInfo.SetActive(false);

        // 로드한 모델링 해제 정도
        progressViewer.SetDefault();
    }

    public void OnShowFrame()
    {
        progressViewer.SetDefault();
    }

    public void OnCloseFrame()
    {
        // 로드한 씬 해제
        progressViewer.Release();
    }

    private void SetInfoDetail()
    {
        txtNoticeCondition.text = DBLocale.GetText("PetAdventure_Join_Pet_Des", Data.table.NeedPetPower);

        var listClearDrop = CreateDropList(Data.table.DefaultDropGroupID);
        var listExtraDrop = CreateDropList(Data.table.AdvancedDropGroupID_1, Data.table.AdvancedDropGroupID_2, Data.table.AdvancedDropGroupID_3);

        var resultExtraDrop = new List<ScrollItemData>();

        // 추가보상 내 기본보상 제거
        foreach(var iter in listExtraDrop)
        {
            if (listClearDrop.Find(item => item.tid == iter.tid) != null)
                continue;
            resultExtraDrop.Add(iter);
        }


        osaRewardClear.ResetData(listClearDrop);
        osaRewardExtra.ResetData(resultExtraDrop);

        registPetList.Refresh(Data.table.AdventureMaxSlotCnt);
        btnAdventure.interactable = false;

        switch (Data.advData.status)
        {
            case E_PetAdvStatus.Wait:
                break;
            case E_PetAdvStatus.Cancel:
                break;
        }
    }

    public List<ulong> GetRegistedPetList()
    {
        var listID = new List<ulong>();

        foreach (var iter in registPetList.RegistedList)
        {
            listID.Add(Me.CurCharData.GetPetData(iter).PetId);
        }

        return listID;
    }

    public void BlockInteractible(bool state = true)
    {
        btnAdventure.interactable = !state;

        objReward.interactable = !state;
        objCancel.interactable = !state;

        objResetCooltime.interactable = !state;
    }

    private void SetInprogress()
    {
        bool isInProgress = Data.advData.EndDt > TimeManager.NowSec; // false = reward

        progressViewer.SetProgress(Data, isInProgress);

        List<uint> petList = new List<uint>();

        foreach (var iter in Me.CurCharData.GetPetDataList())
        {
            if (iter.AdvId == Data.advData.AdvId)
                petList.Add(iter.PetTid);
        }
        registPetListInProgress.SetUI(petList);
    }

    private void OnClickItemSlot(ScrollItemData itemData)
    {
        objPopup.SetActive(true);
        popupInfo.Initialize(E_ItemPopupType.None, itemData.tid, () => objPopup.SetActive(false), itemData.Count);
        popupInfo.SetOFFButtonGroup();


        // 아이템 팝업출력
    }

    public void OnConfirmRegist(List<uint> listRegisted)
    {
        registPetList.SetUI(listRegisted);

        uint advPower = 0;
        foreach (var iter in listRegisted)
            advPower += DBPetAdventure.GetPetAdventurePower(iter);

        btnAdventure.interactable = advPower >= Data.table.NeedPetPower;
    }

    public void OnClickPetSlot()
    {
        UIManager.Instance.Open<UIScreenBlock>();
        UIManager.Instance.Open<UIPopupRegistPetAdventure>((str, popup) =>
        {
            popup.SetPopup(Data, registPetList.RegistedList, OnConfirmRegist);
        });
    }

    private List<ScrollItemData> CreateDropList(params uint[] groupTid)
    {
        List<ScrollItemData> list = new List<ScrollItemData>();

        Dictionary<uint, MonsterDrop_Table> dicReward = new Dictionary<uint, MonsterDrop_Table>();

        foreach (var gid in groupTid)
        {
            if (DBMonster.GetDropTableList(gid, out var clearReward))
            {
                foreach (var iter in clearReward)
                {
                    var groupId = DBItem.GetViewGroupId(iter.DropItemID);

                    if (dicReward.ContainsKey(groupId) == false)
                    {
                        dicReward.Add(groupId, iter);
                    }
                    else if (DBItem.GetBelongType(iter.DropItemID) == GameDB.E_BelongType.None &&
                             DBItem.GetBelongType(dicReward[groupId].DropItemID) == GameDB.E_BelongType.Belong)
                    {
                        dicReward[groupId] = iter;
                    }
                }
            }
        }

        foreach (var iter in dicReward.Values)
        {
            list.Add(new ScrollItemData(iter.DropItemID, iter.DropItemMinCount) { slotType = ScrollItemData.E_SlotType.View });
        }

        return list;
    }
}

[Serializable]
public class PetAdventureRegistList
{
    [Serializable]
    private class PetRegistSlotPair
    {
        public bool HasValue { get; private set; } = false;

        [SerializeField] private GameObject objParent;
        [SerializeField] private UIPetChangeListItem registedPet;

        public void SetSlot(uint petTid, bool interectParent = false)
        {
            HasValue = petTid > 0;

            if (interectParent)
                objParent.SetActive(false);
            else
                registedPet.gameObject.SetActive(HasValue);

            if (HasValue)
            {
                if (interectParent)
                    objParent.SetActive(true);

                registedPet.SetSlotSimple(E_PetChangeViewType.Pet, petTid, postSetting: E_PCR_PostSetting.GainStateOff);
            }
        }
    }

    [SerializeField] private List<PetRegistSlotPair> listRegistSlotPair;

    [SerializeField] private Text txtRegistedNum;

    [SerializeField] private bool UpdateText = true;
    [SerializeField] private bool InterectParent = false;

    private int registedMaxNum;

    public List<uint> RegistedList { get; private set; } = new List<uint>();

    public void Initialize()
    {
        Refresh(0);
    }

    public void Refresh(int max)
    {
        RegistedList.Clear();

        foreach (var iter in listRegistSlotPair)
        {
            iter.SetSlot(0);
        }

        registedMaxNum = max;
        RefreshRegistedNum();
    }

    public void SetUI(List<uint> registed)
    {
        RegistedList = registed;

        for (int i = 0; i < listRegistSlotPair.Count; i++)
        {
            if (RegistedList.Count <= i)
            {
                listRegistSlotPair[i].SetSlot(0, InterectParent);
            }
            else
                listRegistSlotPair[i].SetSlot(RegistedList[i], InterectParent);
        }

        RefreshRegistedNum();
    }

    private void RefreshRegistedNum()
    {
        if (UpdateText == false)
            return;

        txtRegistedNum.text = UICommon.GetProgressText(RegistedList.Count, registedMaxNum, false);
    }
}

[Serializable]
public class PetAdventureProgressViewer
{
    private const string TRIGGER_IDLE = "";
    private const string TRIGGER_ATK_PET = "Action_001";
    private const string TRIGGER_ATK_MONSTER = "Attack_001";

    /// <summary>
    /// idx
    ///  1 5
    /// 3   7
    ///  0 4
    /// 2   6
    /// 
    ///   ^
    ///   |
    ///  cam
    /// </summary>


    private struct PetAdvResourceData
    {
        public int idx;//  
        public string resourceName;// 
        public Vector3 scale;
        public string atkTrigger;
        public bool useHitFx;

        public PetAdvResourceData(int _idx, string _name, Vector3 _scale, string _atkTrigger, bool _useHitFx)
        {
            idx = _idx;
            resourceName = _name;
            scale = _scale;
            atkTrigger = _atkTrigger;
            useHitFx = _useHitFx;
        }
    }

    [SerializeField] private RawImage imgProcessViewer;

    [SerializeField] private Image imgFadePanel;

    // {pet filename hash : obj} 나만의 작은 풀
    private Dictionary<int, List<GameObject>> dicModelPool = new Dictionary<int, List<GameObject>>();
    // {pet filename hash : attacktrigger}
    private Dictionary<int, string> dicTrigger = new Dictionary<int, string>();

    private List<PetAdvResourceData> listResData = new List<PetAdvResourceData>();
    private bool isLoadScene = false;

    // 프레임 내려갔는지 => 비동기타이밍 미스로인한 오브젝트 해제처리용
    private bool isReserveUnload = false;
    private PetAdvSceneRoot sceneRoot = null;

    private int viewModelCnt;// 현재 소환된(보여지고있는) 모델 갯수
    private int destModelCnt;// 목표 모델 갯수

    private bool isAttackAnim = false;

    private int uniqueKey = 0;// 현재 소환 유니크키 (미동기 타이밍미스 방지용)

    /// <summary>
    /// 나올법한 상황
    /// 
    /// 모델로드중 -> 다른탭
    /// 모델로드중 -> 
    /// </summary>

    public void Initialize()
    {
        isLoadScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_PET_ADV).isLoaded;
        isReserveUnload = false;
        uniqueKey = 0;
        SetDefault();
    }

    public void SetProgress(OSA_AdventureData data, bool _isInProgress)
    {
        SetDefault();

        isAttackAnim = _isInProgress;

        uniqueKey++;

        // 처음들어왔다면 씬+모델링 모두 로드
        if (isLoadScene == false)
        {
            LoadSubScene();
        }

        // 두번째부터라면 모델링만 로드

        // 나의 펫

        int idx = 0;

        listResData.Clear();

        foreach (var iter in Me.CurCharData.GetPetDataList())
        {
            if (data.advData.AdvId != iter.AdvId)
                continue;

            if (DBPet.TryGet(iter.PetTid, out var table) == false)
                continue;

            var scale = Vector3.one * (table.ViewScale * .01f);

            listResData.Add(new PetAdvResourceData(idx++, table.ResourceFile, scale, TRIGGER_ATK_PET,true));
        }

        // 적펫

        // 모험끝나면 출력안함
        if (isAttackAnim)
        {
            listResData.Add(GetResData(4, data.table.BattleMonsterResourceID_1, data.table.MonsterScale_1));
            listResData.Add(GetResData(5, data.table.BattleMonsterResourceID_2, data.table.MonsterScale_2));
            listResData.Add(GetResData(6, data.table.BattleMonsterResourceID_3, data.table.MonsterScale_3));
            listResData.Add(GetResData(7, data.table.BattleMonsterResourceID_4, data.table.MonsterScale_4));
        }

        if (isLoadScene)
            StartLoadModel(uniqueKey);

        PetAdvResourceData GetResData(int _idx, uint resID, float scale)
        {
            var resData = new PetAdvResourceData();

            if (DBResource.TryGet(resID, out var resTable) == false)
                return resData;

            resData.idx = _idx;
            resData.scale = Vector3.one * (scale * .01f);
            resData.resourceName = resTable.ResourceFile;
            resData.atkTrigger = TRIGGER_ATK_MONSTER;
            resData.useHitFx = false;

            return resData;
        }
    }

    public void SetDefault()
    {
        //initialize이 한번만 실행될때 아래로직 제거
        isLoadScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_PET_ADV).isLoaded;
        isReserveUnload = false;

        //----
        imgFadePanel.CrossFadeAlpha(1, 0f, true);

        sceneRoot?.ClearSpawnDic();

        foreach (var iter in dicModelPool.Values)
        {
            foreach (var model in iter)
            {
                model.transform.SetParent(sceneRoot.SpawnRoot);
                model.SetActive(false);
            }
        }

        destModelCnt = 0;
        viewModelCnt = 0;
    }

    public void Release()
    {
        imgProcessViewer.texture = null;
        isReserveUnload = true;
        sceneRoot = null;

        UnloadSubScene();
        UnloadModel();
    }

    //-----SCENE

    private void ResetSpawnCondition()
    {

    }

    private void LoadSubScene()
    {
        ZSceneManager.Instance.OpenAdditive(ZUIConstant.SUB_SCENE_PET_ADV,
                            (float _progress) =>
                            {
                                ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
                            },
                            (onLoadEnd) =>
                            {
                                OnLoadSubScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_PET_ADV));
                            });
    }

    private void OnLoadSubScene(UnityEngine.SceneManagement.Scene scene)
    {
        if (isReserveUnload)
        {
            UnloadSubScene();
            return;
        }


        foreach (var iter in scene.GetRootGameObjects())
        {
            if (iter.name.Equals("Root"))
            {
                iter.transform.position = new Vector3(-10000, -10000, 0);

                sceneRoot = iter.GetComponent<PetAdvSceneRoot>();

                Vector2 size = new Vector2(imgProcessViewer.rectTransform.rect.width, imgProcessViewer.rectTransform.rect.height);
                imgProcessViewer.texture = sceneRoot.SetRenderTexture(size);
            }
        }

        isLoadScene = true;

        if (listResData.Count > 0)
        {
            StartLoadModel(uniqueKey);
        }
    }

    public void UnloadSubScene()
    {
        ZSceneManager.Instance.CloseAdditive(ZUIConstant.SUB_SCENE_PET_ADV, null);
    }

    //--------------Model

    private void StartLoadModel(int _uniqueKey)
    {
        destModelCnt = listResData.Count;
        foreach (var iter in listResData)
            LoadModel(iter, _uniqueKey);
    }

    private void LoadModel(PetAdvResourceData resData, int _uniqueKey)
    {
        if (string.IsNullOrEmpty(resData.resourceName))
        {
            viewModelCnt++;
            return;
        }

        int hash = resData.resourceName.GetHashCode();

        if (dicModelPool.ContainsKey(hash) == false)
        {
            dicModelPool.Add(hash, new List<GameObject>());
            dicTrigger.Add(hash, resData.atkTrigger);
        }

        if (HasViewableModel(hash) == false)
        {
            var res = resData;

            Addressables.InstantiateAsync(res.resourceName).Completed += (obj) =>
            {
                if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    OnLoadModel(res, obj.Result);

                    int capt = _uniqueKey;

                    SetView(capt, hash, res.idx);
                }
            };
        }
        else
            SetView(_uniqueKey, hash, resData.idx);
    }

    private bool HasViewableModel(int hash)
    {
        foreach (var iter in dicModelPool[hash])
        {
            if (iter.activeSelf == false)
                return true;
        }
        return false;
    }


    private void OnLoadModel(PetAdvResourceData res, GameObject obj)
    {
        if (isReserveUnload)
        {
            Addressables.ReleaseInstance(obj);
            return;
        }

        obj.transform.SetParent(sceneRoot.SpawnRoot);
        obj.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, res.scale);
        obj.GetOrAddComponent<ViewModelAnimEventListener>().Initialize(res.useHitFx);

        obj.SetActive(false);

        dicModelPool[res.resourceName.GetHashCode()].Add(obj);
    }

    private void SetView(int _uniqueKey, int hash, int idx)
    {
        if (_uniqueKey != uniqueKey)
            return;

        foreach (var iter in dicModelPool[hash])
        {
            if (iter.activeSelf == true)
                continue;


            iter.SetActive(true);
            iter.transform.SetParent(sceneRoot.ListModelRoot[idx]);

            iter.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, iter.transform.localScale);
            
            var modelController = iter.GetComponent<ViewModelAnimEventListener>();
            sceneRoot.AddSpawnDic(idx, modelController);
            modelController.SetTrigger(isAttackAnim ? dicTrigger[hash] : TRIGGER_IDLE);

            viewModelCnt++;

            break;
        }

        if (viewModelCnt >= destModelCnt)
        {
            // 소켓등록
            sceneRoot.SetSocket();

            imgFadePanel.CrossFadeAlpha(0f, .5f, true);
        }
    }

    private void UnloadModel()
    {
        foreach (var iter in dicModelPool.Values)
        {
            foreach (var model in iter)
            {
                model.GetComponent<ViewModelAnimEventListener>().Release();
                Addressables.ReleaseInstance(model);
            }
        }

        dicTrigger.Clear();
        dicModelPool.Clear();
        listResData.Clear();
    }

}