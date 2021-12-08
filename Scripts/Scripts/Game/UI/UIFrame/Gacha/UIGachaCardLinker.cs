using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;

public class UIGachaCardLinker : MonoBehaviour
{
    [SerializeField] private List<UIGachaCardSlot> listCardSlot;

    // 카드 외 추가로 상태관리할 오브젝트들
    [SerializeField] private List<GameObject> listChangeStateTarget;

    [SerializeField] private PlayableDirector timeline;

    [SerializeField] private UIGachaCardSlot goldCardSlot;

    [SerializeField] private bool isUseGoldCard = false;
    [SerializeField] private ParticleSystem fxGoldCard;

    [SerializeField] private List<GameObject> listObjRare;
    [SerializeField] private List<GameObject> listObjNormal;


    private Action OnTimelineEnd;

    private bool isTimelineEnd = false;

    private float timer = 0f;

    private bool isInitialized = false;

    private int turnCnt;

    private bool isGoldCardSeq = false;

    public bool IsHidden { get; private set; } = false;

    public Action onCardTurn = null;

    public void Initialize(UIGachaEnum.E_GachaStyle style,  List<uint> listTid, Action _onTimelineEnd, Action _onCardTurn = null, bool isAllTurned = false)
    {
        isTimelineEnd = false;
        OnTimelineEnd = _onTimelineEnd;
        timer = 0f;
        turnCnt = 0;
        isInitialized = true;

        onCardTurn = _onCardTurn;
        timeline.RebuildGraph();

        if (isUseGoldCard)
        {
            goldCardSlot.gameObject.SetActive(false);
            //fxGoldCard.Stop();
            //fxGoldCard.Play();

            //goldCardSlot.AttachParticleSystem(ref fxGoldCard);
        }

        bool isRareSeq = false;

        for (int i = 0; i < listCardSlot.Count; i++)
        {
            if (listTid.Count <= i)
            {
                listCardSlot[i].gameObject.SetActive(false);
                continue;
            }
            else
                listCardSlot[i].gameObject.SetActive(true);
            listCardSlot[i].SetSlotData(style, listTid[i], OnSlotTurn, isTurned : isAllTurned);

            if (isRareSeq == false && listCardSlot[i].IsRare())
                isRareSeq = true;
        }

        listObjRare.ForEach(item => item.gameObject.SetActive(isRareSeq));
        listObjNormal.ForEach(item => item.gameObject.SetActive(!isRareSeq));
    }

    /// <summary>
    ///  타임라인에 존재하는 카드의 visible상태를 제어
    /// </summary>
    /// <param name="viewState"></param>
    public void SetState(bool viewState)
    {
        listCardSlot.ForEach(item => item.SetState(viewState));
        listChangeStateTarget.ForEach(item => item.SetActive(viewState));

        if (turnCnt >= listCardSlot.Count)
        {
            if (UIManager.Instance.Find(out UIFrameGacha _gacha))
            {
                _gacha.TurnAllCardBtn.gameObject.SetActive(false);
                _gacha.CloseBtn.gameObject.SetActive(true);

                if(_gacha.SpecialShopTid>0)
                    _gacha.ReplayGachaBtn.gameObject.SetActive(true);
            }
        }

    }

    public void Clear()
    {
        timeline.RebuildGraph();
        timeline.initialTime = 0;
        listCardSlot.ForEach(item => item.Clear());
    }

    public void TurnAll()
    {
        StartCoroutine(CoSmoothTurnAll());
        //listCardSlot.ForEach(item => item.SetTurn(true));
    }


    public void SetActiveGoldCard(UIGachaEnum.E_GachaStyle style, uint tid)
    {
        isGoldCardSeq = true;
        if (isUseGoldCard == false)
            return;
        goldCardSlot.SetSlotData(style, tid, null, true, OnClickGoldCard);

        goldCardSlot.gameObject.SetActive(true);
    }

    // 골드카드 시퀀스 종료(카드클릭시)
    private void OnClickGoldCard()
    {
        if (isGoldCardSeq == false)
            return;

        SetEndGoldCardSequence();

        if (UIManager.Instance.Find(out UIFrameGacha _gacha))
            _gacha.GachaVideo.PlayVideoFinish(_gacha.GachaVideo.DefaultVideo);
    }

    // 골드카드 시퀀스 종료
    public void SetEndGoldCardSequence()
    {
        if (isGoldCardSeq == false)
            return;

        isGoldCardSeq = false;

        if (isUseGoldCard)
            goldCardSlot.gameObject.SetActive(false);

        SetState(true);

    }

    private void OnSlotTurn()
    {
        if (++turnCnt >= listCardSlot.Count)
        {
            if (UIManager.Instance.Find(out UIFrameGacha _gacha))
            {
                _gacha.TurnAllCardBtn.gameObject.SetActive(false);
                _gacha.CloseBtn.gameObject.SetActive(true);

                if (_gacha.SpecialShopTid > 0)
                    _gacha.ReplayGachaBtn.gameObject.SetActive(true);
            }
        }

        onCardTurn?.Invoke();
    }

    private void Update()
    {
        if (isInitialized == false)
            return;

        if (isTimelineEnd == false)
        {
            timer += Time.deltaTime;

            if (timer >= timeline.duration)
            {
                OnTimelineEnd?.Invoke();
                listCardSlot.ForEach(item => item.SetReady());
                isTimelineEnd = true;
            }
        }

    }

    public IEnumerator CoSmoothTurnAll()
    {
        foreach (var iter in listCardSlot)
        {
            if (iter.TurnState == true)
                continue;
            if (iter.gameObject.activeSelf == false)
                continue;

            iter.SetTurn(true);

            if (iter.Grade >= ZUIConstant.RARE_SEQUENCE_GRADE)
            {
                if (UIManager.Instance.Find(out UIFrameGacha _gacha))
                {
                    if (!_gacha.SkipMode && _gacha.CurrentGachaStyle != UIGachaEnum.E_GachaStyle.Class)
                    {
                        yield break;
                    }
                }
            }

            yield return new WaitForSeconds(.1f);
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        listCardSlot.Clear();
        listCardSlot.AddRange(gameObject.GetComponentsInChildren<UIGachaCardSlot>());
        timeline = GetComponent<PlayableDirector>();
    }
#endif
}
