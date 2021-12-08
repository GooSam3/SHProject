using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[AddComponentMenu("ZUI/ZButton", 1)]
[RequireComponent(typeof(RectTransform))]
public class ZButton : Button 
{
    /// <summary>클릭 사운드 TableID</summary>
    public uint SoundTID;

    public List<ZSelectTransition> multiTransitions = new List<ZSelectTransition>();

    private bool isDisableColor;

    //------------------------------------------------------
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);

        if (!IsInteractable())
            return;

        if (AudioManager.Instance && SoundTID > 0)
            AudioManager.Instance.PlaySFX(SoundTID);
    }

    public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
	}

	//-------------------------------------------------------   

    public void SetListener(UnityEngine.Events.UnityAction _callback)
	{
        onClick.RemoveAllListeners();
        onClick.AddListener(_callback);
	}

    public bool HasSelected()
	{
        bool selected = false;
        if (currentSelectionState == SelectionState.Selected)
		{
            selected = true;
		}
        return selected;
	}

    public override void Select()
	{
        base.Select();
	}

    /// <summary> 버튼은 작동하고 버튼 칼라 비활성만 원할때 </summary>
    public void SetDisableColor(bool disable)
    {
        isDisableColor = disable;

        if (disable) {
            for (int i = 0; i < multiTransitions.Count; i++) {
                var transition = multiTransitions[i];
                Color tintColor = transition.m_Colors.disabledColor;
                transition.StartColorTween(tintColor * transition.m_Colors.colorMultiplier, true);
            }

            if (targetGraphic != null) {
                targetGraphic.color = colors.disabledColor;
            }
        }
        else {
            if (targetGraphic != null) {
                targetGraphic.color = colors.normalColor;
            }

            DoStateTransition(currentSelectionState, true);
        }
    }

    //------------------------------------------------------

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (isDisableColor) {
            return;
        }

        base.DoStateTransition(state, instant);

        if (!gameObject.activeInHierarchy || multiTransitions.Count <= 0)
            return;

        Color tintColor;
        Sprite transitionSprite;

        for (int i = 0; i < multiTransitions.Count; i++)
        {
            var transition = multiTransitions[i];

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = transition.m_Colors.normalColor;
                    transitionSprite = null;
                    break;
                case SelectionState.Highlighted:
                    tintColor = transition.m_Colors.highlightedColor;
                    transitionSprite = transition.m_SpriteState.highlightedSprite;
                    break;
                case SelectionState.Pressed:
                    tintColor = transition.m_Colors.pressedColor;
                    transitionSprite = transition.m_SpriteState.pressedSprite;
                    break;
                case SelectionState.Disabled:
                    tintColor = transition.m_Colors.disabledColor;
                    transitionSprite = transition.m_SpriteState.disabledSprite;
                    break;
                case SelectionState.Selected:
                    tintColor = transition.m_Colors.selectedColor;
                    transitionSprite = transition.m_SpriteState.selectedSprite;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    break;
            }

            switch (transition.m_Transition)
            {
                case Transition.ColorTint:
                    transition.StartColorTween(tintColor * transition.m_Colors.colorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    transition.DoSpriteSwap(transitionSprite);
                    break;
            }
        }
    }
}
