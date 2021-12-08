using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowTarget : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private RectTransform RT;
    [SerializeField] private Text Name;
    [SerializeField] private Image Icon;
    #endregion

    #region System Variable
    private GameDB.E_UnitType Type;
    [SerializeField, ReadOnly]
    private Transform Target;
    [SerializeField] private List<GameObject> m_listObjFocus = new List<GameObject>();
    private EntityBase OwnerEntity;
    #endregion

    public void Init(Transform _target)
    {
        Target = _target;

        RT.SetParent(UIManager.Instance.Find<UIFrameHUD>().gameObject.transform, false);
        RT.localScale = Vector2.one;

        SetFocus(false);

        CameraManager.Instance.DoAddEventCameraUpdated(UpdateTargetUI);
    }

    private void OnDestroy()
    {
        if (CameraManager.hasInstance)
        {
            CameraManager.Instance.DoRemoveEventCameraUpdated(UpdateTargetUI);
        }

        RemoveEvent(OwnerEntity);
    }

    public void UpdateTargetUI()
    {
        if (Target != null)
        {
            RectTransform uiManagerRect = UIManager.Instance.GetComponent<RectTransform>();
            // TODO : Camera.main  property라서 접급시마다, 카메라 검색해서 알려주기 때문에, Caching한 카메라 사용하던지 해야함!
            Vector3 ViewportPosition = Camera.main.WorldToViewportPoint(Target.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            (ViewportPosition.x * uiManagerRect.sizeDelta.x) - uiManagerRect.sizeDelta.x / 2,
            (ViewportPosition.y * uiManagerRect.sizeDelta.y) - uiManagerRect.sizeDelta.y / 2);

            if (RT != null)
            {
                if (ViewportPosition.z < 0)
                    RT.anchoredPosition = new Vector2(100000, 100000);
                else
                    RT.anchoredPosition = WorldObject_ScreenPosition;
            }
        }
        else
        {
            // TODO : 일단 다른데서도 파괴로 없애니까.
            Destroy(gameObject);
        }
    }

    public void SetFocus(bool _focus)
    {
        foreach (GameObject go in m_listObjFocus)
        {
            if (go != null)
            {
                go.SetActive(_focus);
            }
        }
    }

    public void SetData(EntityBase entity)
    {
        SetOwnerPawn(entity);

        Name.text = entity.EntityData.Name;        

        switch (Type)
        {
            case GameDB.E_UnitType.Character:
                {
                    UIManager.Instance.Find<UIFrameHUD>().PlayerTargetUI = this;
                    UpdateCharacterNameColor();
                }
                break;
            case GameDB.E_UnitType.Monster:
                {
                    
                }
                break;
            case GameDB.E_UnitType.NPC:
                {
                    if (null != Icon && DBNpc.TryGet(entity.EntityData.TableId, out var table))
                    {
                        bool isValid = !string.IsNullOrEmpty(table.Icon);
                        if (isValid)
                        {
                            Icon.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
                        }

                        Icon.gameObject.SetActive(isValid);
                    }
                }
                break;
        }
    }

    private void SetOwnerPawn(EntityBase owner)
    {
        RemoveEvent(OwnerEntity);

        OwnerEntity = owner;
        Type = owner.EntityData.EntityType;

        AddEvent(owner);
    }

    /// <summary> 이벤트 등록 </summary>    
    private void AddEvent(EntityBase entity)
    {
        if (null != entity && entity is ZPawn pawn)
        {
            switch (Type)
            {
                case E_UnitType.Character:
                    {
                        //pk 상태 체크
                        pawn.DoAddEventUpdateCustomConditionControl(UpdateConditionControl);
                        //성향 체크
                        pawn.DoAddEventChangeTendency(UpdateTendency);
                    }
                    break;
                case E_UnitType.Monster:
                    {
                        //TODO :: 퀘스트 관련 처리 필요.
                    }
                    break;
                case E_UnitType.NPC:
                    {
                        //TODO :: 퀘스트 관련 처리 필요.
                    }
                    break;
            }            
        }
    }

    /// <summary> 등록된 이벤트 제거 </summary>
    /// <param name="entity"></param>
    private void RemoveEvent(EntityBase entity)
    {
        if (null != entity && entity is ZPawn pawn)
        {
            switch (Type)
            {
                case E_UnitType.Character:
                    {
                        pawn.DoRemoveEventUpdateCustomConditionControl(UpdateConditionControl);
                        pawn.DoRemoveEventChangeTendency(UpdateTendency);
                    }
                    break;
                case E_UnitType.Monster:
                    {
                        //TODO :: 퀘스트 관련 처리 필요.
                    }
                    break;
                case E_UnitType.NPC:
                    {
                        //TODO :: 퀘스트 관련 처리 필요.
                    }
                    break;
            }            
        }
    }

    /// <summary> 캐릭터의 상태가 변경될 경우 처리 </summary>
    private void UpdateConditionControl(E_CustomConditionControl control, bool _bApply)
    {
        if(control.HasFlag(E_CustomConditionControl.Pk))
            UpdateCharacterNameColor();
    }

    /// <summary> 캐릭터 성향이 변경될 경우 처리 </summary>
    private void UpdateTendency(int value)
    {
        UpdateCharacterNameColor();
    }

    /// <summary> 캐릭터 이름 색상 변경 </summary>
    private void UpdateCharacterNameColor()
    {
        if (Type != E_UnitType.Character)
        {
            return;
        }

        if(OwnerEntity is ZPawn pawn)
        {
            //Pk 상태면 처리
            if(pawn.IsCustomConditionControl(E_CustomConditionControl.Pk))
            {
                Name.color = ResourceSetManager.Instance.SettingRes.Palette.PkNameColor;                
            }
            else
            {
                int tendency = pawn.Tendency;
                if (tendency > 0)
                {
                    float tendencyRatio = tendency / (float)DBConfig.MaxGoodTendency;
                    Name.color = Color.Lerp(
                        ResourceSetManager.Instance.SettingRes.Palette.NormalTendencyNameColor,
                        ResourceSetManager.Instance.SettingRes.Palette.GoodTendencyNameColor,
                        tendencyRatio);
                }
                else
                {
                    float tendencyRatio = tendency / (float)DBConfig.MaxEvilTendency;
                    Name.color = Color.Lerp(
                        ResourceSetManager.Instance.SettingRes.Palette.NormalTendencyNameColor, 
                        ResourceSetManager.Instance.SettingRes.Palette.EvilTendencyNameColor,
                        tendencyRatio);
                }
            }            
        }
        else
        {
            //TODO :: 성향에 따른 처리
            Name.color = Color.white;
        }
    }
}