using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameDB;

public class UIInvenArtifactEquipSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image Icon;
    [SerializeField] private Image GradeBoard;
    #endregion

    #region System Variable
    private Sprite oriIconSprite;
    /// <summary>
    /// 0 이면 미장착 상태 
    /// </summary>
    private uint artifactTidEquipped;
    #endregion

    #region Properties
    public uint ArtifactTidEquipped { get { return artifactTidEquipped; } }
    #endregion

    public void SetUI(uint artifactTid)
    {
        if (oriIconSprite == null)
            oriIconSprite = Icon.sprite;

        artifactTidEquipped = artifactTid;

        if (artifactTid == 0)
        {
            SetEmtpy();
            return;
        }

        var data = DBArtifact.GetArtifactByID(artifactTid);

        if (data == null)
        {
            SetEmtpy();
            return;
        }

        Icon.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
        GradeBoard.sprite = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(data.Grade));
        GradeBoard.gameObject.SetActive(true);
    }

    public void OnClickSlot()
    {
        if (ArtifactTidEquipped == 0)
        {
            return;
        }

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupArtifactItemInfo), (_obj) =>
        {
            UISubHUDCharacterState characterState = UIManager.Instance.Find<UISubHUDCharacterState>();
            var obj = _obj.GetComponent<UIPopupArtifactItemInfo>();

            if (obj != null)
            {
                characterState.SetInfoPopup_Artifact(obj);
                obj.transform.SetParent(characterState.gameObject.transform);

                bool popUpOpened = obj.Initialize(
                    ArtifactTidEquipped
                    , new Vector3(880, -535, 0)
                    , true
                    , false
                    , onMoveArtifactUpgradeCallback: () =>
                   {
                       characterState.OnActiveInfoPopup(false);
                   });

                if (popUpOpened == false)
                    characterState.RemoveInfoPopup_Artifact();
            }
        });
    }

    void SetEmtpy()
    {
        Icon.sprite = oriIconSprite;
        GradeBoard.gameObject.SetActive(false);
    }
}
