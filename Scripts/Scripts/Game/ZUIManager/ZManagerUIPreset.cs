using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ZManagerUIPreset : CManagerUIPresetBase
{ public static new ZManagerUIPreset Instance { get { return CManagerUIPresetBase.Instance as ZManagerUIPreset; } }

	[SerializeField]
	private string AddressableName = "UIAtlasReference";

	//--------------------------------------------------------
	public void DoUIPresetLoadAtlas()
	{
		Addressables.InstantiateAsync(AddressableName, gameObject.transform).Completed += HandlePresetLoadAtlas;
	}

    //--------------------------------------------------------
    protected override string OnUIPresetLocalizingText(string _TextKey)
    {
		if (_TextKey == null) return null;

		if (GameDBManager.hasInstance == false) return _TextKey;

		string ResultText = _TextKey;

		GameDB.Locale_Table LocalTable;
		if (DBLocale.TryGet(_TextKey, out LocalTable))
        {
			ResultText = LocalTable.Text;
		}


		return ResultText;
    }
	//--------------------------------------------------------
	private void HandlePresetLoadAtlas(AsyncOperationHandle<GameObject> _LoadObject)
	{
		if (_LoadObject.Status == AsyncOperationStatus.Succeeded)
		{
			CAtlasReference AtlasRef = _LoadObject.Result.gameObject.GetComponent<CAtlasReference>();
			ProtUIPressetSetAtlaReference(AtlasRef);
		}
	}
	//-------------------------------------------------------
}
