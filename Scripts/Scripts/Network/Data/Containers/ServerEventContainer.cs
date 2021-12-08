using System;
using System.Collections.Generic;
using ZNet.Data;

public class ServerEventInfoConvert
{
	public readonly ulong EventId;
	public readonly WebNet.E_ServerEventCategory Category;
	public readonly WebNet.E_ServerEventSubCategory SubCategory;
	public readonly ulong StartDt;
	public readonly ulong EndDt;
	public readonly TinyJSON.Variant Args;

	public readonly string bgUrl; // 이벤트 배경
	public readonly string bgHash; // 이벤트 배경 해쉬값

	public ServerEventInfoConvert(WebNet.ServerEventInfo info)
	{
		EventId = info.EventId;
		Category = info.Category;
		SubCategory = info.SubCategory;
		StartDt = info.StartDt;
		EndDt = info.EndDt;

		bgUrl = info.BgUrl;
		bgHash = info.BgHash;

		if (string.IsNullOrEmpty(info.Args) == false)
		{
			Args = TinyJSON.JSON.Load(info.Args);
		}
	}
}

public class IngameEventInfoConvert : ServerEventInfoConvert
{
	public readonly uint groupId = 0; // 이벤트 고유 번호

	public readonly ulong startTime = 0; // 이벤트 출력시작 시간(초)
	public readonly ulong endTime = 0; // 이벤트 출력종료 시간(초)

	public readonly string title = string.Empty;

	public IngameEventInfoConvert(WebNet.ServerEventInfo info) : base(info)
	{
		if (Args == null)
			return;

		groupId = 0;

		string groupIdStr = Args["group_id"] ?? string.Empty;

		uint.TryParse(groupIdStr, out groupId);

		//groupId = Args["group_id"]?.Make<uint>() ?? 0;

		title = Args["view_message"] ?? string.Empty;

		if (Args["open_time"] != null)
		{
			foreach (TinyJSON.Variant timeInfo in Args["open_time"] as TinyJSON.ProxyArray)
			{
				startTime = timeInfo["start_time"]?.Make<ulong>() ?? 0;
				endTime = timeInfo["end_time"]?.Make<ulong>() ?? 0;
			}
		}
	}
}

public sealed class ServerEventContainer : ContainerBase
{
	private List<ServerEventInfoConvert> eventInfoList = new List<ServerEventInfoConvert>();

	public override void Clear()
	{
		eventInfoList.Clear();
	}

	// REQ_GetServerEventList forceRequest : false 도 가능하지만, 혼란 방지용
	public List<ServerEventInfoConvert> GetServerEventList() => eventInfoList;

	/// <summary> 서버에 이벤트를 리스트를 요청한다 forceRequest가 false 이고 리스트가 있을경우 패킷 요청없이 리턴한다 </summary>
	public void REQ_GetServerEventList(Action<List<ServerEventInfoConvert>> callback, bool forceRequest = true)
	{
		if (eventInfoList.Count > 0 && forceRequest == false)
		{
			callback?.Invoke(eventInfoList);
			return;
		}

		ZWebManager.Instance.WebGame.REQ_GetServerEventList((res, msg) =>
		{
			eventInfoList.Clear();

			for (int i = 0; i < msg.ServerEventsLength; ++i)
			{
				var eventInfo = msg.ServerEvents(i).Value;

				if (eventInfo.Category == WebNet.E_ServerEventCategory.Event)
					eventInfoList.Add(new IngameEventInfoConvert(eventInfo));
				else
					eventInfoList.Add(new ServerEventInfoConvert(eventInfo));
			}

			callback?.Invoke(eventInfoList);
		});
	}
}