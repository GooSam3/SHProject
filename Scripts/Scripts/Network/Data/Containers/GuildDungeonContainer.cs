using System.Collections.Generic;

public class GuildDungeonContainer : ContainerBase
{
	public List<MyGuildDungeonInfo> GuildDungeonInfoList = new List<MyGuildDungeonInfo>();
	public ulong LastOpenDt;
	public uint OpenCount;
	public ulong RemainTime;
	public ulong EndTime;
	public uint BossMonsterKillCount;
	public uint BossMonsterSpawnCount;
	public string CurrentBossMonsterName;

	public override void Clear()
	{
		
	}

	public MyGuildDungeonInfo GetGuildDungeonInfo(uint stageTid)
	{
		MyGuildDungeonInfo info = GuildDungeonInfoList.Find(a => a.StageTid == stageTid);

		return info;
	}

	public void AddGuildDungeonInfo(WebNet.GuildDungeonInfo? info)
	{
		MyGuildDungeonInfo dungeonInfo = GetGuildDungeonInfo(info.Value.StageTid);

		ZLog.Log(ZLogChannel.UI, $"## StageTid {info.Value.StageTid}, OpenGuildId {info.Value.OpenGuildId}, OpenTsSec {info.Value.OpenTsSec}, BossKillTsSec {info.Value.BossKillTsSec}, Addr {info.Value.Addr}, RoomNo {info.Value.RoomNo}, Status {info.Value.Status}");

		if (dungeonInfo == null)
		{
			GuildDungeonInfoList.Add(new MyGuildDungeonInfo(info));
		}
		else
		{
			dungeonInfo.Reset(info);
		}
	}
}

public class MyGuildDungeonInfo
{
	public uint StageTid;
	public ulong OpenGuildId;
	public ulong OpenTsSec;
	public ulong BossKillTsSec;
	public string Addr;
	public ulong RoomNo;
	public WebNet.E_GuildDungeonStatus Status;

	public MyGuildDungeonInfo(WebNet.GuildDungeonInfo? info)
	{
		StageTid = info.Value.StageTid;
		OpenGuildId = info.Value.OpenGuildId;
		OpenTsSec = info.Value.OpenTsSec;
		BossKillTsSec = info.Value.BossKillTsSec;
		Addr = info.Value.Addr;
		RoomNo = info.Value.RoomNo;
		Status = info.Value.Status;
	}

	public void Reset(WebNet.GuildDungeonInfo? info)
	{
		StageTid = info.Value.StageTid;
		OpenGuildId = info.Value.OpenGuildId;
		OpenTsSec = info.Value.OpenTsSec;
		BossKillTsSec = info.Value.BossKillTsSec;
		Addr = info.Value.Addr;
		RoomNo = info.Value.RoomNo;
		Status = info.Value.Status;
	}
}

public class GuildDungeonClearReward
{
	public uint ItemTid;
	public uint Cnt;

	public GuildDungeonClearReward(uint itemTid, uint cnt)
	{
		ItemTid = itemTid;
		Cnt = cnt;
	}
}