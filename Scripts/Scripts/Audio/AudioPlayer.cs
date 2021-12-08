using ClockStone;
using UnityEngine;

public class AudioPlayer : MonoBehaviour, IAudioHandler
{
    public void ApplyAudioFile(AudioItem audioFile)
    {
        if (null == audioFile)
            return;

        if (audioFile.FadeState)
        {
            AudioManager.Instance.Controller.musicCrossFadeTime_In  = audioFile.FadeIn;
            AudioManager.Instance.Controller.musicCrossFadeTime_Out = audioFile.FadeOut;
            AudioManager.Instance.Controller.musicCrossFadeTime     = audioFile.FadeTime;
        }
    }

    public void SetPrimaryPitchAndBlend(AudioObject audioObject, float pitch, float blend)
    {
        if (!audioObject)
            return;

        audioObject.primaryAudioSource.pitch = pitch;
        audioObject.primaryAudioSource.spatialBlend = blend;
    }

    public void SetSpatialBlend(AudioObject audioObject, float value)
    {
        if (!audioObject)
            return;

        AudioSource source = audioObject.GetComponent<AudioSource>();
        if (source != null)
            source.spatialBlend = value;
    }
}