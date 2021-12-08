using Zero;

public class UIPatchManagerLoader : Singleton<UIPatchManagerLoader>
{
    public void Initialize()
    {
        Destroy(this.gameObject);
    }
}
