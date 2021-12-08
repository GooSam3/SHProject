using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GachaViewController : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Transform fxRootImpact;
    [SerializeField] private Transform fxRootIdle;

    [SerializeField] private Camera viewCamera;

    [SerializeField]
    private List<ParticleSystem> fxListImpact;

    [SerializeField]
    private List<ParticleSystem> fxListIdle;

    // {tid : model}
    private Dictionary<uint, GameObject> dicSpawnModel = new Dictionary<uint, GameObject>();

    private List<uint> listModelTid = new List<uint>();

    private Action onLoadEnd;

    private int loadCnt;
    private int loadDest;

    private int curViewIndex;

    private Coroutine curCoroutine;

#if UNITY_EDITOR

    private void Awake()
    {
        StopFx();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Initialize(new List<uint>() { 10013, 20014, 30010, 40022, 50023, 60017 }, () => print("LOAD_END"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            print(SetNext());
        }
    }

#endif

    /// <summary>
    /// 모델링을 로드한다.
    /// </summary>
    /// <param name="listTid"> 로드할 모델링 tid리스트 </param>
    /// <param name="_onLoadEnd"> 요청한 모델링 로드 끝났을때 콜백 </param>
    public void Initialize(List<uint> listTid, Action _onLoadEnd)
    {
        onLoadEnd = _onLoadEnd;
        //dicSpawnModel.Clear();//!! : 딕셔너리는 clear함수로 초기화
        listModelTid.Clear();

        listModelTid.AddRange(listTid);

        loadCnt = 0;
        loadDest = 0;

        curViewIndex = 0;

        // 한번의 다수의 클래스가 나오는게 아닌관계로 각1개씩만 생성함
        foreach (var iter in listTid)
        {
            if (DBChange.TryGet(iter, out var table) == false)
                continue;

            if (dicSpawnModel.ContainsKey(iter))
                continue;

            dicSpawnModel.Add(iter, null);
            loadDest++;

            uint capt = iter;

            Addressables.InstantiateAsync(DBResource.GetResourceFileName(table.ResourceID)).Completed += (obj) =>
            {
                if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    OnModelLoaded(capt, obj.Result, (Vector3.one * table.Scale * .01f));
                }
            };
        }

        if (loadDest == 0)
        {
            this.gameObject.SetActive(true);
            onLoadEnd?.Invoke();
        }

        UIManager.Instance.DoSubCameraStack(viewCamera, true);
    }

    private void OnModelLoaded(uint key, GameObject obj, Vector3 scale)
    {
        var model = obj;

        model.transform.SetParent(modelRoot);
        model.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, scale);
        model.SetLayersRecursively("UIModel");

        model.SetActive(false);

        dicSpawnModel[key] = model;

        if (++loadCnt >= loadDest)
        {
            this.gameObject.SetActive(true);
            onLoadEnd?.Invoke();
        }
    }

    public void Clear()
    {
        foreach (var iter in dicSpawnModel.Values)
        {
            Addressables.ReleaseInstance(iter);
        }

        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
            curCoroutine = null;
        }

        if(viewCamera != null)
            UIManager.Instance.DoSubCameraStack(viewCamera, false);
    }

    /// <summary>
    /// 다음 모델을 출력한다. 
    /// </summary>
    /// <returns>마지막 놈이었나??</returns>
    public bool SetNext()
    {
        if (curViewIndex >= listModelTid.Count)
        {
            StopFx();
            foreach(var iter in dicSpawnModel.Values)
            {
                if (iter.activeSelf)
                    iter.SetActive(false);
            }
            this.gameObject.SetActive(false);

            return true;
        }

        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
        }

        if (curViewIndex > 0)
        {
            uint pastTid = listModelTid[curViewIndex-1];
            dicSpawnModel[pastTid].SetActive(false);
        }

        curCoroutine = StartCoroutine(CoModelViewTransition());

        ++curViewIndex;

        return false;
    }

    private void StopFx()
    {
        fxListImpact.ForEach(item => { item.Stop(); item.Clear(); });
        fxListIdle.ForEach(item => { item.Stop(); item.Clear(); });
    }

    private IEnumerator CoModelViewTransition()
    {
        //기존놈들꺼줌==================

        StopFx();

        uint curTid = listModelTid[curViewIndex];

        yield return null;//===========

        // 새로들어온놈 임팩트 fx, 모델링 켜줌==============

        if (DBChange.TryGet(curTid, out var table) == false)
        {
            ZLog.LogError(ZLogChannel.System, $"CHANGE_TABLE_TID_NULL TID : {curTid}");

            yield break;
        }

        fxListImpact[table.Grade - 1].Play();

        dicSpawnModel[curTid].SetActive(true);

        yield return new WaitForSeconds(.2f);//=========

        // 유휴fx 켜줌
        fxListIdle[table.Grade - 1].Play();
    }

}
