using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Animations;
using OfficeOpenXml.FormulaParsing.Utilities;
using Priority_Queue;

public class AnimatorTempleData
{
    public string Name = string.Empty;  // 폴더의 전체 명칭을 이름으로 정한다.
    public string ID = string.Empty;    // 폴더의 숫자를 제외한 명칭을 아이디라고 정한다.
    public int index = 0;               // 아이디값이 있을경우 거기에 순서
    public string Path = string.Empty;  // 경로

    //clipName, clipPath
    public Dictionary<E_AnimStateName, string> AniClipPaths = new Dictionary<E_AnimStateName, string>();
}

public class AnimatorTempleCreate : EditorWindow
{
    private string baseControllerPath = "Assets/IcarusSource/Animator/Icarus_Pawn_Animator_Temple.controller";
    private string lastClickPath;
    private Vector2 scrollPos;
    private string TempleFileName = "_Animator_Temple";
    private string AnimatorFileName = "_Animator";

    private List<string> LogBuilder { get; set; } = new List<string>(1000);
    //private Dictionary<string, AnimatorTempleData> dicAnimatorFolderData = new Dictionary<string, AnimatorTempleData>();
    List<string> selectPaths = new List<string>();

    // 검색 속도를 위해 한번 검색 한건 저장후 있으면 그대로 사용
    Dictionary<string, string> dicTempFilenameClip = new Dictionary<string, string>();

    [MenuItem("ZGame/Tools/Animator_Temple")]
    static void Init()
    {
        var window = (AnimatorTempleCreate)EditorWindow.GetWindow(typeof(AnimatorTempleCreate));
        window.titleContent = new GUIContent("[Extrator] CreateAnimator");
        window.minSize = new Vector2(400, 350);
    }

    void OnEnable()
    {
        // lastClickPath = EditorPrefs.GetString($"CreateAnimator_Path", Application.dataPath);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.BeginVertical();
        EditorGUILayout.TextField("애니메이션 읽을 경로 선택");
        //lastClickPath = EditorGUILayout.TextField("읽어올 FBX 경로", lastClickPath);
        //if (GUILayout.Button("...", GUILayout.Width(40f)))
        //{
        //    string path = EditorUtility.SaveFolderPanel("FBX를 읽어올 폴더 설정", "", "폴더설정");
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        lastClickPath = path.Replace(Application.dataPath, "Assets");
        //        EditorPrefs.SetString($"CreateAnimator_Path", lastClickPath);
        //    }
        //}
        GUILayout.EndVertical();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        {
            foreach (string msg in LogBuilder)
            {
                EditorGUILayout.LabelField(msg, ZGUIStyles.RichText);
            }
        }
        EditorGUILayout.EndScrollView();

        ZGUIStyles.Separator();

        if (GUILayout.Button("변경 작업 시작", GUILayout.Height(30)))
        {
            LogBuilder.Clear();

            InitAnimatorSetting();
        }
        if (GUILayout.Button("파일 이동", GUILayout.Height(30)))
        {
            LogBuilder.Clear();
        }
    }
#endif

    private void InitAnimatorSetting()
    {
        //폴더 1차 를 전부 읽어 들여서 해당 폴더에 -> Animations와 Models 폴더가 있으면 그 폴더에 설정하자
        string clickPath = string.Empty;

        var currentFolderParts = GetSelectPaths(out clickPath);

        if (currentFolderParts.Count != 0)
        {
            lastClickPath = clickPath;
        }
        else
        {
            currentFolderParts.Add(lastClickPath);
        }

        var dicAnimatorFolderData = LoadFolderData(currentFolderParts);

        if (dicAnimatorFolderData.Count == 0)
        {
            AddLog($"변경시킬 폴더가 없습니다. 경로 확인 해주세요. ", Color.red);
            return;
        }


        var listData = dicAnimatorFolderData.ToList();

        foreach (var folderData in dicAnimatorFolderData)
        {
            string animatorFileName = $"{Path.GetFileNameWithoutExtension(folderData.Value.Path)}{AnimatorFileName }";
            string animatorFullPaths = $"{ folderData.Value.Path}/{animatorFileName }.overrideController";
            var animatorController = AssetDatabase.LoadAssetAtPath(animatorFullPaths, typeof(AnimatorOverrideController)) as AnimatorOverrideController;
            AnimatorTempleData baseTempleData = null;

            string animatorClipTargetFolderPath = GetTargetClipList(animatorController);
            AnimatorTempleData animatorClipTargetFolderData = LoadFolderData(animatorClipTargetFolderPath);


            if (animatorController == null) { Debug.Log("Animator파일 로드 안됨" + animatorFileName); }

            if (!(folderData.Value.index == -1 && folderData.Value.index == 0))
            {
                baseTempleData = listData.Find(obj => obj.Value.ID == folderData.Value.ID && obj.Value.index == 1).Value;
            }

            var templeController = CreateAnimator(folderData.Value);

            DoSetAnimaotrCliptData(templeController, animatorController, folderData.Value, baseTempleData, animatorClipTargetFolderData);
        }
    }


