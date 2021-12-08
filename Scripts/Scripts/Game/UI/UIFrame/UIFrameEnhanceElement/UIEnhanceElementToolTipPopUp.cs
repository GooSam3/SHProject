using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIEnhanceElement;

public class UIEnhanceElementToolTipPopUp : MonoBehaviour
{
    #region UI Fields
    [SerializeField] RectTransform rectTransform;
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtAbilitySource;
    [SerializeField] private RectTransform contentTxtParent;
    [SerializeField] private RectTransform backgroundCloser;

    // 런타임에 세팅이 필요한데 개수가 부족하면 자동 확장함 
    private List<Text> abilityTxts = new List<Text>();
    #endregion

    List<UIAbilityData> abilityDataPairCached = new List<UIAbilityData>();

    private System.Action OnClosed;

    #region Properties
    public RectTransform RectTransform { get { return rectTransform; } }
    public float BoardHeight { get { return contentTxtParent.rect.height; } }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (txtAbilitySource.gameObject.activeSelf)
            txtAbilitySource.gameObject.SetActive(false);
    }
    #endregion

    public void Open(string title, Color titleColor, Color valueColor, List<AbilityActionTitleValuePair> abilityContents, System.Action onClosed)
    {
        for (int i = 0; i < abilityContents.Count; i++)
        {      
            if (abilityTxts.Count <= i)
            {
                AddContentTxt();
            }

            var t = abilityTxts[i];

            t.color = valueColor;
            t.text = abilityContents[i].title + abilityContents[i].value;
            t.gameObject.SetActive(true);
        }

        txtTitle.text = title;
        txtTitle.color = titleColor;
        this.OnClosed = onClosed;

        gameObject.SetActive(true);
    }

    Text AddContentTxt()
    {
        var newTxt = Instantiate(txtAbilitySource, contentTxtParent);
        if (newTxt != null)
        {
            abilityTxts.Add(newTxt);
        }
        return newTxt;
    }

    private void OnEnable()
    {
        backgroundCloser.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        backgroundCloser.gameObject.SetActive(false);
        abilityTxts.ForEach(t => t.gameObject.SetActive(false));
        OnClosed?.Invoke();
    }
}
