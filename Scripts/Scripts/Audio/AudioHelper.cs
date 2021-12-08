using ClockStone;
using UnityEngine;

public class AudioHelper : MonoBehaviour, IAudioHelper
{
    public AudioItem GetAudioItem(AudioClip clip, string category)
    {
        AudioItem audioItem = AudioController.GetAudioItem(clip.name);

        if (audioItem == null)
        {
            AudioCategory audioCategory = GetCategory(category);

            audioItem = new AudioItem();
            audioItem.Name = clip.name;
            audioItem.subItems = new AudioSubItem[1];

            var audioSubItem = new AudioSubItem();
            audioSubItem.Clip = clip;
            audioItem.subItems[0] = audioSubItem;

            AudioController.AddToCategory(audioCategory, audioItem);
        }

        return audioItem;
    }

    public AudioCategory GetCategory(string category)
    {
        AudioCategory audioCategory = AudioController.GetCategory(category);

        if (audioCategory == null)
        {
            audioCategory = new AudioCategory(AudioManager.Instance.Controller);
            audioCategory.Name = category;

            ArrayHelper.AddArrayElement(ref AudioController.Instance.AudioCategories, audioCategory);
        }

        return audioCategory;
    }
}