    AnimatorTempleData LoadFolderData(string folderPath)
    {
        if (folderPath.Equals(string.Empty) == true) return null;
        List<string> folderPaths = new List<string>();
        folderPaths.Add(folderPath);
        var dicFolderTempelData = LoadFolderData(folderPaths);
        if (dicFolderTempelData.Count == 0)
        {
            return null;
        }
        var listTempData = dicFolderTempelData.ToList();
        return listTempData[0].Value;
    }
    //폴더 데이터 생성
    public Dictionary<string, AnimatorTempleData> LoadFolderData(List<string> folderPaths)
    {
        Dictionary<string, AnimatorTempleData> dicAnimatorFolderData = new Dictionary<string, AnimatorTempleData>();
        foreach (var folderPart in folderPaths)
        {
            //자식 폴더가 0개거나  Models폴더가 없으면 애니메이션 기본 폴더가 아니여서 제외 시킨다.
            var parentParts = Directory.GetDirectories(folderPart).ToList();
            if (parentParts.Count == 0) continue;
            var modelsParts = parentParts.Find(obj => obj.Contains("Models"));
            if (modelsParts == null) continue;

            AnimatorTempleData animatorTempledata = null;
            string name = $"{Path.GetFileNameWithoutExtension(folderPart)}";

            string ID = string.Empty;
            int index = 0;

            var folderSplitParts = name.Split('_');
            index = -1;
            //ID
            if (int.TryParse(folderSplitParts[folderSplitParts.Length - 1], out index))
            {
                for (int i = 0; i < folderSplitParts.Length; i++)
                {
                    if (i < folderSplitParts.Length - 1)
                    {
                        ID += folderSplitParts[i];
                    }

                    if (folderSplitParts.Length - 2 == i) break;
                    ID += "_";
                }
            }
            else
            {
                ID = string.Empty;
            }

            if (dicAnimatorFolderData.ContainsKey(name))
            {
                AddLog($"같은 이름의 폴더가 존재 합니다. 폴더이름 확인  : {folderPart}", Color.red);
            }
            else
            {
                animatorTempledata = new AnimatorTempleData();
                animatorTempledata.ID = ID;
                animatorTempledata.Name = name;
                animatorTempledata.index = index;
                animatorTempledata.Path = folderPart;
                dicAnimatorFolderData.Add(animatorTempledata.Name, animatorTempledata);
            }
            LoadAnimatorClipData(animatorTempledata);
        }
        return dicAnimatorFolderData;
    }


    private E_AnimStateName FindAnimatorCliptType(E_AnimStateName eType, string findClipType, List<string> paths, out string fileClipPath)
    {
        if (dicTempFilenameClip.ContainsKey(findClipType))
        {
            fileClipPath = dicTempFilenameClip[findClipType];
            return eType;
        }

        foreach (var path in paths)
        {

            string fileClipName = $"{Path.GetFileNameWithoutExtension(path)}";
            string type = string.Empty;

            var fileClipNameParts = fileClipName.Split('_');
            type = fileClipNameParts[fileClipNameParts.Length - 1];
            if (fileClipNameParts.Contains(findClipType))
            {
                fileClipPath = path;
                return eType;
            }
        }
        fileClipPath = string.Empty;
        return E_AnimStateName.None;

    }

