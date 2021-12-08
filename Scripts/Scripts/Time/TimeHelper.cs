using System;

/// <summary>
/// UTS : Unix-TimeStamp
///		ex) 1553495865
///	DateTime
///		ex) 03/25/2019 @ 6:37am (UTC)
/// </summary>
/// <remarks>
/// </remarks>
public static class TimeHelper
{
	/// <summary>
	/// Get unix time (ms)
	/// </summary>
	/// <returns>unix time (ms)</returns>
	public static ulong LocalNow
	{
		get
		{
			return (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}
	}

	static DateTime OffsetDateTime;
	static uint m_SecondOffset;
	public static uint SecondOffset
	{
		set
		{
			m_SecondOffset = value;
			OffsetDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			OffsetDateTime = OffsetDateTime.AddSeconds(m_SecondOffset);
		}
		get
		{
			return m_SecondOffset;
		}
	}

	/// <summary>
	/// Convert unix time (ms) to DateTime object
	/// </summary>
	/// <param name="msUTS">unix time (ms)</param>
	/// <returns>Converted DateTime object</returns>
	public static DateTime Time2DateTimeMs(ulong msUTS)
	{
		return OffsetDateTime.AddMilliseconds(msUTS);
	}

	public static DateTime Time2DateTimeSec(ulong secUTS)
	{
		return OffsetDateTime.AddSeconds(secUTS);
	}

	public static long DateTime2TimeMs(DateTime dateTime)
	{
		return (long)(dateTime - OffsetDateTime).TotalMilliseconds;
	}

	public static long DateTime2TimeSec(DateTime dateTime)
	{
		return (long)(dateTime - OffsetDateTime).TotalSeconds;
	}

	/// <summary> 1초 -> 1000로 변환 </summary>
	public const float Unit_SecToMs = 1000f;
	/// <summary> 1000ms -> 1초로 변환 </summary>
	public const float Unit_MsToSec = 0.001f;

	public const ulong DaySecond = 86400;
	public const ulong HourSecond = 3600;
	public const ulong MinuteSecond = 60;

	//현재 시간과 비교 값
	public static string CompareNow(ulong CheckSecondTime, string Prefix = "", string Postfix = "", ulong minSecond = 0, ulong maxSecond = ulong.MaxValue, string minStr = "", string maxStr = "")
	{
		string NewMsg = "";
		ulong compareSecond = 0;

		if (CheckSecondTime > TimeManager.NowSec)
		{
			compareSecond = CheckSecondTime - TimeManager.NowSec;
		}
		else
		{
			compareSecond = TimeManager.NowSec - CheckSecondTime;
		}

		if (compareSecond < minSecond)
			return minStr;
		else if (compareSecond > maxSecond)
			return maxStr;

		if (compareSecond > DaySecond)
			NewMsg = string.Format("{0}{1}일{2}", Prefix, (compareSecond / DaySecond), Postfix);
		else if (compareSecond > HourSecond)
			NewMsg = string.Format("{0}{1}시간{2}", Prefix, (compareSecond / HourSecond), Postfix);
		else if (compareSecond > MinuteSecond)
			NewMsg = string.Format("{0}{1}분{2}", Prefix, (compareSecond / MinuteSecond), Postfix);
		else if (compareSecond > 0)
			NewMsg = string.Format("{0}{1}초{2}", Prefix, compareSecond, Postfix);

		return NewMsg;
	}

	public static TimeSpan GetCompareTimeSpan(ulong checkSec, ulong now = 0)
	{
		if (now == 0)
			now = TimeManager.NowSec;

		var remainTime = (long)checkSec - (long)now;

		if (remainTime <= 0)
			remainTime = 0;

		return TimeSpan.FromSeconds(remainTime);
	}

	public static DateTime ParseTimeStamp(long timestamp)
	{
		return OffsetDateTime.AddSeconds(timestamp);
	}

	public static string GetTimeStrSimple(DateTime TargetTime)
	{
		return string.Format("{0:0000}.{1:00}.{2:00}", TargetTime.Year, TargetTime.Month, TargetTime.Day);
	}

	public static string GetTimeStr(DateTime TargetTime)
	{
		return string.Format("{0:0000}.{1:00}.{2:00} {3:00}:{4:00}", TargetTime.Year, TargetTime.Month, TargetTime.Day, TargetTime.Hour, TargetTime.Minute);
	}

	public static string GetRemainTime(DateTime TargetTime)
	{
		string NewMsg = "";
		if ((TargetTime - DateTime.Now).TotalDays > 0)
			NewMsg = (TargetTime - DateTime.Now).TotalDays + " 일";
		else if ((TargetTime - DateTime.Now).TotalHours > 0)
			NewMsg = (TargetTime - DateTime.Now).TotalHours + " 시간";
		else if ((TargetTime - DateTime.Now).TotalMinutes > 0)
			NewMsg = (TargetTime - DateTime.Now).TotalMinutes + " 분";
		else if ((TargetTime - DateTime.Now).TotalSeconds > 0)
			NewMsg = (TargetTime - DateTime.Now).TotalSeconds + " 초";

		return NewMsg;
	}

	public static string GetRemainTime(ulong TargetTime, string Prefix = "", string Postfix = "")
	{
		string NewMsg = "";

		if (TargetTime > (DaySecond - HourSecond))
			NewMsg = string.Format("{0}{1}일{2}", Prefix, (TargetTime / DaySecond) + ((TargetTime % DaySecond > 0) ? (ulong)1 : 0), Postfix);
		else if (TargetTime > (HourSecond - MinuteSecond))
			NewMsg = string.Format("{0}{1}시간{2}", Prefix, (TargetTime / HourSecond) + ((TargetTime % HourSecond > 0) ? (ulong)1 : 0), Postfix);
		else if (TargetTime > MinuteSecond)
			NewMsg = string.Format("{0}{1}분{2}", Prefix, (TargetTime / MinuteSecond) + ((TargetTime % MinuteSecond > 0) ? (ulong)1 : 0), Postfix);
		else if (TargetTime > 0)
			NewMsg = string.Format("{0}{1}초{2}", Prefix, TargetTime, Postfix);

		return NewMsg;
	}

	public static string GetRemainFullTimeHour(ulong TargetTime, string Prefix = "", string Postfix = "")
	{
		string NewMsg = "";

		if (TargetTime > DaySecond)
		{
			NewMsg = string.Format("{0}일", TargetTime / DaySecond);
			TargetTime -= (TargetTime / DaySecond) * DaySecond;
		}

		if (TargetTime > HourSecond)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}시간", NewMsg, (TargetTime / HourSecond));
			else
				NewMsg = string.Format("{0}시간", (TargetTime / HourSecond));
			TargetTime -= (TargetTime / HourSecond) * HourSecond;
		}

