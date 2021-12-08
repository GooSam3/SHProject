using GameDB;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBColosseum : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static Dictionary<byte, Colosseum_Table> DicColosseum
	{
		get { return GameDBManager.Container.Colosseum_Table_data; }
	}

	public static bool TryGet( uint _tid, out Colosseum_Table outTable )
	{
		return GameDBManager.Container.Colosseum_Table_data.TryGetValue( ( byte )_tid, out outTable );
	}

	public static Colosseum_Table Get( uint _tid )
	{
		if( GameDBManager.Container.Colosseum_Table_data.TryGetValue( ( byte )_tid, out var foundTable ) ) {
			return foundTable;
		}
		return null;
	}

	public static Colosseum_Table FindByColosseumPoint( uint _score ) { 
		List<Colosseum_Table> list = DicColosseum.Values.ToList();
		list.Sort( ( a, b ) => {
			return a.Grade.CompareTo( b.Grade );
		} );

		for( int i = 0; i < list.Count; ++i ) {
			if( list[i].ColosseumPoint <= _score ) {

				if( i + 1 == list.Count ) {
					return list[ i ];
				}
				else {
					if( list[ i + 1 ].ColosseumPoint > _score ) {
						return list[ i ];
					}
				}
			}
		}
		return null;
	}

	/// <summary> 콜로세움 시즌 테이블 </summary>
	public static ColoSeasonReward_Table GetSeasenReward( uint _tid )
	{
		if( GameDBManager.Container.ColoSeasonReward_Table_data.TryGetValue( _tid, out var foundTable ) ) {
			return foundTable;
		}
		return null;
	}
}
