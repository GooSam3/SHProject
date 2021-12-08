using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class CheatType_Character : CheatPanel
{
    [SerializeField] private InputField levelInput;

    [SerializeField] private Text myLevel;
    [SerializeField] private Text lastLevel;

    public override void Initialize()
    {
    }

    public override void SetActive(bool state)
    {
        if(state)
        {
            try
            {
                myLevel.text = $"현재 레벨 : <color=cyan>{Me.CurCharData.Level.ToString()}</color>";
                lastLevel.text = $"마지막 레벨 : <color=orange>{Me.CurCharData.Level.ToString()}</color>";
            }
            catch // UI 테스트씬
            {
                myLevel.text = $"현재 레벨 :<color=green>---</color>";
                lastLevel.text = $"마지막 레벨 :<color=orange>---</color>";
            }
        }

        base.SetActive(state);
    }

    public void ReqLevelUp()
    {
        if (uint.TryParse(levelInput.text, out uint inputLv) == false)
        {
            Debug.LogError("옳바른 값을 입력해주십쇼");
            return;
        }

        if (Me.CurCharData.Level == inputLv)
        {
            Debug.LogError("현재 레벨과 입력 레벨이 같습니다.");
        }

		if (0 == ZPawnManager.Instance.MyEntityId)
		{
			Debug.LogError("MyEntity가 유효하지 않습니다.");
		}

		string cheatMsg = $"/level {inputLv}";

		ZMmoManager.Instance.Field.REQ_MapChat(ZPawnManager.Instance.MyEntityId, cheatMsg);
    }
}