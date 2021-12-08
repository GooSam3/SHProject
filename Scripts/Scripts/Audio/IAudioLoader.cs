using ClockStone;

public interface IAudioLoader
{
    void AddAudioFileLoadAsync(uint soundID, System.Action<AudioItem> loadCB);
}
