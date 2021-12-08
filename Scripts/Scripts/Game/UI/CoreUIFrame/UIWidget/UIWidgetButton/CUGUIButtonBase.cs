using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

abstract public class CUGUIButtonBase : CUGUIWidgetTweenBase
{
    [SerializeField] private GameObject FocusObject = null;
    private ZButton mButton = null;
    //---------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();       
    }

	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
        mButton = GetButton();
        PrivUIButtonRegistOnClick(mButton);
      
        if (FocusObject != null)
		{
            FocusObject.SetActive(false);
		}
    }

	protected override void OnUIWidgetChangeSprite(Sprite _ChangeSprite)
	{
        if (mButton != null)
		{
            mButton.image.sprite = _ChangeSprite;
		}
	}

	protected override void OnUIWidgetFocus(bool _On)
	{
        if (FocusObject != null)
        {
            FocusObject.SetActive(_On);
        }
    }

	//---------------------------------------------------------
	protected ColorBlock GetUIButtonColor()
	{
        return GetButton().colors;
	}

    protected bool IsUIButtonInteraction()
	{
        return GetButton().interactable;
	}

    protected void SetButtonImage(Sprite _sprite)
	{
        if (mButton.image != null)
		{
            mButton.image.sprite = _sprite;
		}
	}
    
    //---------------------------------------------------------
    private void PrivUIButtonRegistOnClick(ZButton _button)
	{
        bool Regist = true;
        int TotalCount = mButton.onClick.GetPersistentEventCount();
        for (int  i = 0; i < TotalCount; i++)
		{
            if (_button.onClick.GetPersistentMethodName(i) == nameof(HandleUIButtonClick))
			{
                Regist = false;
			}
		}

        if (Regist)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(HandleUIButtonClick);
		}
	}
   
	//---------------------------------------------------------
	public UnityAction ExportButtonClickHandler()
    {
        return HandleUIButtonClick;
    }

    public void SetUIButtonInteraction(bool _interaction)
	{
        GetButton().interactable = _interaction;
        OnUIButtonInteractionOn(_interaction);
    }

    public void DoUIButtonClickEvent()
    {
        if (mButton != null)
            mButton.onClick?.Invoke();
    }

    //---------------------------------------------------------
    private void HandleUIButtonClick()
    {
        ProtUITweenStart();
    }

    private ZButton GetButton()
	{
        if (mButton == null)
		{
            mButton = GetComponent<ZButton>();
		}
       
        return mButton;
	}

    //----------------------------------------------------------
    protected sealed override void ProtUITweenStart() { base.ProtUITweenStart(); }
    protected virtual void OnUIButtonInteractionOn(bool _interaction) { }
}
