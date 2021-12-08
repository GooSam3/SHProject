using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKillMessage : ZUIFrameBase
{
	public class MessageInfo
	{
		public float TotalDuration;
		public string MessageText;
		public E_KillMessage MessageType;
		public float Duration;
	}

	[SerializeField] private UIKillMessageItem killMessageItemPf;

	private Queue<MessageInfo> messageQueue = new Queue<MessageInfo>();

	protected override void OnInitialize()
	{
		base.OnInitialize();
		killMessageItemPf.Initialize();
	}

	public void AddMessage( string _txt, E_KillMessage _messageType, float _duration )
	{
		MessageInfo messageInfo = new MessageInfo();
		messageInfo.MessageText = _txt;
		messageInfo.MessageType = _messageType;
		messageInfo.TotalDuration = _duration;
		messageInfo.Duration = _duration;
		messageQueue.Enqueue( messageInfo );
	}

	void Update()
	{
		if( messageQueue.Count == 0 ) {
			return;
		}

		var info = messageQueue.Peek();
		
		if( info.Duration == info.TotalDuration ) {
			killMessageItemPf.Show( info );
		}
		
		if( info.Duration > 0 ) {
			info.Duration -= Time.deltaTime;
		}
		else {
			killMessageItemPf.Hide();
			messageQueue.Dequeue();
		}
	}
}
