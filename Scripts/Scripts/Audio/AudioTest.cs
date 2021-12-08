using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zero;

public class AudioTest : Singleton<AudioTest>
{
    public static bool mTestAudio;

    #region Test용 UI 관련 변수
    [SerializeField]
    Text filename = null;
    [SerializeField]
    Text volume = null;
    [SerializeField]
    Text delay = null;
    [SerializeField]
    Text starttime = null;
    [SerializeField]
    Text speedrate = null;
    [SerializeField]
    Text fadestate = null;
    [SerializeField]
    Text fadein = null;
    [SerializeField]
    Text fadeout = null;
    [SerializeField]
    Text audiotype = null;
    [SerializeField]
    Text loop = null;
    #endregion

    [SerializeField]
    GameObject mListener = null;

    public override void Awake()
    {
        mTestAudio = true;
    }

    #region Test용 함수
    public void OnClickDucking()
    {
        AudioManager.Instance.DuckingAllAudio(E_SoundType.BGM, 3.0f);
    }

    public void OnClickPlay()
    {
        AudioManager.Instance.Play(Convert.ToUInt32(filename.text));
    }

    public void OnClickAllStop()
    {
        AudioManager.Instance.StopAllAudio();
    }

    public void OnClickTestSFX()
    {
        AudioManager.Instance.PlaySFX(Convert.ToUInt32(filename.text), mListener.transform.position);
    }
    #endregion

    public TestAudioForm GetFile()
    {
        float _volume     = (volume.text    == null || volume.text    == string.Empty) ? 1.0f            : Convert.ToSingle(volume.text);
        float _delay      = (delay.text     == null || delay.text     == string.Empty) ? 0.0f            : Convert.ToSingle(delay.text);
        float _starttime  = (starttime.text == null || starttime.text == string.Empty) ? 0.0f            : Convert.ToSingle(starttime.text);
        float _speedrate  = (speedrate.text == null || speedrate.text == string.Empty) ? 1.0f            : Convert.ToSingle(speedrate.text);
        bool _fadestate   = (fadestate.text == null || fadestate.text == string.Empty) ? false           : Convert.ToBoolean(Convert.ToInt32(fadestate.text));
        float _fadein     = (fadein.text    == null || fadein.text    == string.Empty) ? 0.0f            : Convert.ToSingle(fadein.text);
        float _fadeout    = (fadeout.text   == null || fadeout.text   == string.Empty) ? 0.0f            : Convert.ToSingle(fadeout.text);
        E_SoundType _type = (audiotype.text == null || audiotype.text == string.Empty) ? E_SoundType.BGM : (E_SoundType)Convert.ToInt32(audiotype.text);
        bool _loop        = (loop.text      == null || loop.text      == string.Empty) ? false           : Convert.ToBoolean(Convert.ToInt32(loop.text));

        TestAudioForm form = new TestAudioForm
        {
            Volume = _volume,
            Delay = _delay,
            StartTime = _starttime,
            SpeedRate = _speedrate,
            FadeState = _fadestate,
            FadeIn = _fadein,
            FadeOut = _fadeout,
            Type = _type,
            Loop = _loop
        };

        return form;
    }
}

public class TestAudioForm
{
    public uint Name;
    public float Volume;
    public float Delay;
    public float StartTime;
    public float SpeedRate;
    public bool FadeState;
    public float FadeIn;
    public float FadeOut;
    public E_SoundType Type;
    public bool Loop;
}