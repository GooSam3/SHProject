using GameDB;
using UnityEngine.SceneManagement;

public class MapToolDefines
{
	/// <summary> Table 경로 데이터 저장/로드를 위한 키값 </summary>
	public const string Prefs_GameDBPath = "ICARUS_GameDBPath";

	/// <summary>
	/// 맵 객체 관리 최상위 게임객체명
	/// </summary>
	public const string MapManagementObjectName = "MapObjectRoot";

	/// <summary> <see cref="MapData"/> 저장될 경로 </summary>
	public const string Data_SaveFolder = "Assets/_ZAssetBundle/MapData/";
	/// <summary> <see cref="MapData"/> Json버전 저장될 경로 </summary>
	public static string DataJson_SaveFolder = "./ExportedMapData_Json/";

	/// <summary> 현재 프로젝트에서 편집할 Scene파일들 존재하는 폴더들 리스트</summary>
	public static readonly string[] SceneStorageFolders = new string[] 
	{
		"Assets/Scenes",
		"Assets/_ZAssetBundle/Scenes"
	};


}

/// <summary> 편집중인 씬 정보 </summary>
public class EditorSceneInfo
{
	public string Path { get; private set; }
	public Scene Scene { get; private set; }
	/// <summary> 현재 씬과 매치되는 테이블 데이터 </summary>
	public Stage_Table TableData { get; private set; }

	public MapDataManagement MapManagement { get; private set; }

	public bool IsValidPath => !string.IsNullOrEmpty(Path);
	public bool IsValidTable => null != TableData;

	public void Renew(string _scenePath, Stage_Table _stageTable, Scene _scene)
	{
		this.Path = _scenePath;
		this.Scene = _scene;
		this.TableData = _stageTable;
	}

	public void SetManagement(MapDataManagement _management)
	{
		this.MapManagement = _management;
	}
}