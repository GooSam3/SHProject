using System;
using UnityEngine;
using UnityEngine.UI;
using WebNet;

public class UILoginServerListItem : MonoBehaviour
{
    #region Variable
    [SerializeField] private GameObject Alram = null;

    [SerializeField] private Text ServerName = null;
    [SerializeField] private Text ServerState = null;
    [SerializeField] private Text ServerAlram = null;

    [SerializeField] private Action SelectCallback = null;

    public UILoginEnum.E_ServerState ServerStateType { get; private set; } = UILoginEnum.E_ServerState.None;
    #endregion

    /// <summary> 슬롯에 서버 리스트 정보 설정 </summary>
    /// <param name = "_name">  서버 이름 </param>
    /// <param name = "_state"> 서버 상태 </param>
    /// <param name = "_new">   서버 신규 여부 </param>
    /// <param name = "_cb">    서버 선택 시 실행 콜백 </param>
    public void Initialize(string _name, E_ServerTrafficStatus _state, byte _new, Action _cb)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector2.one;

        ServerName.text = _name;
        ServerStateType = _new != 0 ? UILoginEnum.E_ServerState.New : UILoginEnum.E_ServerState.None;
        ServerState.text = UILoginData.GetTrafficStateTxt(_state);
        ServerAlram.text = UILoginData.GetStateText(ServerStateType);
        SelectCallback = _cb;

        Alram.SetActive(_new != 0);
    }

	#region Input
	/// <summary>서버 선택 콜백 실행</summary>
	public void OnConnectSelectServer()
    {
        SelectCallback?.Invoke();
    }
	#endregion
}