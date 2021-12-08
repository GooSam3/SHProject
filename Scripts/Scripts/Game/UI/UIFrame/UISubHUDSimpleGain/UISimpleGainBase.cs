using UnityEngine;
using uTools;

public abstract class UISimpleGainBase : MonoBehaviour
{
	[SerializeField] private uTweenScale tweenScale;
	[SerializeField] private uTweenAlpha tweenAlpha;
	[SerializeField] private uTweenPosition tweenPosition;
	[SerializeField] private ZText gainText;

	public void Initialized()
	{
		tweenScale.onFinished.AddListener( FinishedTweener );
	}

	public void DoStart( ulong value )
	{
		gameObject.SetActive( true );

		gainText.text = $"{value}";

		tweenScale.ResetToBeginning();
		tweenAlpha.ResetToBeginning();
		tweenPosition.ResetToBeginning();
		tweenScale.enabled = true;
		tweenAlpha.enabled = true;
		tweenPosition.enabled = true;
	}

	public void DoStop()
	{
		tweenScale.onFinished.RemoveListener( FinishedTweener );
		gameObject.SetActive( false );
	}

	private void FinishedTweener()
	{
		gameObject.SetActive( false );
	}

	public void DoActivate( bool bActive )
	{
		gainText.enabled = bActive;
	}
}