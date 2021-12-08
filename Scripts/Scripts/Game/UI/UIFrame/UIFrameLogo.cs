using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class UIFrameLogo : ZUIFrameBase
{
    [SerializeField] private GameObject GameDescription;
 
    private bool mPlaying = false; public bool IsPlaying { get { return mPlaying; } }
    private VideoPlayer mVideoPlayer = null;
    //-----------------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();       
        mVideoPlayer = GetComponentInChildren<VideoPlayer>();
        mPlaying = true;
        GameDescription.SetActive(false);
    }

    protected override void OnUnityStart()
    {
        base.OnUnityStart();
        StartCoroutine(CoroutineLogoEndDelay());
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 )
        {
            mPlaying = false;
        }
    }

	protected override void OnCommandContents(ZCommandUIButton.E_UIButtonCommand _commandID, ZCommandUIButton.E_UIButtonGroup _groupID, int _arguement, CUGUIWidgetBase _commandOwner)
	{
        mPlaying = false;
        StopAllCoroutines();
    }
   
    //-----------------------------------------------------------------------------
    private IEnumerator CoroutineLogoEndDelay()
    {
		if (!ZGameManager.Instance.StarterData.SkipLogo)
		{
			float VideoLength = (float)mVideoPlayer.length;
			yield return new WaitForSeconds(VideoLength);
			GameDescription.SetActive(true);
			yield return new WaitForSeconds(3);
		}
        mPlaying = false;
        yield break;
    }
}
