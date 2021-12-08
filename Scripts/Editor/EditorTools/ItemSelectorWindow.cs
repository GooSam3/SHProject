using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 특정 아이템 리스트중에 하나를 선택해서 반환해주는 기능 윈도우
/// </summary>
public class ItemSelectorWindow : EditorWindow
{
	public struct Item
	{
		public string KeyName;
		public object Data;
	}

	static ItemSelectorWindow sWindow;

	static public Item? SelectedItem { get; private set; }
	static System.Action<Item> SelectionCallback;

	List<Item> mListDisplayItem = new List<Item>();
	List<string> mListDisplayName = new List<string>();

	int mCurSelectedIdx = -1;

	public static bool Visible
	{
		get { return (sWindow != null); }
	}

	public static void Show(string title, List<Item> itemList, System.Action<Item> selectCallback)
	{
		sWindow = (ItemSelectorWindow)EditorWindow.GetWindow(typeof(ItemSelectorWindow));
		sWindow.mListDisplayItem.Clear();
		sWindow.mListDisplayName.Clear();
		foreach (var item in itemList)
		{
			sWindow.mListDisplayItem.Add(item);
			sWindow.mListDisplayName.Add(item.KeyName);
		}

		SelectionCallback = selectCallback;

		sWindow.titleContent = new GUIContent(title);
		sWindow.ShowAuxWindow();
	}

	Vector2 scrollPos;
	void OnGUI()
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		{
			int columnCnt = (int)(this.position.width / 220);
			columnCnt = columnCnt == 0 ? 1 : columnCnt;

			//mCurSelectedIdx = mListDisplayItem.FindIndex((item) => item.KeyName == SelectedItem);
			mCurSelectedIdx = GUILayout.SelectionGrid(mCurSelectedIdx, mListDisplayName.ToArray(), columnCnt, ZGUIStyles.RichButton);
			if (mCurSelectedIdx != -1)
			{
				SelectedItem = mListDisplayItem[mCurSelectedIdx];
			}
			else
			{
				SelectedItem = null;
			}
		}

		GUILayout.EndScrollView();

		ZGUIStyles.Separator();

		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(SelectedItem.HasValue ? SelectedItem.Value.KeyName : "미선택", ZGUIStyles.RichText, GUILayout.ExpandWidth(true), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		using (var h1 = new GUILayout.HorizontalScope())
		{
			GUI.enabled = SelectedItem.HasValue;

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Ok", GUILayout.Width(100), GUILayout.Height(30)))
			{
				SelectionCallback?.Invoke(SelectedItem.Value);
				Close();
			}
			GUILayout.FlexibleSpace();

			GUI.enabled = true;
		}
	}
}
