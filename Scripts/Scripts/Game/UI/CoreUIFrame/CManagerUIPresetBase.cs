using UnityEngine;

// 주의 ! 여기 등록된 아틀라스는 메모리 해제가 되지 않으므로 선별에 주의할것 

abstract public class CManagerUIPresetBase : CManagerTemplateBase<CManagerUIPresetBase>
{
    private CAtlasReference mAtlasReference = null;

    //-----------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        mAtlasReference = GetComponentInChildren<CAtlasReference>();        
    }

	/// <summary> Atlas리스트중에 찾고자하는 <see cref="Sprite"/>가 존재하면 반환해준다. </summary>
	/// <returns>없다면 null</returns>
    public Sprite GetSprite(string _spriteName)
    {
        Sprite result = null;
        if (false == string.IsNullOrEmpty(_spriteName) && mAtlasReference)
		{
            result = mAtlasReference.GetSprite(_spriteName);
        }
        return result;
    }

    public void SetSprite(ZImage _image, string _spriteName)
	{
        Sprite spriteImage = GetSprite(_spriteName);
        if (spriteImage)
		{
            _image.sprite = spriteImage;
		}
	}

    public string GetUIPresetLocalizingText(string _TextKey)
    {
        return OnUIPresetLocalizingText(_TextKey);
    }

    //------------------------------------------------------------------------
    protected void ProtUIPressetSetAtlaReference(CAtlasReference _AtlasReference)
	{
        mAtlasReference = _AtlasReference;
	}

    //------------------------------------------------------------------------
    protected virtual string OnUIPresetLocalizingText(string _TextKey) { return _TextKey; } 

}
