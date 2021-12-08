using UnityEngine;
using UnityEngine.UI;

/// <summary> 파티 버프 슬롯 </summary>
public class UISubHUDPartyBuffSlot : MonoBehaviour
{
    [SerializeField]
    private Image ImgIcon;

    private void Awake()
    {
        ImgIcon.gameObject.SetActive(false);
    }

    public void SetBuff(string iconName)
    {
        ImgIcon.gameObject.SetActive(false == string.IsNullOrEmpty(iconName));
        ImgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(iconName);
    }
}
