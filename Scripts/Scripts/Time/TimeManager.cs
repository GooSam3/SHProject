using UnityEngine;
using Zero;

/// <summary>
/// 공통으로 사용할 전역 시간 관리자
/// </summary>
public class TimeManager : Singleton<TimeManager>
{
	/// <summary>
	/// Milliseconds까지 포함된 UnixTimeStamp값 (ex:1555330269697)
	/// </summary>
	public static ulong NowMs
	{
		get { return curServerUTS + accumUnscaledDeltaMS; }
	}

	/// <summary>
	/// seconds 단위
	/// </summary>
	public static ulong NowSec
	{
		get { return (ulong)(NowMs / (uint)TimeHelper.Unit_SecToMs); }
	}

	/// <summary> 핑 전송 간격 </summary>
	public const float PingInterval = 30f;

	/// <summary> 마지막 갱신된 서버시간보다 늦은시간이 적용되는걸 막기 위한 변수</summary>
	static ulong lastServerUTS;

	static ulong curServerUTS = 0;
	static ulong prevServerUTS = 0;
	static double accumUnscaledDeltaTime = 0;
	static double accumDeltaTime = 0;
	/// <summary> Milliseconds </summary>
	static ulong accumUnscaledDeltaMS = 0;
	/// <summary> Time.Scale변조 감지용 </summary>
	static long accumDeltaMS = 0;

    static long halfRTT = 0;

	protected override void Init()
	{
		base.Init();

		enabled = false;
	}

	/// <summary>
	/// WebServer에서 받은 시간 저장 처리
	/// </summary>
	/// <param name="reqClientMsUTS"></param>
	/// <param name="serverMsUTS"></param>
	public void SetTime(ulong reqClientMsUTS, ulong serverMsUTS)
	{
		long now = (long)TimeManager.NowMs;
		halfRTT = 80;//now > 0 ? (now - (long)reqClientMsUTS) / 2 : 0; //왕복시간

        TimeManager.Instance.SetTime(serverMsUTS);
	}

    public static long GetHalfRTT()
    {
        return halfRTT;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newServerTimestamp">Milliseconds UTS</param>
    private void SetTime(ulong newServerTimestamp)
	{
		if (lastServerUTS > newServerTimestamp)
			return;

		lastServerUTS = newServerTimestamp;

		#region :: 서버시간 기반으로 스핵 감지 코드 ::

		if (curServerUTS != 0f)
			prevServerUTS = curServerUTS;
		else
		{
			prevServerUTS = newServerTimestamp;
			accumUnscaledDeltaTime = 0;
		}

		curServerUTS = newServerTimestamp;

		// 이전 [서버시간] 동기화로 부터 얼마나 지났는지 체크
		double syncElapsedTime = curServerUTS - prevServerUTS;
		// 실제 게임에서 지나간 시간과 차이 계산
		double unscaledDiffTime = accumUnscaledDeltaMS - syncElapsedTime;

		//if (!PlatformSpecific.IsUnityServer)
		//{
		//	// 1초이상 차이가나면 검사하자.
		//	if (unscaledDiffTime > 1000)
		//	{
		//		double speedRate = accumUnscaledDeltaMS / syncElapsedTime;
		//		if (speedRate > 1.3f) // 1.3배이상 빠르다면
		//		{
		//			ZNetGame.ReportHackUser(WebNet.E_HACK_CATEGORY.Speed, $"UserID: {NetData.UserID}, CharID: {NetData.CharID}, SpeedRate: {speedRate}, (curServerUTS - prevServerUTS): {syncElapsedTime}, diffTime: {unscaledDiffTime}, Revision: {ZGameManager.instance.RevisionVer}", (packet, resHackUser) =>
		//			{
		//			});

		//			ZLog.LogWarning($"SystemTimeHack UserID: {NetData.UserID}, CharID: {NetData.CharID}, SpeedRate: {speedRate}, (curServerUTS - prevServerUTS): {syncElapsedTime}, diffTime: {unscaledDiffTime}");
		//		}
		//	}

		//	// TimeScale 변조 검사
		//	bool modifiedTimeScale = CheckTimeScaleModified();
		//	if (modifiedTimeScale)
		//	{
		//		ZNetGame.ReportHackUser(WebNet.E_HACK_CATEGORY.Speed, $"TimeScale : UserID: {NetData.UserID}, CharID: {NetData.CharID}, TimeScale: {Time.timeScale}, Revision: {ZGameManager.instance.RevisionVer}", (packet, resHackUser) =>
		//		{
		//		});
		//		Time.timeScale = 1f; // 복구

		//		ZLog.LogWarning($"TimeScaleHack UserID: {NetData.UserID}, CharID: {NetData.CharID}, TimeScale: {Time.timeScale}");
		//	}
		//}
		#endregion

		accumUnscaledDeltaTime = 0;
		accumUnscaledDeltaMS = 0;

		accumDeltaTime = 0;
		accumDeltaMS = 0;

		enabled = true;
	}

	/// <summary>TimeScale 변조 검사용</summary>
	private bool CheckTimeScaleModified()
	{
		bool modifiedTimeScale = Time.timeScale > 1f;

		return modifiedTimeScale;
	}

	void Update()
	{
		accumUnscaledDeltaTime += Time.unscaledDeltaTime;
		accumDeltaTime += Time.deltaTime;

		accumDeltaMS = (long)(accumDeltaTime * 1000f);
		accumUnscaledDeltaMS = (ulong)(halfRTT + (accumUnscaledDeltaTime * 1000f));
	}
}