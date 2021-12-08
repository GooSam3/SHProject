using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
// 비동기 입출력을 지원하는 텍스처 출력 기능 

public class ZRawImage : RawImage
{
	private string mCurrentTextName = null;
	private Texture mDefaultTexture = null;
	//-------------------------------------------------------------
	protected override void Awake()
	{
		base.Awake();
		mDefaultTexture = texture;
	}

	//----------------------------------------------------------------
	/// <summary>
	/// 최초 할당된 리소스는 보관해서 로딩시마다 출력해 준다.  
	/// </summary>
	public void LoadTexture(string _addressableName)
	{
		if (_addressableName == mCurrentTextName) return;

		RemoveTexture();

		mCurrentTextName = _addressableName;
		Addressables.LoadAssetAsync<Texture>(_addressableName).Completed += (AsyncOperationHandle<Texture> _loadedObject) =>
		{
			texture = _loadedObject.Result;
		};
	}

	private void RemoveTexture()
	{
		if (texture != null)
		{
			if (texture != mDefaultTexture)
			{
				Addressables.Release<Texture>(texture);
			}
			texture = mDefaultTexture;
		}
	}
}
