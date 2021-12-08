using GameDB;

/// <summary>
/// 로비용 모델 처리
/// </summary>
public class EntityComponentModelLobby : EntityComponentModelPawn
{
	protected override void OnSetTable()
	{
		uint tableId = 0;
		switch (Owner.EntityType)
		{
			case E_UnitType.Character:
				{
					ZPawnDataCharacter pawnData = Owner.EntityData.To<ZPawnDataCharacter>();

					if (pawnData == null || pawnData.ChangeTid == 0)
					{
						if (DBCharacter.TryGet(Owner.TableId, out Character_Table table))
						{
							tableId = table.ResourceID;
							this.ModelScaleFactor = table.Scale * 0.01f;
						}
					}
					else
					{
						if (DBChange.TryGet(pawnData.ChangeTid, out Change_Table table))
                        {
							tableId = table.ResourceID;
							this.ModelScaleFactor = table.SeletScale * 0.01f;
						}
					}
				}
				break;
			case E_UnitType.Monster:
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

			default:
				{
					ModelScaleFactor = 1f;
				}
				break;
		}

		ResourceTable = DBResource.Get(tableId);
	}

	protected override void SetAssetName()
	{
		base.SetAssetName();

		mAssetName = ResourceTable.LobbyFile ?? ResourceTable.ResourceFile;
	}

    protected override void OnPostSetModel()
    {
        base.OnPostSetModel();

		//레이어 변경
		Owner.gameObject.SetLayersRecursively(UnityConstants.Layers.Player);
	}
}
