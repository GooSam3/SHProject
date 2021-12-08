using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TinyJSON;

/// <summary>
/// [wisreal][2020.11.16]
/// 패킷 개별 설정값 저장 및 관리
/// 1.네트워크를 통해서 설정 값을 가져온다.
/// 2.패킷 사용 시 설정값이 있는지 체크하고 있으면 설정값을 적용 한다.
/// </summary>

public class ZNetPacketConfig
{
    [UnityEngine.Scripting.Preserve]
    public class PacketInfo
    {
        public WebNet.Code PacketCode;
        public float PacketCoolTime;
        public byte IsIndicatorMsg;
        public byte IsUniqueMsg;
    }

    private static Dictionary<WebNet.Code, PacketInfo> packetList = new Dictionary<WebNet.Code, PacketInfo>();

    static float LastCheckTime;
    public static float CheckTime = 3600f;

    public static void CheckLastestInfo()
    {
        if (LastCheckTime == 0 || Time.realtimeSinceStartup - LastCheckTime > CheckTime)
        {
            LastCheckTime = Time.realtimeSinceStartup;

            ZLog.Log( ZLogChannel.WebSocket , "PacketConfig List Download Start");

            ZWebManager.Instance.DownloadFile(Auth.AssetUrlShortcut.PacketInfoUrl + "packetlist.json", LoadDoneFile);

            TimeInvoker.Instance.Invoke("CheckLastestInfo", CheckTime);
        }
    }

    public static bool IsIndicatorPacket(WebNet.Code packetCode)
    {
        if (packetList.TryGetValue(packetCode, out var packetInfo))
            return packetInfo.IsIndicatorMsg == 1;

        return true;
    }

    public static bool IsUniquePacket(WebNet.Code packetCode)
    {
        if (packetList.TryGetValue(packetCode, out var packetInfo))
            return packetInfo.IsUniqueMsg == 1;

        return true;
    }

    public static float GetPacketCoolTime(WebNet.Code packetCode)
    {
        if (packetList.TryGetValue(packetCode, out var packetInfo))
            return packetInfo.PacketCoolTime;

        return 0;
    }

    static void LoadDoneFile(DownloadHandler handler)
    {
        ZLog.Log( ZLogChannel.WebSocket, "Download PacketConfig : " + handler.text);

        var loadFileJson = JSON.Load(handler.text);

        packetList.Clear();

        foreach (var obj in loadFileJson as ProxyArray)
        {
            WebNet.Code code = (WebNet.Code)obj["packet"].Make<uint>();

            if (!packetList.ContainsKey(code))
                packetList.Add(code, new PacketInfo() { PacketCode = code, PacketCoolTime = obj["cooltime"].Make<uint>(), IsIndicatorMsg = obj["bindicator"].Make<byte>() , IsUniqueMsg = obj["is_unique_msg"].Make<byte>()});
        }

        ZLog.Log(ZLogChannel.WebSocket, "PacketConfig load done : " + packetList.Count);
    }
}
