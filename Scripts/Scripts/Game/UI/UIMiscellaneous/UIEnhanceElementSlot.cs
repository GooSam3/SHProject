using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIEnhanceElementSlot : MonoBehaviour
{
    // public delegate void OnClickSlot(ScrollEnhanceElementData data);

    #region UI Variable
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image imgBG;
    [SerializeField] private Image imgNormalStateCircleLine;
    [SerializeField] private Image imgEnhance;

    [SerializeField] private Image imgObtainedInnerglow;
    [SerializeField] private Image imgObtainedEnhance;
    [SerializeField] private RectTransform selectedObjRoot;
    [SerializeField] public GameObject effPossible;
    #endregion

    #region Properties
    public RectTransform RectTransform { get { return rectTransform; } }
    public bool IsLocked { get; private set; }

    public uint tid { get; private set; }
    #endregion

    #region System Variable
    private Action onClick;
    #endregion

    #region Public Methods 
    public void Initialize(Action onClickListener)
    {
        this.onClick = onClickListener;
    }

    public void Set(
        uint TID
        , UIEnhanceElementSettingProvider settingProvider
        , E_UnitAttributeType type
        , Sprite iconSprite
        , Color bgColor
        , Color etcColor
        , bool isObtained
        , bool isNextTarget)
    {
        tid = TID;

        var t = settingProvider.FindTypeGroupProperty(type);

        if (t != null)
        {
            if (isObtained)
			{
				imgObtainedEnhance.sprite = iconSprite;
				imgObtainedInnerglow.color = t.bgInnerGlowOnActive;
				imgObtainedEnhance.color = t.bgEnhanceOnActive;
                imgObtainedInnerglow.gameObject.SetActive(true);
                imgObtainedEnhance.gameObject.SetActive(true);
            }
			else
            {
                imgObtainedInnerglow.gameObject.SetActive(false);
                imgObtainedEnhance.gameObject.SetActive(false);
            }
        }

        imgEnhance.sprite = iconSprite;
        SetImageColor(bgColor, imgBG);
        SetImageColor(etcColor, imgNormalStateCircleLine, imgEnhance);
        selectedObjRoot.gameObject.SetActive(false);
        effPossible.gameObject.SetActive(isNextTarget);
    }

	public void SetSelectedActive(bool active)
	{
        selectedObjRoot.gameObject.SetActive(active);
    }
	#endregion

	#region Private Methods
	private void SetImageColor(Color color, params Image[] img)
    {
        if (img == null)
            return;

        for (int i = 0; i < img.Length; i++)
        {
            if (img[i].color.Equals(color) == false)
                img[i].color = color;
        }
    }
    #endregion

    #region UnityMethods
    #endregion

    #region Overrides
    #endregion

    #region OnClick Event
    public void OnClick()
    {
        onClick?.Invoke();
    }
    #endregion

    #region TEST 
    #endregion
}
