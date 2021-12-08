using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;

[CreateAssetMenu(fileName = "BuildScriptPackedCustom.asset", menuName = "Addressables/Content Builders/ ZCustom Build Script")]
public class BuildScriptPackedModeCustom : BuildScriptPackedMode
{
    [SerializeField] string DataSavePath = "Assets/AddressableAssetsData";

    public override string Name
    {
        get
        {
            return "ZCustom Build Script";
        }
    }


    //---------------------------------------------------------------------
    protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
    {
        TResult result = base.DoBuild<TResult>(builderInput, aaContext);
        // 작업이 전부 끝나면 어드레서블 구동 파일을 특정 디랙토리로 복사해준다. SVN 관리용. 해당 파일은 빌드시 APK에 삽입되어야 어드레서블이 구동된다. 어드레서블에 의해 StreamingAssets/aa로 예약되어 있으니 주의
        CopyToAddressableData();
        return result;
    }

    //--------------------------------------------------------------------
    private void CopyToAddressableData()
	{
        if (!Directory.Exists(DataSavePath))
            Directory.CreateDirectory(DataSavePath);

        string sourceFile = $"{Addressables.BuildPath}/link.xml";
        string destFile = $"{ExtractCustomBuildPath()}/link.xml";

        if (File.Exists(sourceFile))
		{
            File.Copy(sourceFile, destFile, true);
        }

        sourceFile = $"{Addressables.BuildPath}/settings.json";
        destFile = $"{ExtractCustomBuildPath()}/settings.json";

        if (File.Exists(sourceFile))
        {
            File.Copy(sourceFile, destFile, true);
        }
    }

    private string ExtractCustomBuildPath()
    {
        return string.Format("{0}/{1}", DataSavePath, PlatformMappingService.GetPlatform());
    }

}