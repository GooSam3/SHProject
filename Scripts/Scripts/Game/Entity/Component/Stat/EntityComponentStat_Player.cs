/// <summary> entity stat 관련 처리 </summary>
public class EntityComponentStat_Player : EntityComponentStatBase
{
    protected override void OnInitializeComponentImpl()
    {
        if (null != ZPawnManager.Instance.MyCharInfo)
        {
            var charInfo = ZPawnManager.Instance.MyCharInfo;

            foreach (var abil in charInfo.m_dicAbility)
            {
                SetAbility(abil.Key, abil.Value);
            }

            foreach (var skillAbil in charInfo.m_dicSkillAbility)
            {
                uint skillTid = skillAbil.Key;

                foreach (var abil in skillAbil.Value)
                {
                    SetSkillAbility(skillTid, abil.Key, abil.Value);                    
                }
            }
        }

        base.OnInitializeComponentImpl();
    }
}
