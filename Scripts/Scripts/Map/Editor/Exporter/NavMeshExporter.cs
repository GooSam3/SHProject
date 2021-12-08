/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.

This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder. */

/*
 * Edited by KPRO (김경호)
 * 
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using UnityEngine.AI;
using MS.Shell.Editor;
using System.Collections.Generic;
using System.Globalization;

public class NavMeshExporter
{
    private static string OutputFolder = "./ExportedNavMesh";

    private static string MeshToString(string name, NavMeshTriangulation tri)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("g ").Append(name).Append("\n");

        foreach (Vector3 lv in tri.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", lv.x, lv.y, lv.z));
        }

        for (int i = 0; i < tri.indices.Length; i += 3)
        {
            sb.Append(string.Format("f {0} {1} {2}\n",
                tri.indices[i] + 1, tri.indices[i + 1] + 1, tri.indices[i + 2] + 1));
        }

        return sb.ToString();
    }

    private static bool CreateTargetFolder()
    {
        try
        {
            var dirInfo = Directory.CreateDirectory(OutputFolder);

			Debug.Log($"NavMeshExport Output Directory : {dirInfo.FullName}");
		}
        catch
        {
            EditorUtility.DisplayDialog("Error!", $"Failed to create target folder!\nPath: {OutputFolder}", "OK");
            return false;
        }
        return true;
    }

	[MenuItem("ZGame/Export NavMesh -->> OBJ")]
	public static void ExportNavMesh_WithoutCommit()
	{
		ExportNavMesh(false);
	}

	[MenuItem("ZGame/Export NavMesh -->> OBJ (with Commit)")]
	public static void ExportNavMesh_WithCommit()
	{
		ExportNavMesh(true);
	}

    private static void ExportNavMesh(bool withCommit)
    {
        if (!CreateTargetFolder())
            return;

        bool isExported = false;
        string filename = string.Empty;
        try
        {
            var tri = NavMesh.CalculateTriangulation();

            if (0 >= tri.vertices.Length)
            {
                //throw new Exception("vertices.Length is zero");
            }

            if (0 >= tri.indices.Length)
            {
                //throw new Exception("indices.Length is zero");
            }

            filename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            using (StreamWriter sw = new StreamWriter(OutputFolder + "/" + filename + ".obj"))
            {
                sw.Write("mtllib ./" + filename + ".mtl\n");
                sw.Write(MeshToString(filename, tri));
            }
            isExported = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        if (isExported)
        {
			if (withCommit)
			{
				SVN_Commit($"\"update '{filename}' NavMesh for MMO\"", new string[] { $"{filename}.obj" },
					(/*success*/) =>
					{
						if (EditorUtility.DisplayDialog("NavMesh Exporter", $"[{OutputFolder}/{filename}] 파일 추출 및 SVN Commit 성공!!", "OK"))
						{
						}
					},
					(/*failure*/) =>
					{
						if (EditorUtility.DisplayDialog("NavMesh Exporter", $"[{OutputFolder}/{filename}] 파일 추출은 성공했지만, SVN Commit은 실패!!", "OK"))
						{
						}
					});
			}
			else
			{
				if (EditorUtility.DisplayDialog("NavMesh Exporter", $"[{OutputFolder}/{filename}] 파일 추출 성공!!", "OK"))
				{					
				}
			}

			AssetDatabase.Refresh();
		}
        else
            EditorUtility.DisplayDialog("[Error] NavMesh Exporter", $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}씬에서 추출에 실패하였습니다.\n 현재 Scene에 NavMesh가 baked돼어있는지 확인바랍니다!!.", "OK");
    }

	#region ---------- SVN Section ----------

	/// <summary> Shell작동시 기본 옵션 </summary>
	private static EditorShell.Options ShellOptionsDefault
	{
		get;
	} = new EditorShell.Options()
	{
		workDirectory = NavMeshExporter.OutputFolder,
		encoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
		environmentVars = new Dictionary<string, string>()
		{
		}
	};

	/// <summary> commit전에 add먼저 해야되는지 여부 </summary>
	private static bool bNeedAddBeforeCommit = false;

	private static void SVN_Commit(string message, string[] files, System.Action success = null, System.Action failure = null)
	{
		bNeedAddBeforeCommit = false;

		System.Text.StringBuilder cmd = new System.Text.StringBuilder();
		cmd.Append("svn commit ");
		// 커밋할 파일 리스트 추가
		for (int i = 0; i < files.Length; i++)
		{
			cmd.Append(" " + files[i].Replace(" ", "\\ "));
		}
		// 커밋시 로그용 메시지 추가
		cmd.Append(" -m " + message);

		Debug.Log($"[SVN commit] Command | {cmd.ToString()}");
		var task = EditorShell.Execute(cmd.ToString(), ShellOptionsDefault);
		task.onExit += (exitCode) => 
		{
			if (exitCode == 1 && bNeedAddBeforeCommit)
			{
				Debug.LogWarning($"[SVN commit] : Repository에 없던 파일이라 add 작동.");

				SVN_Add(message, files, () =>
				{
					// add명령어 성공시 파일 커밋 작동하도록 하자.
					SVN_Commit(message, files, success, failure);
				}, failure);
			}
			else if (exitCode != 0)
			{
				failure?.Invoke();
			}
			else
			{
				Debug.Log($"<color=green>[SVN commit] 성공!</color>");
				success?.Invoke();
			}
		};
		task.onLog += (EditorShell.LogType LogType, string log) => 
		{
			Debug.Log($"[SVN commit] | {log}");

			// 파일추가가 안된상태라면 추가먼저 하고 커밋하도록 하자.
			if (log.Contains("is not under version control"))
			{
				bNeedAddBeforeCommit = true;				
			}
		};
	}

	private static void SVN_Add(string message, string[] files, System.Action success = null, System.Action failure = null)
	{
		System.Text.StringBuilder cmd = new System.Text.StringBuilder();
		cmd.Append("svn add ");
		// 커밋할 파일 리스트 추가
		for (int i = 0; i < files.Length; i++)
		{
			cmd.Append(" " + files[i].Replace(" ", "\\ "));
		}
		// 이미 존재하던말던 커밋
		//cmd.Append(" --force");

		Debug.Log($"[SVN add] Command | {cmd.ToString()}");
		var task = EditorShell.Execute(cmd.ToString(), ShellOptionsDefault);
		task.onExit += (exitCode) =>
		{
			if (exitCode == 0)
			{
				Debug.Log($"<color=green>[SVN add] 성공!</color>");
				success?.Invoke();
			}
			else
			{
				Debug.LogError($"[SVN add] ExitCode: {exitCode}");
				failure?.Invoke();
			}
		};
		task.onLog += (EditorShell.LogType LogType, string log) =>
		{
			Debug.Log($"[SVN add] | {log}");
		};
	}

	#endregion
}
#endif