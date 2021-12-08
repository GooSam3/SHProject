using UnityEngine;

public class OptionSetting : MonoBehaviour
{
    public bool bAutoSave = true;

    public virtual void Start()
    {
        LoadOption();
    }

    public virtual void OnEnable()
    {
        LoadOption();
    }

    public virtual void OnDisable()
    {
        if(bAutoSave)
            SaveOption();
    }

    public virtual void LoadOption()
    {
        
    }

    public virtual void SaveOption()
    {

    }
}
