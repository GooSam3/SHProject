using System.Collections.Generic;

public class BossWarContainer : ContainerBase
{
	public enum E_Location
	{
		None,
		Camp,
		Field,
	}

	public uint StageTid;
	public ulong EnterableStartTsSec;
	public ulong EnterableEndTsSec;
	public bool IsEnterable;
	public bool IsKill;
	public ulong KillableStartTsSec;
	public ulong KillableEndTsSec;
	public ulong RoomExpireTsSec;
	public ulong SpawnTsSec;
	public List<ulong> SpawnTimeScheduleList;
	public E_Location Location = E_Location.None;
	public float MyPoint;
	public string BossName = string.Empty;

	public BossWarContainer()
	{

	}

	public BossWarContainer(WebNet.ResGetServerBossInfo bossInfo)
	{
		SpawnTimeScheduleList = new List<ulong>();

		StageTid = bossInfo.OpenBoss.Value.StageTid;
		EnterableStartTsSec = bossInfo.OpenBoss.Value.EnterAbleStartTsSec;
		EnterableEndTsSec = bossInfo.OpenBoss.Value.EnterAbleEndTsSec;
		IsEnterable = bossInfo.OpenBoss.Value.IsEnterAble;
		IsKill = bossInfo.OpenBoss.Value.IsKill;
		KillableStartTsSec = bossInfo.OpenBoss.Value.KillAbleStartTsSec;
		KillableEndTsSec = bossInfo.OpenBoss.Value.KillAbleEndTsSec;
		RoomExpireTsSec = bossInfo.OpenBoss.Value.RoomExpireTsSec;
		SpawnTsSec = bossInfo.OpenBoss.Value.SpawnTsSec;

		for (int i = 0; i < bossInfo.OpenBoss.Value.TimeSecListLength; i++)
		{
			SpawnTimeScheduleList.Add(bossInfo.OpenBoss.Value.TimeSecList(i));
		}

		ZLog.Log(ZLogChannel.UI, $"## StageTid:{StageTid}, EnterableStartTsSec:{EnterableStartTsSec}, EnterableEndTsSec:{EnterableEndTsSec}, IsEnterable:{IsEnterable}, IsKill:{IsKill}, KillableStartTsSec:{KillableStartTsSec}, KillableEndTsSec:{KillableEndTsSec}, RoomExpireTsSec:{RoomExpireTsSec}, SpawnTsSec:{SpawnTsSec}");
	}

	public override void Clear()
	{
		
	}
}

public class BossWarPointRanking
{
	public uint Rank;
	public ulong CharId;
	public float Point;
	public uint ServerIdx;

	public BossWarPointRanking(uint rank, ulong charId, float point, uint serverIdx)
	{
		Rank = rank;
		CharId = charId;
		Point = point;
		ServerIdx = serverIdx;
	}
}