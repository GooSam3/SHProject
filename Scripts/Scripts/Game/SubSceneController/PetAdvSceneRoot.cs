using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetAdvSceneRoot : MonoBehaviour
{
	public List<Transform> ListModelRoot = new List<Transform>();

	public List<GameObject> ListBackground = new List<GameObject>();

	public Transform SpawnRoot;

	[SerializeField]
	private Camera sceneCamera;

	private RenderTexture renderTexture;

	private Dictionary<int, ViewModelAnimEventListener> dicSpawnModel = new Dictionary<int, ViewModelAnimEventListener>();

	public RenderTexture SetRenderTexture(Vector2 size)
	{
		renderTexture = new RenderTexture((int)size.x, (int)size.y, 0);
		renderTexture.Create();

		sceneCamera.targetTexture = renderTexture;

		return renderTexture;
	}
	public void AddSpawnDic(int idx, ViewModelAnimEventListener modelController)
	{
		if (dicSpawnModel.ContainsKey(idx))
			return;

		dicSpawnModel.Add(idx, modelController);
	}

	public void SetSocket()
	{
		foreach(var iter in dicSpawnModel)
		{
			var target = GetTargetSocket(iter.Key);

			if (dicSpawnModel.TryGetValue(target, out var control))
			{
				iter.Value.SetHitTarget(control.SocketHit);
			}
			else
				iter.Value.SetHitTarget(null);
		}
	}

	public void ClearSpawnDic()
	{
		dicSpawnModel.Clear();	
	}

	// 반대편 root 가져옴, 수정해야함
	private int GetTargetSocket(int idx)
	{
		int targetIdx = idx;

		if (idx >= 4)
			targetIdx -= 4;
		else
			targetIdx += 4;

		return targetIdx;
	}

	public void OnDestroy()
	{
		renderTexture.Release();
		renderTexture.DiscardContents();
		Destroy(renderTexture);
	}
}
