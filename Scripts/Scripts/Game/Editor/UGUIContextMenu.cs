using NodeCanvas.BehaviourTrees;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

static class UGUIContextMenu
{
    [MenuItem("GameObject/ZUI/ZButton", false, 30)]
    static void ContextmenuZButton()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Bt_");           
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZButton>();
            LastSelecting = NewObject;     
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }
     
    [MenuItem("GameObject/ZUI/ZDropDown", false, 31)]
    static void ContextmenuZDropDown()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Dp_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZDropDown>();
            LastSelecting = NewObject;
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }

    [MenuItem("GameObject/ZUI/ZImage", false, 32)]
    static void ContextmenuZImage()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Img_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZImage>();
            LastSelecting = NewObject;
            SelectObject(NewObject);
        }
    }

    [MenuItem("GameObject/ZUI/ZImageClickable", false, 33)]
    static void ContextmenuZImageClickable()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("ImgC_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZImageClickable>();
            LastSelecting = NewObject;
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }

    [MenuItem("GameObject/ZUI/ZScrollBar", false, 34)]
    static void ContextmenuZScrollBar()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Scroll_Bar");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZScrollBar>();
            LastSelecting = NewObject;         
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }

    [MenuItem("GameObject/ZUI/ZScrollRect", false, 35)]
    static void ContextmenuZScrollRect()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Scroll_View");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZScrollRect>();
            NewObject.AddComponent<Mask>();
            NewObject.AddComponent<ZImage>();

            GameObject NewGrid = new GameObject("Grid");
            NewGrid.AddComponent<VerticalLayoutGroup>();
            NewGrid.transform.SetParent(NewObject.transform, false);
            LastSelecting = NewGrid;            
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }

    [MenuItem("GameObject/ZUI/ZSlider", false, 36)]
    static void ContextmenuZSlider()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Gauge_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ZSlider>();
            LastSelecting = NewObject;
        }

        if (LastSelecting != null)
        {
            Collapse(LastSelecting, true);
        }
    }

    [MenuItem("GameObject/ZUI/ZText", false, 37)]
    static void ContextmenuZText()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Txt_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            ZText TextComponent = NewObject.AddComponent<ZText>();
            TextComponent.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/IcarusSource/Font/MainFont.otf");
            TextComponent.fontSize = 24;
            TextComponent.lineSpacing = 1.2f;
            TextComponent.supportRichText = true;
            TextComponent.resizeTextForBestFit = true;
            TextComponent.resizeTextMinSize = 12;
            TextComponent.resizeTextMaxSize = 24;
            TextComponent.alignment = TextAnchor.MiddleLeft;
            TextComponent.raycastTarget = true;


            SelectObject(NewObject);
        }
    
    }

    [MenuItem("GameObject/ZUI/ZToggle", false, 38)]
    static void ContextmenuZToggle()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
       
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject ToggleParent = new GameObject("ZToggle");
            ToggleParent.transform.SetParent(arrSelectObject[i].transform, false);
            ZToggle Toggle = ToggleParent.AddComponent<ZToggle>();

            GameObject Bg = new GameObject("Bg");
            Bg.transform.SetParent(ToggleParent.transform, false);
            Bg.AddComponent<ZImage>();
            Toggle.targetGraphic = Bg.GetComponent<ZImage>();

            GameObject ToggleObjectOn = new GameObject("On");
			ToggleObjectOn.transform.SetParent(ToggleParent.transform, false);
            ToggleObjectOn.AddComponent<RectTransform>();
            Toggle.On = ToggleObjectOn;

            GameObject ToggleObjectOff = new GameObject("Off");
			ToggleObjectOff.transform.SetParent(ToggleParent.transform, false);
            ToggleObjectOff.AddComponent<RectTransform>();
            Toggle.Off = ToggleObjectOff;

            GameObject TitleText = new GameObject("Txt");
            TitleText.transform.SetParent(ToggleParent.transform, false);
            TitleText.AddComponent<ZText>();
        }
    }

    [MenuItem("GameObject/ZUI/ZRadio", false, 38)]
    static void ContextmenuZRadio()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;

        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject ToggleGroupParent = new GameObject("ToggleGroup");
            ToggleGroupParent.transform.SetParent(arrSelectObject[i].transform, false);
            ZToggleGroup ToggleGroup = ToggleGroupParent.AddComponent<ZToggleGroup>();

            for(int j = 0; j < 2; j++)
            {
                GameObject ToggleParent = new GameObject("ZToggle_" + j.ToString());
                ToggleParent.transform.SetParent(ToggleGroupParent.transform, false);
                ZToggle Toggle = ToggleParent.AddComponent<ZToggle>();
                Toggle.group = ToggleGroup;
                Toggle.ZToggleGroup = ToggleGroup;

                GameObject Bg = new GameObject("Bg");
                Bg.transform.SetParent(ToggleParent.transform, false);
                Bg.AddComponent<ZImage>();
                Toggle.targetGraphic = Bg.GetComponent<ZImage>();

                GameObject ToggleObjectOn = new GameObject("On");
                ToggleObjectOn.transform.SetParent(ToggleParent.transform, false);
                ToggleObjectOn.AddComponent<RectTransform>();
                Toggle.On = ToggleObjectOn;

                GameObject ToggleObjectOff = new GameObject("Off");
                ToggleObjectOff.transform.SetParent(ToggleParent.transform, false);
                ToggleObjectOff.AddComponent<RectTransform>();
                Toggle.Off = ToggleObjectOff;

                GameObject TitleText = new GameObject("Txt");
                TitleText.transform.SetParent(ToggleParent.transform, false);
                TitleText.AddComponent<ZText>();
            }
        }
    }

    //--------------------------------------------------------------------------
    [MenuItem("GameObject/ZParticle", false, 30)]
    static void ContextmenuZParticle()
    {
        GameObject[] arrSelectObject = Selection.gameObjects;
        GameObject LastSelecting = null;
        for (int i = 0; i < arrSelectObject.Length; i++)
        {
            GameObject NewObject = new GameObject("Eff_");
            NewObject.transform.SetParent(arrSelectObject[i].transform, false);
            NewObject.AddComponent<ParticleSystem>();
            LastSelecting = NewObject;
        }

        if (LastSelecting != null)
		{
            Collapse(LastSelecting, true);
        }
    }

    //---------------------------------------------------------------------------

    public static void Collapse(GameObject go, bool collapse)
    {       
        var hierarchy = GetFocusedWindow("Hierarchy");
        SelectObject(go);
        var key = new Event { keyCode = collapse ? KeyCode.RightArrow : KeyCode.LeftArrow, type = EventType.KeyDown };
        hierarchy.SendEvent(key);
    }

    public static void CollapseDepth1(GameObject Parent, GameObject Child)
	{
        var hierarchy = GetFocusedWindow("Hierarchy");
        SelectObject(Parent);
        var key = new Event { keyCode = KeyCode.RightArrow, type = EventType.KeyDown };
        hierarchy.SendEvent(key);
        SelectObject(Child);
    }

    public static void SelectObject(Object obj)
    {
        Selection.activeObject = obj;
    }
    public static EditorWindow GetFocusedWindow(string window)
    {
        FocusOnWindow(window);
        return EditorWindow.focusedWindow;
    }
    public static void FocusOnWindow(string window)
    {
        EditorApplication.ExecuteMenuItem("Window/General/" + window);  // 유니티 버전에 따라 호출 상수 문자열이 다르다.
    }
}
