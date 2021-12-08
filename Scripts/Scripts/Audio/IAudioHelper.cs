using ClockStone;
using UnityEngine;

public interface IAudioHelper
{
    AudioItem GetAudioItem(AudioClip clip, string categoryName);

    AudioCategory GetCategory(string categoryName);
}