    // animatorClipDataLoad
    private void LoadAnimatorClipData(AnimatorTempleData data)
    {
        List<string> filePaths = Directory.GetFiles(data.Path, "*.anim", SearchOption.AllDirectories).ToList();

        dicTempFilenameClip.Clear();

        for (E_AnimStateName stateName = E_AnimStateName.None; stateName < E_AnimStateName.Max; stateName++)
        {

            string clipPath = string.Empty;
            E_AnimStateName enumClipType = E_AnimStateName.None;
            switch (stateName)
            {
                case E_AnimStateName.Attack_001: enumClipType = FindAnimatorCliptType(stateName, "Attack1", filePaths, out clipPath); break;
                case E_AnimStateName.Attack_002: enumClipType = FindAnimatorCliptType(stateName, "Attack2", filePaths, out clipPath); break;
                case E_AnimStateName.Attack_003: enumClipType = FindAnimatorCliptType(stateName, "Attack3", filePaths, out clipPath); break;
                case E_AnimStateName.Buff_001: enumClipType = FindAnimatorCliptType(stateName, "Buff", filePaths, out clipPath); break;

                case E_AnimStateName.SkillCast_End_001: enumClipType = FindAnimatorCliptType(stateName, "SkillCastEnd", filePaths, out clipPath); break;
                case E_AnimStateName.SkillCast_Start_001: enumClipType = FindAnimatorCliptType(stateName, "SkillCastStart", filePaths, out clipPath); break;
                case E_AnimStateName.SkillCasting_001: enumClipType = FindAnimatorCliptType(stateName, "SkillCasting", filePaths, out clipPath); break;

                case E_AnimStateName.SkillRush_End_001: enumClipType = FindAnimatorCliptType(stateName, "SkillRushEnd", filePaths, out clipPath); break;
                case E_AnimStateName.SkillRush_Start_001: enumClipType = FindAnimatorCliptType(stateName, "SkillRushStart", filePaths, out clipPath); break;
                case E_AnimStateName.SkillRush_001: enumClipType = FindAnimatorCliptType(stateName, "SkillRush", filePaths, out clipPath); break;

                case E_AnimStateName.SkillLeap_End_001: enumClipType = FindAnimatorCliptType(stateName, "SkillLeapEnd", filePaths, out clipPath); break;
                case E_AnimStateName.SkillLeap_Start_001: enumClipType = FindAnimatorCliptType(stateName, "SkillLeapStart", filePaths, out clipPath); break;
                case E_AnimStateName.SkillLeap_001: enumClipType = FindAnimatorCliptType(stateName, "SkillLeap", filePaths, out clipPath); break;

                case E_AnimStateName.SkillPull_001: enumClipType = FindAnimatorCliptType(stateName, "SkillPull", filePaths, out clipPath); break;

                case E_AnimStateName.Climbing_001: enumClipType = FindAnimatorCliptType(stateName, "Climbing", filePaths, out clipPath); break;
                case E_AnimStateName.Climbing_End_001: enumClipType = FindAnimatorCliptType(stateName, "Hanging", filePaths, out clipPath); break;
                case E_AnimStateName.Die_001: enumClipType = FindAnimatorCliptType(stateName, "Die", filePaths, out clipPath); break;
                case E_AnimStateName.FallDown_001: enumClipType = FindAnimatorCliptType(stateName, "FallDown", filePaths, out clipPath); break;
                case E_AnimStateName.Gliding_001: enumClipType = FindAnimatorCliptType(stateName, "Gliding", filePaths, out clipPath); break;
                case E_AnimStateName.Hit_001: enumClipType = FindAnimatorCliptType(stateName, "Hit", filePaths, out clipPath); break;
                case E_AnimStateName.Idle_001: enumClipType = FindAnimatorCliptType(stateName, "Idle", filePaths, out clipPath); break;
                case E_AnimStateName.Jump_001: enumClipType = FindAnimatorCliptType(stateName, "Jump", filePaths, out clipPath); break;
                case E_AnimStateName.Jump_Start_001: enumClipType = FindAnimatorCliptType(stateName, "JumpStart", filePaths, out clipPath); break;
                case E_AnimStateName.Knockback_001: enumClipType = FindAnimatorCliptType(stateName, "Knockback", filePaths, out clipPath); break;
                case E_AnimStateName.Landing_001: enumClipType = FindAnimatorCliptType(stateName, "JumpEnd", filePaths, out clipPath); break;
                case E_AnimStateName.Lift_End_001: enumClipType = FindAnimatorCliptType(stateName, "PutDown", filePaths, out clipPath); break;
                case E_AnimStateName.Lift_Idle_001: enumClipType = FindAnimatorCliptType(stateName, "LiftWait", filePaths, out clipPath); break;
                case E_AnimStateName.Lift_Start_001: enumClipType = FindAnimatorCliptType(stateName, "Lift", filePaths, out clipPath); break;
                case E_AnimStateName.Lift_Walk_001: enumClipType = FindAnimatorCliptType(stateName, "HoldWalk", filePaths, out clipPath); break;
                case E_AnimStateName.Pull_001: enumClipType = FindAnimatorCliptType(stateName, "PullWalk", filePaths, out clipPath); break;
                case E_AnimStateName.PullPush_Idle_001: enumClipType = FindAnimatorCliptType(stateName, "PullWait", filePaths, out clipPath); break;
                case E_AnimStateName.Push_001: enumClipType = FindAnimatorCliptType(stateName, "PushWalk", filePaths, out clipPath); break;
                case E_AnimStateName.Run_001: enumClipType = FindAnimatorCliptType(stateName, "Run", filePaths, out clipPath); break;
                case E_AnimStateName.Skill_001: enumClipType = FindAnimatorCliptType(stateName, "Skill1", filePaths, out clipPath); break;
                case E_AnimStateName.Sliding_Ladder_001: enumClipType = FindAnimatorCliptType(stateName, "SlidingLadders", filePaths, out clipPath); break;
                case E_AnimStateName.Stun_001: enumClipType = FindAnimatorCliptType(stateName, "Stun", filePaths, out clipPath); break;
                case E_AnimStateName.Walk_001: enumClipType = FindAnimatorCliptType(stateName, "Walk", filePaths, out clipPath); break;
                case E_AnimStateName.Throw_001: enumClipType = FindAnimatorCliptType(stateName, "Throw", filePaths, out clipPath); break;
                    
                case E_AnimStateName.RideIdle_001: enumClipType = FindAnimatorCliptType(stateName, "RideIdle", filePaths, out clipPath); break;
                case E_AnimStateName.Riding_001: enumClipType = FindAnimatorCliptType(stateName, "Riding", filePaths, out clipPath); break;

                default: break;
            }

            if (enumClipType == E_AnimStateName.None) continue;

            if (data.AniClipPaths.ContainsKey(enumClipType) == false)
            {
                data.AniClipPaths.Add(enumClipType, clipPath);
            }
            else
            {
                AddLog($"같은 이름의 애니클립이 존재 합니다. 이름 확인  : {enumClipType}", Color.red);
            }
        }
    }

