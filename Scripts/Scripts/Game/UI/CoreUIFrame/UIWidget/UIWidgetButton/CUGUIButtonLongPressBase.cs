using UnityEngine;

abstract public class CUGUIButtonLongPressBase : CUGUIButtonBase 
{
    [SerializeField]
    private float StartWaiting = 0.2f;   // 최초 발생 시간까지 대기시간 
    [SerializeField]
    private float EventInterval = 0.1f;  // + - 버튼과 같이 누른 상태에서 계속 이벤트가 발생하는 간격 
    [SerializeField]
    private bool  EventOnce = false;    // 아이콘 롱 프레스와 같이 한번만 발생  
}
