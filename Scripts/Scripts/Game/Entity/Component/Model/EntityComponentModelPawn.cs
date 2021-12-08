using GameDB;
using UnityEngine;

/// <summary> Pawn 모델 관련 처리 </summary>
public class EntityComponentModelPawn : EntityComponentModelBase
{
    protected override void OnSetTable()
    {
        uint tableId = 0;
        switch (Owner.EntityType)
        {
            case E_UnitType.Character:
                {
                    if (DBChange.TryGet(Owner.EntityData.To<ZPawnDataCharacter>().ChangeTid, out var changeTable))
                    {
                        tableId = changeTable.ResourceID;
                        ModelScaleFactor = changeTable.Scale * 0.01f;
                    }
                    else if (DBCharacter.TryGet(Owner.TableId, out Character_Table table))
                    {
                        tableId = table.ResourceID;
                        ModelScaleFactor = table.Scale * 0.01f;
                    }
                }
                break;
            case E_UnitType.Monster:
            case E_UnitType.Summon:
                {
                    if (DBMonster.TryGet(Owner.TableId, out Monster_Table table))
                    {
                        tableId = table.ResourceID;
                        ModelScaleFactor = table.Scale * 0.01f;
                    }
                }
                break;
            case E_UnitType.NPC:
                {
                    if (DBNpc.TryGet(Owner.TableId, out NPC_Table table))
                    {
                        tableId = table.ResourceID;
                        ModelScaleFactor = table.Scale * 0.01f;
                    }
                }
                break;
            case E_UnitType.Pet:
                {
                    if (DBPet.TryGet(Owner.TableId, out Pet_Table table))
                    {
                        tableId = 0;// table.ResourceID;
                        mAssetName = table.ResourceFile;
                        ModelScaleFactor = table.Scale * 0.01f;
                    }
                }
                break;
            case E_UnitType.Object:
                {
                    if(DBObject.TryGet(Owner.TableId, out Object_Table table))
                    {
                        tableId = 0;// table.ResourceID;
                        mAssetName = table.ResourceName;
                        ModelScaleFactor = 1f;//table.Scale * 0.01f;
                    }
                }
                break;
            default:
                {
                    ModelScaleFactor = 1f;
                }
                break;
        }

        if(0 < tableId)
        {
            ResourceTable = DBResource.Get(tableId);
        }
        else
        {
            ResourceTable = null;
        }
    }

    protected override void OnPostSetModel()
    {
        if (null != ResourceTable)
        {
            Vector3 colSize = new Vector3(ResourceTable.SizeX, ResourceTable.SizeY, ResourceTable.SizeZ);
            Owner.SetCollider(colSize * ModelScaleFactor);
            ModelGo.transform.localScale = Vector3.one * ModelScaleFactor;
        }

        // TODO : 강제 조절중.
        if (null != mLODGroup && Owner.EntityType == E_UnitType.Character)
            mLODGroup?.ForceLOD(0);
    }
}
