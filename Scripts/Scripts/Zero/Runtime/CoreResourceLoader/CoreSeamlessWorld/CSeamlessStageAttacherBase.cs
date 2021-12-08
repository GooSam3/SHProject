using UnityEngine;

// AdditiveScene에 설치하는 컴포넌트로 충돌체를 통한 이벤트를 생성한다

[RequireComponent(typeof(BoxCollider))]
abstract public class CSeamlessStageAttacherBase : CSeamlessStageBase
{
    [SerializeField]
    private bool StageRoot = false;  public bool pStageRoot { get { return StageRoot; } }
}
