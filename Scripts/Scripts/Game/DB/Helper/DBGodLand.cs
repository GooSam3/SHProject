using GameDB;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBGodLand : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<uint, GodLand_Table> DicGodLand
	{
		get { return GameDBManager.Container.GodLand_Table_data; }
	}

	public static bool TryGet( uint _tid, out GodLand_Table outTable )
	{
		return GameDBManager.Container.GodLand_Table_data.TryGetValue( _tid, out outTable );
	}

	public static GodLand_Table Get( uint _tid )
	{
		if( GameDBManager.Container.GodLand_Table_data.TryGetValue( _tid, out var foundTable ) ) {
			return foundTable;
		}
		return null;
	}

	public static GodLand_Table GetByGroupID( uint godLandGroupId )
	{
		var it = DicGodLand.GetEnumerator();
		while( it.MoveNext() ) {
			var value = it.Current.Value;
			if( value.SlotGroupID == godLandGroupId ) {
				return value;
			}
		}
		return null;
	}

	/// <summary> groupID 에 해당하는 리스트를 가져온다 </summary>
	public static List<GodLand_Table> GetListByGroupID( uint godLandGroupId )
	{
		List<GodLand_Table> list = new List<GodLand_Table>();
		var it = DicGodLand.GetEnumerator();
		while( it.MoveNext() ) {
			var value = it.Current.Value;
			if( value.SlotGroupID == godLandGroupId ) {
				list.Add( value );
			}
		}
		return list;
	}

	/// <summary> 테이블에서 거점을 대표하는 테이블을 하나씩만 가져온다 </summary>
	public static List<GodLand_Table> GetGroupMainList()
	{
		List<GodLand_Table> list = new List<GodLand_Table>();
		var it = DicGodLand.GetEnumerator();
		while( it.MoveNext() ) {
			var value = it.Current.Value;
			if( list.Exists( v => v.SlotGroupID == value.SlotGroupID ) == false ) {
				list.Add( value );
			}
		}
		return list;
	}
}
