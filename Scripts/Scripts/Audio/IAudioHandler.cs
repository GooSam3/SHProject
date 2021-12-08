using ClockStone;

public interface IAudioHandler
{
    void ApplyAudioFile(AudioItem audioFile);

    void SetSpatialBlend(AudioObject audioObject, float value);

    void SetPrimaryPitchAndBlend(AudioObject audioObject, float pitch, float blend);
}