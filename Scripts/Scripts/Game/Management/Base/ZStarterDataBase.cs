/// <summary>
/// 우리 게임용 기본 시작 데이터
/// </summary>
/// <remarks>
/// <see cref="GameStarterDataBase<>"/>는 Generic이라 Inspector에 노출이 안돼서, wrap class 추가
/// </remarks>
public abstract class ZStarterDataBase : GameStarterDataBase<ZGameManager>
{
	/// <summary> 로고 연출 넘어가기 옵션 </summary>
	public bool SkipLogo = false;

	protected override void OnStart()
	{
	}
}