		return string.Format("{0}{1}{2}", NewMsg, Prefix, Postfix);
	}

	public static string GetRemainFullTimeMin(ulong TargetTime, string Prefix = "", string Postfix = "")
	{
		string NewMsg = "";

		if (TargetTime > DaySecond)
		{
			NewMsg = string.Format("{0}일", TargetTime / DaySecond);
			TargetTime -= (TargetTime / DaySecond) * DaySecond;
		}

		if (TargetTime > HourSecond)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}시간", NewMsg, (TargetTime / HourSecond));
			else
				NewMsg = string.Format("{0}시간", (TargetTime / HourSecond));
			TargetTime -= (TargetTime / HourSecond) * HourSecond;
		}

		if (TargetTime > MinuteSecond)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}분", NewMsg, (TargetTime / MinuteSecond));
			else
				NewMsg = string.Format("{0}분", (TargetTime / MinuteSecond));
			TargetTime -= (TargetTime / MinuteSecond) * MinuteSecond;
		}

		return string.Format("{0}{1}{2}", NewMsg, Prefix, Postfix);
	}

	public static string GetRemainFullTime(ulong TargetTime, string Prefix = "", string Postfix = "")
	{
		string NewMsg = "";

		if (TargetTime > DaySecond)
		{
			NewMsg = string.Format("{0}일", TargetTime / DaySecond);
			TargetTime -= (TargetTime / DaySecond) * DaySecond;
		}

		if (TargetTime > HourSecond)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}시간", NewMsg, (TargetTime / HourSecond));
			else
				NewMsg = string.Format("{0}시간", (TargetTime / HourSecond));
			TargetTime -= (TargetTime / HourSecond) * HourSecond;
		}

		if (TargetTime > MinuteSecond)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}분", NewMsg, (TargetTime / MinuteSecond));
			else
				NewMsg = string.Format("{0}분", (TargetTime / MinuteSecond));
			TargetTime -= (TargetTime / MinuteSecond) * MinuteSecond;
		}

		if (TargetTime >= 0)
		{
			if (!string.IsNullOrEmpty(NewMsg))
				NewMsg = string.Format("{0} {1}초", NewMsg, TargetTime);
			else
				NewMsg = string.Format("{0}초", TargetTime);
			TargetTime -= (TargetTime / MinuteSecond) * MinuteSecond;
		}

		return string.Format("{0}{1}{2}", NewMsg, Prefix, Postfix);
	}

	//특정 시간 까지 남은 시간 ex)오전 5시 30, => hour : 5 , minute : 30 현재로 부터 오전 5시30분 까지 남은 시간 계산(초)
	public static ulong GetRemainSpecificTime(uint Hour, uint Minute = 0, uint Second = 0)
	{
		DateTime checkTime = Time2DateTimeMs(TimeManager.NowMs);

		checkTime = checkTime.AddSeconds((-checkTime.Hour * (long)HourSecond) + (-checkTime.Minute * (long)MinuteSecond) + (-checkTime.Second)).AddSeconds(Hour * HourSecond + Minute * MinuteSecond + Second);
		ulong checkTimeSec = (ulong)DateTime2TimeSec(checkTime);
		if (checkTimeSec < TimeManager.NowSec)
		{
			//check next day
			checkTimeSec = (ulong)DateTime2TimeSec(checkTime.AddDays(1));
		}

		return checkTimeSec - TimeManager.NowSec;
	}

	//특정 요일 시간 까지 남은 시간 ex)수요일 오전 5시 30, => 다음 수요일 hour : 5 , minute : 30 현재로 부터 오전 5시30분 까지 남은 시간 계산(초)
	public static ulong GetRemainSpecificTime(DayOfWeek day, uint Hour, uint Minute = 0, uint Second = 0)
	{
		DateTime checkTime = Time2DateTimeMs(TimeManager.NowMs);
		DateTime nowTime = Time2DateTimeMs(TimeManager.NowMs);

		while (checkTime.DayOfWeek != day || checkTime <= nowTime)
		{
			checkTime = checkTime.AddDays(1);
		}

		checkTime = checkTime.AddSeconds((-checkTime.Hour * (long)HourSecond) + (-checkTime.Minute * (long)MinuteSecond) + (-checkTime.Second)).AddSeconds(Hour * HourSecond + Minute * MinuteSecond + Second);

		return (ulong)(checkTime - nowTime).TotalSeconds;
	}

	//특정 요일 시간 까지 남은 시간 ex)수요일 오전 5시 30, => 다음 수요일 hour : 5 , minute : 30 현재로 부터 오전 5시30분 까지 남은 시간 계산(초)
	public static ulong GetRemainSpecificTimeDateTime(DayOfWeek day, uint Hour, uint Minute = 0, uint Second = 0)
	{
		DateTime checkTime = DateTime.Now;
		DateTime nowTime = DateTime.Now;

		while (checkTime.DayOfWeek != day || checkTime <= nowTime)
		{
			checkTime = checkTime.AddDays(1);
		}

		checkTime = checkTime.AddSeconds((-checkTime.Hour * (long)HourSecond) + (-checkTime.Minute * (long)MinuteSecond) + (-checkTime.Second)).AddSeconds(Hour * HourSecond + Minute * MinuteSecond + Second);

		return (ulong)(checkTime - nowTime).TotalSeconds;
	}

	/*
      * 계산방식 : 
      * 지금 시간 - 오늘 출석 기준 시간 ( 새벽 5시 , 오늘 새벽 5시 또는 이전날의 새벽 5시가 될수있음 )
      * 지금 시간 - 해당 유저의 마지막 로그인 시간을 체크해서 
      * 전자가 더 크다면 해당 유저는 출석한거임 . 
  * */
	public static bool IsGivenDtToday(ulong logoutDt, int hourSinceCheck)
	{
		var today = DateTime.Today;

		var timeLastLogoutSince = new DateTime(today.Year, today.Month, today.Day, hourSinceCheck, 0, 0);
		double secondsSinceCheckTimeTilNow = 0;

		// 만약 지금 Hour 이 체킹 기준 hour 보다 낮다면 이전날의 시간으로 체킹해야함 . 
		if (DateTime.Now.Hour < hourSinceCheck)
		{
			timeLastLogoutSince = timeLastLogoutSince.Subtract(TimeSpan.FromDays(1));
		}

		// 마지막 새벽 5 시로부터 지금까지의 시간 
		secondsSinceCheckTimeTilNow = (DateTime.Now - timeLastLogoutSince).TotalSeconds;

		return TimeManager.NowSec - logoutDt < secondsSinceCheckTimeTilNow;
	}

	/// <summary>
	/// "오늘" 리셋시간을 가져옴 => 오늘새벽5시 ~ 어제 새벽5시(12시 이후)
	/// </summary>
	/// <returns></returns>
	public static ulong GetTodayResetTime()
	{
		var today = DateTime.Today;

		var timeLastLogoutSince = new DateTime(today.Year, today.Month, today.Day, 5, 0, 0);

		double secondsSinceCheckTimeTilNow = 0;

		// 만약 지금 Hour 이 체킹 기준 hour 보다 낮다면 이전날의 시간으로 체킹해야함 . 
		if (DateTime.Now.Hour < 5)
		{
			timeLastLogoutSince = timeLastLogoutSince.Subtract(TimeSpan.FromDays(1));
		}
		secondsSinceCheckTimeTilNow = (DateTime.Now - timeLastLogoutSince).TotalSeconds;

		return TimeManager.NowSec - (ulong)secondsSinceCheckTimeTilNow;
	}

	// "오늘" 00:00 반환
	public static ulong GetTodayStartTime()
	{
		var today = DateTime.Today;
		var startDt = new DateTime(today.Year, today.Month, today.Day, 5, 0, 0);

		return TimeManager.NowSec - (ulong)((DateTime.Now - startDt).TotalSeconds);
	}
}
