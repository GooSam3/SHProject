using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ljh : 채팅버튼이 위치하는 관계로 채팅관리코드 추가하겠습니다.
/// 이슈생길시 따로 빼겠습니다.
/// </summary>
public class UISubHUDLeftMenu : ZUIFrameBase
{
	[SerializeField] private UIChatScrollAdapter scrollChat;

	[SerializeField] private CanvasGroup hudChatGroup;

	private Sequence sequence;

	public void Init()
	{
		ZPoolManager.Instance.Spawn( E_PoolType.UI, $"{nameof( UIChattingListItem )}", obj=> {
			Initialize();
			scrollChat.SetAutoFocus( true );
			ZPoolManager.Instance.Return(obj);
		} );
	}

	private void Initialize()
	{
		scrollChat.InitScrollData( null, true );
		ZWebChatData.OnAddMsg += OnAddChatMessage;
		ZNet.Data.Me.CurCharData.ChatFilterUpdate += OnUpdateChatFilter;
	}

	protected override void OnShow( int _LayerOrder )
	{
		base.OnShow( _LayerOrder );

		if( scrollChat.IsInitialized ) {
			scrollChat.SetNormalizedPosition( 1 );
			scrollChat.SetAutoFocus( true );
		}
		hudChatGroup.alpha = 0f;
	}

	protected override void OnHide()
	{
		base.OnHide();

		scrollChat.SetAutoFocus( false );

		if( sequence != null && sequence.active )
			sequence.Kill( false );
	}
	protected override void OnRemove()
	{
		ZWebChatData.OnAddMsg -= OnAddChatMessage;
		ZNet.Data.Me.CurCharData.ChatFilterUpdate -= OnUpdateChatFilter;

		base.OnRemove();
	}

	public void SelectChat()
	{
		UIFrameChatting chattingFrame = UIManager.Instance.Find<UIFrameChatting>();

		if( chattingFrame == null ) {
			UIManager.Instance.Load( nameof( UIFrameChatting ), ( _loadName, _loadFrame ) => {
				( _loadFrame as UIFrameChatting ).Init( () => {
					UIManager.Instance.Open<UIFrameChatting>();
				} );
			} );
		}
		else {
			// 로드 안된상태에서 들어옴
			if (chattingFrame.IsInitialized == false)
				return;

			if( !chattingFrame.Show ) {
				var hudSubMenu = UIManager.Instance.Find<UISubHUDCharacterState>();
				if( hudSubMenu != null ) {
					hudSubMenu.OnActiveInfoPopup(false);
				}
				UIManager.Instance.Open<UIFrameChatting>();
			}
			else
				UIManager.Instance.Close<UIFrameChatting>();
		}
	}

	public void OnAddChatMessage( ZDefine.ChatFilter filter, ZDefine.ChatData msg )
	{
		if( filter == ZDefine.ChatFilter.TYPE_NONE || filter == ZDefine.ChatFilter.TYPE_TRADE )
			return;

		if( ZNet.Data.Me.CurCharData.chatFilter.HasFlag( filter ) == false )
			return;

		if (msg.type == ZDefine.ChatViewType.TYPE_SYSTEM_GUILD_GREETING)
			return;


		scrollChat.AddChatData( msg );

		if( sequence != null && sequence.active )
			sequence.Kill( false );

		if( this.isActiveAndEnabled == false )
			return;

		sequence = DOTween.Sequence().Join( hudChatGroup.DOFade( 1, .2f ) ).
									  AppendInterval( DBConfig.HUD_Chatting_Fade_Interval ).
									  Append( hudChatGroup.DOFade( 0, .2f ) ).Play();
	}

	public void OnUpdateChatFilter()
	{
		if( ZNet.Data.Me.CurCharData.chatFilter.HasFlag( ZDefine.ChatFilter.TYPE_TRADE ) )
			ZNet.Data.Me.CurCharData.chatFilter &= ~ZDefine.ChatFilter.TYPE_TRADE;

		scrollChat.ResetData( ZNet.Data.Me.CurCharData.chatFilter );
	}

}