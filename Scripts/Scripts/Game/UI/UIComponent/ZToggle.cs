using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[AddComponentMenu("ZUI/ZToggle", 31)]
[RequireComponent(typeof(RectTransform))]
public class ZToggle : Toggle
{
    /// <summary>클릭 사운드 TableID</summary>
    public uint SoundTID;

    [System.Serializable]
    public class ChangeObject
	{
       public GameObject Target;
       public bool OnActiveFalse;
    }

    [Tooltip("토글이 활성화 될 때 Active 되는 최상위 오브젝트")]
    public GameObject On;
    [Tooltip("토글이 비활성화 될 때 Active 되는 최상위 오브젝트")]
    public GameObject Off;
    [Tooltip("다른 토글 버튼과 연계(라디오 버튼)하여 사용할 경우 등록")]
    public ZToggleGroup ZToggleGroup;
    [Tooltip("토글의 On / Off 여부와 상관 없이 독립적으로 Active를 제어하고 싶은 오브젝트를 등록하는 리스트")]
    public List<ChangeObject> ToggleChangeStyle = new List<ChangeObject>();

    public List<ZSelectTransition> MultiTransitions = new List<ZSelectTransition>();

    private UnityAction<uint> mSoundEvent = null;

    private bool Instant = false;

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);

		if (!IsInteractable())
			return;

        if (SoundTID != 0)
            mSoundEvent?.Invoke(SoundTID);

        if (ZToggleGroup != null)
            SelectToggle();
        else
            UpdateToggle();
    }

	public override void GraphicUpdateComplete()
	{
		base.GraphicUpdateComplete();

        UpdateToggle();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (!gameObject.activeInHierarchy)
            return;

        Instant = instant;

        UpdateToggle();
    }

	private void UpdateToggle()
	{
        if (On != null) On.SetActive(isOn);
        if (Off != null) Off.SetActive(!isOn);

        for (int i = 0; i < ToggleChangeStyle.Count; i++)
        {
            if (ToggleChangeStyle[i].OnActiveFalse)
                ToggleChangeStyle[i].Target.SetActive(!isOn);
            else
                ToggleChangeStyle[i].Target.SetActive(isOn);
        }

        if (MultiTransitions.Count > 0)
        {
            Color tintColor;
            Sprite transitionSprite;

            for (int i = 0; i < MultiTransitions.Count; i++)
            {
                var transition = MultiTransitions[i];

                if (interactable && !isOn)
                {
                    tintColor = transition.m_Colors.normalColor;
                    transitionSprite = null;
                }
                else if (interactable && isOn)
                {
                    tintColor = transition.m_Colors.selectedColor;
                    transitionSprite = transition.m_SpriteState.selectedSprite;
                }
                else
                {
                    tintColor = transition.m_Colors.disabledColor;
                    transitionSprite = transition.m_SpriteState.disabledSprite;
                }

                switch (transition.m_Transition)
                {
                    case Transition.ColorTint:
                        transition.StartColorTween(tintColor * transition.m_Colors.colorMultiplier, Instant);
                        break;
                    case Transition.SpriteSwap:
                        transition.DoSpriteSwap(transitionSprite);
                        break;
                }
            }
        }
    }

    /// <summary>토글 강제 선택</summary>
    /// <param name="_actionEnable">토글에 연결된 콜백 실행 여부</param>
    /// <param name="_actionIdx">실행 또는 실행 취소할 콜백 Idx</param>
    public void SelectToggle(bool _actionEnable = true, int[] _actionIdx = null)
	{
        if (ZToggleGroup != null)
            ZToggleGroup.SelectToggle(this, _actionEnable, _actionIdx);
        else
        {
            if (_actionEnable == false && _actionIdx != null)
            {
                SetAction(_actionEnable, _actionIdx);

                Select();

                SetAction(!_actionEnable);
            }
            else
                Select();
        }

        void Select()
        {
            isOn = !isOn;

            UpdateToggle();
        }
    }

    /// <summary>토글 강제 선택</summary>
    /// <param name="_callback">토글 선택 후 실행할 콜백</param>
    /// <param name="_actionEnable">토글에 연결된 콜백 실행 여부</param>
    /// <param name="_actionIdx">실행 또는 실행 취소할 콜백 Idx</param>
    public void SelectToggleAction(UnityAction<ZToggle> _callback = null, bool _actionEnable = true, int[] _actionIdx = null)
    {
        SelectToggle(_actionEnable, _actionIdx);

        _callback?.Invoke(this);
    }

    /// <summary>단일 토글의 상태를 강제로 바꾼다.(그룹 등록된 토글은 사용 X) </summary>
    /// <param name="_isOnState">바꾸려는 IsOn 상태</param>
    /// <param name="_actionEnable">토글에 연결된 콜백 실행 여부</param>
    /// <param name="_actionIdx">실행 또는 실행 취소할 콜백 Idx</param>
    public void SelectToggleSingle(bool _isOnState, bool _actionEnable = true, int[] _actionIdx = null)
    {
        if (_actionEnable == false)
        {
            SetAction(_actionEnable, _actionIdx);

            Select();

            SetAction(!_actionEnable);
        }
        else
            Select();

        void Select()
        {
            if (isOn != _isOnState)
                isOn = _isOnState;

            UpdateToggle();
        }
    }

    public void SetAction(bool _actionEnable, int[] _actionIdx = null)
    {
        int eventCnt = onValueChanged.GetPersistentEventCount();

        UnityEventCallState _state = _actionEnable ? UnityEventCallState.RuntimeOnly : UnityEventCallState.Off;

        if (_actionIdx == null)
        {
            if (_actionIdx == null)
                for (int i = 0; i < eventCnt; i++)
                    onValueChanged.SetPersistentListenerState(i, _state);
        }
        else
        {
            for (int i = 0; i < _actionIdx.Length; i++)
                if (_actionIdx[i] < eventCnt)
                   onValueChanged.SetPersistentListenerState(_actionIdx[i], _state);
        }
    }

    public void SetSoundEvent(UnityAction<uint> _soundEvent)
	{
        mSoundEvent = _soundEvent;
	}
}