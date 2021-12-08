using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using static UIFrameArtifactAbilityActionBuilder;
using GameDB;
using ZNet.Data;
using static UIFrameArtifactManufacture;

public class UIPopupArtifactItemInfo : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtName;

    [SerializeField] private Image imgFirstAbilityIcon;
    [SerializeField] private Text txtFirstAbilityTitle;
    [SerializeField] private Text txtFirstAbilityValue;

    [SerializeField] private GameObject upgradeBtnObj;
    [SerializeField] private GameObject equipBtnObj;
    [SerializeField] private Text txtEquip;

    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private UIFrameArtifactSingleAbilityAction contentSource;
    #endregion

    #region UI Variables
    #endregion
    #endregion

    #region System Variables
    private List<UIFrameArtifactSingleAbilityAction> pooledAbilityActionContents = new List<UIFrameArtifactSingleAbilityAction>();
    private List<AbilityActionTitleValuePair> txtData = new List<AbilityActionTitleValuePair>();
    /// <summary>
    ///  아티팩트 AbilityAction 세팅 헬퍼 
    /// </summary>
    private UIFrameArtifactAbilityActionBuilder abilityHelper = new UIFrameArtifactAbilityActionBuilder();

    private uint curArtifactTID;
    private E_PetType curPetType;
    private EquipActionCase curEquipActionState;

    private Action onClose;
    private Action onMoveArtifactUpgrade;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    private void OnDisable()
    {
        onClose?.Invoke();
        onClose = null;
    }
    #endregion

    #region Public Methods
    public bool Initialize(uint artifactTid, Vector3? pos, bool showUpgradeBtn, bool showEquipBtn, Action onCloseCallback = null, Action onMoveArtifactUpgradeCallback = null)
    {
        curEquipActionState = EquipActionCase.None;

        this.onClose = onCloseCallback;
        this.onMoveArtifactUpgrade = onMoveArtifactUpgradeCallback;

        transform.localScale = Vector3.one;
        if (pos.HasValue)
            transform.localPosition = pos.Value;

        if (artifactTid == 0)
        {
            ZLog.LogError(ZLogChannel.UI, " you are tryin to open artifactInfoPopup and the parameter represents nothing ");
            return false;
        }

        var artifactData = DBArtifact.GetArtifactByID(artifactTid);

        if (artifactData == null)
        {
            ZLog.LogError(ZLogChannel.UI, " could not get the artifact data , TID : " + artifactTid);
            return false;
        }

        curPetType = artifactData.ArtifactType;
        curArtifactTID = artifactTid;

        SetBtnEnable(showUpgradeBtn, showEquipBtn);

        if (showEquipBtn)
        {
            curEquipActionState = GetEquipActionCase(artifactData);

            switch (curEquipActionState)
            {
                case EquipActionCase.Equip:
                case EquipActionCase.Switch:
                    {
                        txtEquip.text = DBLocale.GetText("Equip_Text");
                    }
                    break;
                case EquipActionCase.Unequip:
                    {
                        txtEquip.text = DBLocale.GetText("Lift_Text");
                    }
                    break;
            }
        }

        txtData.Clear();

        imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(artifactData.Icon);
        txtName.text = DBLocale.GetText(artifactData.ArtifactName);
        txtName.color = UIFrameArtifact.GetColorByGrade(artifactData.Grade);

        abilityHelper.BuildAbilityActionTexts(ref txtData, artifactData.AbilityActionID);

        /// 첫 번째거는 별도 세팅 
        if (txtData.Count > 0)
        {
            var t = txtData[0];
            var abilityData = DBAbility.GetAbility(t.type);
            var firstAbilityIconSprite = abilityData != null ? ZManagerUIPreset.Instance.GetSprite(abilityData.AbilityIcon) : null;
            imgFirstAbilityIcon.sprite = firstAbilityIconSprite;
            txtFirstAbilityTitle.text = t.title;
            txtFirstAbilityValue.text = t.strValue;
            txtFirstAbilityValue.color = Color.white;
            imgFirstAbilityIcon.gameObject.SetActive(true);
            txtFirstAbilityTitle.gameObject.SetActive(true);
            txtFirstAbilityValue.gameObject.SetActive(true);
        }
        else
        {
            imgFirstAbilityIcon.gameObject.SetActive(false);
            txtFirstAbilityTitle.gameObject.SetActive(false);
            txtFirstAbilityValue.gameObject.SetActive(false);
        }

        abilityHelper.SetAbilityActionUITexts(false, 1, contentSource, txtData, contentRoot, ref pooledAbilityActionContents);

        return true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region Overrides 
    #endregion

    #region Private Methods
    EquipActionCase GetEquipActionCase(Artifact_Table data)
    {
        bool isEquipped = Me.CurCharData.IsArtifactEquippedByType(data.ArtifactType);
        bool isSelectedAlreadyEquipped = Me.CurCharData.IsArtifactEquippedByTID(data.ArtifactID);

        EquipActionCase result = EquipActionCase.None;

        /// 해당 아티팩트의 타입은 이미 장착된 상황 
        if (isEquipped)
        {
            /// 이미 장착돼 있는 아티팩트가 선택한 아티팩트인 상황 
            if (isSelectedAlreadyEquipped)
            {
                result = EquipActionCase.Unequip;
            }
            /// 이미 장착돼 있는 아티팩트와 현재 선택한 아티팩트가 다른 상황 
            else
            {
                result = EquipActionCase.Switch;
            }
        }
        /// 해당 아티팩트 타입이 비어있으므로 그냥 장착할 수 있는 상황
        else
        {
            result = EquipActionCase.Equip;
        }

        return result;
    }

    private void SetBtnEnable(bool upgrade, bool equip)
    {
        upgradeBtnObj.SetActive(upgrade);
        equipBtnObj.SetActive(equip);
    }

    public void OpenTwoButtonQueryPopUp(
    string title, string content, Action onConfirmed, Action onCanceled = null)
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { "취소", "확인" }, new Action[] {
                () =>
                {
                    onCanceled?.Invoke();
                    _popup.Close();
                },
                () =>
                {
                     onConfirmed?.Invoke();
                    _popup.Close();
                }});
        });
    }
    #endregion

    #region Inspector Events 
    #region OnClick
    /// <summary>
    ///  아티팩트 아이템 인포창에서는 해제만 가능하다 . (아티팩트 패널 UI에서)
    /// </summary>
    public void OnClickUnEquip()
    {
        OpenTwoButtonQueryPopUp("확인", "장착 해제하시겠습니까?"
            , onConfirmed: () =>
            {
                ZWebManager.Instance.WebGame.REQ_ArtifactUnEquip((uint)curPetType
                    , (revPacket, resList) =>
                    {
                        var v = UIManager.Instance.Find<UISubHUDCharacterState>();
                        if (v != null
                        && v.Show)
                        {
                            v.UpdateArtifactSlots();
                        }
                    }, null);

                Close();
            });
    }

    /// <summary>
    /// 아티팩트 UI 로 전환. 
    /// </summary>
    public void OnClickUpgrade()
    {
        OpenTwoButtonQueryPopUp("확인", "아티팩트 화면으로 이동하시겠습니까?"
            , onConfirmed: () =>
            {
                UIFrameArtifact ui = null;
                UIManager.Instance.Find(out ui);

                if (ui == null || ui.Show == false)
                {
                    /// 미리 shortcut 세팅 
                    UIFrameArtifact.ScheduleShortCut_ArtifactManufactureByID(curArtifactTID);
                    UIManager.Instance.Open<UIFrameArtifact>();
                }

                onMoveArtifactUpgrade?.Invoke();
                Close();
            });
    }

    public void OnClickEquipBtn()
    {
        var data = DBArtifact.GetArtifactByID(curArtifactTID);

        if (data == null)
            return;

        switch (curEquipActionState)
        {
            case EquipActionCase.Equip:
                {
                    ZWebManager.Instance.WebGame.REQ_ArtifactEquip(
                        (uint)curPetType
                        , Me.CurCharData.GetMyArtifactIDByTid(curArtifactTID)
                        , (revPacket, resList) =>
                        {
                            Initialize(curArtifactTID, null, false, true);
                        },
                        (err, req, res) =>
                        {
                            gameObject.SetActive(false);
                        });
                }
                break;
            case EquipActionCase.Switch:
                {
                    ZWebManager.Instance.WebGame.REQ_ArtifactUnEquip(
                        (uint)curPetType
                     , (revPacket, resList) =>
                     {
                         ZWebManager.Instance.WebGame.REQ_ArtifactEquip(
                             (uint)curPetType
                             , Me.CurCharData.GetMyArtifactIDByTid(curArtifactTID)
                             , (revPacket_, resList_) =>
                             {
                                 Initialize(curArtifactTID, null, false, true);
                             },
                             (err_, req_, res_) =>
                             {
                                 gameObject.SetActive(false);
                             });
                     },
                     (err, req, res) =>
                     {
                         gameObject.SetActive(false);
                     });
                }
                break;
            case EquipActionCase.Unequip:
                {
                    ZWebManager.Instance.WebGame.REQ_ArtifactUnEquip(
                        (uint)curPetType
                        , (revPacket, resList) =>
                        {
                            Initialize(curArtifactTID, null, false, true);
                        },
                        (err, req, res) =>
                        {
                            gameObject.SetActive(false);
                        });
                }
                break;
        }
    }
    #endregion
    #endregion
}
