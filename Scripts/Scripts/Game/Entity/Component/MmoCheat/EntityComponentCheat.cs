using UnityEngine;

/// <summary> entity stat 관련 처리 </summary>
public class EntityComponentCheat : EntityComponentBase<EntityBase>
{
    private string mCheatMessage = "";

    private void OnGUI()
    {
        GUILayout.Space(30f);
        GUILayout.Label("Cheat Message (/키워드 파라미터)");        
        mCheatMessage = GUILayout.TextField(mCheatMessage, GUILayout.Height(20));

        if (false == string.IsNullOrEmpty(mCheatMessage) && mCheatMessage.StartsWith("/"))
        {
            if (GUILayout.Button($"Cheat : {mCheatMessage}"))
            {
                ZMmoManager.Instance.Field.REQ_MapChat(Owner.EntityId, $"{mCheatMessage}");
                mCheatMessage = string.Empty;
            }
        }
    }
}
