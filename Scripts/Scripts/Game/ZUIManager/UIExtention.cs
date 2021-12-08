using DG.Tweening;

public static class UIExtention
{
    public static void Clear(this Sequence seq)
    {
        if (seq != null && seq.active)
        {
            seq.Kill();
            seq = null;
        }
    }
}