    private AnimatorOverrideController CreateAnimator(AnimatorTempleData folderData)
    {
        string templeFileName = $"{Path.GetFileNameWithoutExtension(folderData.Path)}{TempleFileName}";
        string templeFullPaths = $"{ folderData.Path}/{templeFileName}.overrideController";

        var templeController = AssetDatabase.LoadAssetAtPath(templeFullPaths, typeof(AnimatorOverrideController)) as AnimatorOverrideController;

        // 초기화를 위해 기존 animator을 삭제하자
        if (templeController != null)
        {
            AssetDatabase.DeleteAsset(templeFullPaths);
            templeController = null;
        }

        if (templeController == null)
        {
            var aniController = new AnimatorOverrideController();
            AssetDatabase.CreateAsset(aniController, templeFullPaths);

            templeController = AssetDatabase.LoadAssetAtPath(templeFullPaths, typeof(AnimatorOverrideController)) as AnimatorOverrideController;

            if (templeController == null) { AddLog($"신규 Animator 파일 생성 에러 : {templeFullPaths}", Color.red); }
            else { AddLog($"신규 Animator 파일 생성 : {templeFullPaths}", Color.green); }
        }
        return templeController;
    }
    // 클립을 설정
    private void DoSetAnimaotrCliptData(AnimatorOverrideController templeController, AnimatorOverrideController animatorController, AnimatorTempleData folderData, AnimatorTempleData baseTempleData, AnimatorTempleData animatorClipTargetFolderData)
    {
        AddLog($"Animator 세팅 시작 : {templeController.name}", Color.green);

        AnimatorController baseController = AssetDatabase.LoadAssetAtPath(baseControllerPath, typeof(AnimatorController)) as AnimatorController;
        templeController.runtimeAnimatorController = baseController;

        DoCopyAnimatorToTemple(templeController, animatorController);

        DoChangeAnimationClips(templeController, folderData);

        if (null != baseTempleData)
        {
            DoChangeAnimationClips(templeController, baseTempleData);
        }

        if (null != animatorClipTargetFolderData)
        {
            DoChangeAnimationClips(templeController, animatorClipTargetFolderData);
        }

        AddLog($"Animator 세팅끝 : {templeController.name}", Color.yellow);
    }

