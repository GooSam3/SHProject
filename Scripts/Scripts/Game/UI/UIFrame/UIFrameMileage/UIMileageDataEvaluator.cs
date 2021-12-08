using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UIFrameMileage;

public class UIMileageDataEvaluator : MonoBehaviour
{
    [Serializable]
    public class Option
    {
        /// <summary>
        /// MileageDataEvaluatorKey Enum 의 String 값 
        /// </summary>
        public string evaluatorKey;
        public Button btn;

        [HideInInspector] public int index;
    }

    [SerializeField] private MileageDataEvaluateTargetDataType targetDataType;

    [SerializeField] private GameObject optionObj;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text txtTitleNormal, txtTitleOn;

    [SerializeField] private ZToggleGroup toggleGroup;
    [SerializeField] private ZToggle toggle;

    [SerializeField] private List<Option> options;

    private Action<UIMileageDataEvaluator, Option> onClicked_option;

    private UIFrameMileage FrameMileage;

    #region Properties
    public RectTransform RectTransform
    {
        get
        {
            return this.rectTransform;
        }
    }

    public MileageDataEvaluateTargetDataType TargetDataType
    {
        get
        {
            return this.targetDataType;
        }
    }
    #endregion

    public void Initialize(UIFrameMileage frameMileage)
    {
        this.FrameMileage = frameMileage;

        if (options.Count > 0)
        {
            var textKey = FrameMileage.GetEvaluatorKeyText(options[0].evaluatorKey);
            SetTitleTxt(DBLocale.GetText(textKey));
        }
        else
        {
            ZLog.LogError(ZLogChannel.UI, "no evaluatorKey option set");
        }
    }

    private void Awake()
    {
        for (int i = 0; i < options.Count; i++)
        {
            options[i].index = i;
        }
    }

    public void SetTitleTxt(string txt)
    {
        txtTitleNormal.text = txt;
        txtTitleOn.text = txt;
    }

    //public void SetOptionToDefault()
    //{
    //    if (toggleGroup.GetToggle(0) == null)
    //        return;

    //    toggleGroup.GetSelectToggleIndex(0);
    //}

    //public void SetOptionManual(MileageDataEvaluatorKey key)
    //{
    //    for (int i = 0; i < options.Count; i++)
    //    {
    //        if (options[i].evaluatorKey == key.ToString())
    //        {
    //            toggleGroup.GetSelectToggleIndex(i);
    //        }
    //    }
    //}

    public void SetToggleIsOn(bool active)
    {
        toggle.isOn = active;
        toggle.GraphicUpdateComplete();
    }

    public void SetOnClicked(Action<UIMileageDataEvaluator, Option> callback)
    {
        onClicked_option = callback;
    }

    public void OpenOption()
    {
        optionObj.SetActive(true);
    }

    public void CloseOption()
    {
        optionObj.SetActive(false);
    }

    public void OnOptionClicked(GameObject go)
    {
        toggleGroup.SetAllTogglesOff(false);
        toggle.GraphicUpdateComplete();

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].btn.gameObject == go)
            {
                onClicked_option?.Invoke(this, options[i]);
                break;
            }
        }
    }

    //public void OnToggleValueChanged(ZToggle toggle)
    //{
    //    for (int i = 0; i < options.Count; i++)
    //    {
    //        if (options[i].toggle == toggle
    //            && toggle.isOn)
    //        {
    //            onClicked_option?.Invoke(this, options[i]);
    //        }
    //    }
    //}
}