using UnityEngine;

/// <summary>
/// 천칭 미니게임 코인 데이터
/// </summary>
public class ZMiniGameLibraCoinData
{
	public int Weight;
}

/// <summary>
/// 천칭 미니게임 코인 스크립트
/// </summary>
public class ZTempleMiniGame_LibraCoin : MonoBehaviour
{
	/// <summary>
	/// 코인데이터
	/// </summary>
	public ZMiniGameLibraCoinData CoinData { get; set; }

	public ZGimmick myGimmick { get; set; }

    public void Start()
	{
		myGimmick = GetComponentInParent<ZGimmick>();
	}

	/// <summary>
	/// 코인 선택
	/// </summary>
	/// <param name="isSelect"></param>
	public void SetSelectEffect(bool isSelect)
	{
		if (isSelect)
			myGimmick.SetAttributeMaterialColor(GameDB.E_UnitAttributeType.Fire);
		else
			myGimmick.SetAttributeMaterialColor(GameDB.E_UnitAttributeType.None);
	}

}

