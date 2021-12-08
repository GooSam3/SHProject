using GameDB;
using UnityEngine;

/// <summary> 맵 밖으로 벗어나는 오브젝트 관련 처리 </summary>
public class ZGimmickComp_OutOfMapTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<EntityBase>();

        if (null == entity)
        {
            return;
        }   

        switch(entity.EntityType)
        {
            case E_UnitType.Gimmick:
                {
                    GameObject.Destroy(entity.gameObject);
                }
                break;
            default:
                {
                    //내 pc면 저장된 위치로 이동.
                    if(entity.IsMyPc)
                    {
                        ZTempleHelper.PlayPresetAction(E_TemplePresetAction.WarpCheckPoint);
                    }
                    else
                    {
                        //Kill
                    }
                }
                break;
        }
    }
}