    private void DoChangeAnimationClips(AnimatorOverrideController overrideCon, AnimatorTempleData data)
    {
        if (data.AniClipPaths == null) return;
        AnimationClipOverrides clipOverrids = new AnimationClipOverrides(overrideCon.overridesCount);
        overrideCon.GetOverrides(clipOverrids);

        for (int i = 0; i < overrideCon.runtimeAnimatorController.animationClips.Length; i++)
        {
            E_AnimStateName KeyName = (E_AnimStateName)System.Enum.Parse(typeof(E_AnimStateName), overrideCon.runtimeAnimatorController.animationClips[i].name);

            if (data.AniClipPaths.ContainsKey(KeyName))
            {
                string animationClipPath = data.AniClipPaths[KeyName];
                var animationClip = AssetDatabase.LoadAssetAtPath(animationClipPath, typeof(AnimationClip)) as AnimationClip;
                clipOverrids[overrideCon.animationClips[i].name] = animationClip;
            }
        }
        overrideCon.ApplyOverrides(clipOverrids);
    }
    /// <summary>
    /// Animator-> Temple Controller로  복사하기 위한 함수
    /// </summary>
    private void DoCopyAnimatorToTemple(AnimatorOverrideController templeController, AnimatorOverrideController animatorController)
    {
        if (animatorController == null || templeController == null) return;

        AnimationClipOverrides clipOverrids = new AnimationClipOverrides(templeController.overridesCount);
        templeController.GetOverrides(clipOverrids);

        for (int i = 0; i < animatorController.animationClips.Length; i++)
        {
            var animationClip = ArrayUtility.Find(animatorController.animationClips,
            obj => obj.name == animatorController.animationClips[i].name &&
            obj.name != animatorController.runtimeAnimatorController.animationClips[i].name);

            if (animationClip != null)
            {
                clipOverrids[animatorController.runtimeAnimatorController.animationClips[i].name] = animationClip;
            }
        }

        templeController.ApplyOverrides(clipOverrids);
    }

    private string GetTargetClipList(AnimatorOverrideController templeController)
    {
        if (null == templeController) return string.Empty;

        string path = string.Empty;

        var attackClip = ArrayUtility.Find(templeController.animationClips, obj => obj.name.Contains("Attack1"));

        if (null == attackClip)
        {
            Debug.LogError($"{templeController.name}의 Attack_001의 클립이 존제 하지 않습니다.");
        }

        path = AssetDatabase.GetAssetPath(attackClip);
        var parts = path.Split('/');
        string clipPath = string.Empty;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].Contains("Animations") == true ||
            parts[i].Contains("Animation") == true) continue;
            clipPath = $"{clipPath}{parts[i]}";
            if (i < parts.Length - 2)
            {
                clipPath = $"{clipPath}/";
            }
        }
        return clipPath;
    }
    /// <summary>
    /// 완료된 컨트롤 번들 폴더로 이동 
    /// </summary>
    public void DoCopyControllerBundleFolder()
    {

    }

    private void AddLog(string context, Color? color = null)
    {
        if (color.HasValue)
        {
            LogBuilder.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>{context}</color>");
        }
        else
        {
            LogBuilder.Add(context);
        }
    }

    private List<string> GetSelectPaths(out string targetName)
    {
        Object[] Objects = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
        targetName = string.Empty;
        selectPaths.Clear();
        foreach (var Obj in Objects)
        {
            var Path = AssetDatabase.GetAssetPath(Obj);
            if (Path.Contains("Models") ||
            Path.Contains(".mat") ||
            Path.Contains(".tag") ||
            Path.Contains(".anim") ||
            Path.Contains(".fbx") ||
            Path.Contains("Animations") ||
            Path.Contains(".overrideController"))
            {
                continue;
            }
            selectPaths.Add(Path);
        }
        int selectIndex = 0;
        int partCount = selectPaths.Count;
        for (int i = 0; i < selectPaths.Count; i++)
        {
            var tempPart = selectPaths[i].Split('/');
            if (partCount > tempPart.Length)
            {
                partCount = tempPart.Length;
                selectIndex = i;
            }
        }
        if (selectPaths.Count != 0)
        {
            targetName = selectPaths[selectIndex];
        }
        return selectPaths;
    }

    private class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimationClipOverrides(int capacity) : base(capacity) { }

        public AnimationClip this[string name]
        {
            get { return this.Find(x => x.Key.name.Equals(name)).Value; }
            set
            {
                int index = this.FindIndex(x => x.Key.name.Equals(name));
                if (index != -1)
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }

}

