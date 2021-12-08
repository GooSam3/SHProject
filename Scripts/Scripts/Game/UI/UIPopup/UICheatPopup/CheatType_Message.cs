using UnityEngine;
using UnityEngine.UI;

// 치트메세지는 마지막탭으로 위치하는게 이쁠것같습니다
public class CheatType_Message : CheatPanel
{
    private const char SEPARATOR = ' ';

    [SerializeField] private InputField messageInput;

    public override void Initialize()
    {
        messageInput.text = string.Empty;
    }
    public override void SetActive(bool state)
    {
        if (!state)
            messageInput.text = string.Empty;
        
        base.SetActive(state);
    }

    public void OnClickMessageSend()
    {
        string origin = messageInput.text;

        string[] msg = origin.Split(SEPARATOR);

        if (msg.Length<=1)
        {
            ZLog.Log(ZLogChannel.Default, "파라미터가 충분하지 않습니다!!!!");
            return;
        }

        if(msg[0].Equals("/framerate"))
		{
            if(int.TryParse(msg[1], out int framerate))
			{
                Application.targetFrameRate = framerate;
            }
            return;
		}

        ZLog.Log(ZLogChannel.Default, $"SEND_CHEAT_MESSAGE_{origin}");
        ZMmoManager.Instance.Field.REQ_MapChat(ZPawnManager.Instance.MyEntityId, origin);
    }

}
