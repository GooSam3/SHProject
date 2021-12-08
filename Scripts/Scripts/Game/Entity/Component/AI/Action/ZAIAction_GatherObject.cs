using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions
{
    public class ZAIAction_GatherObject : ZAIActionBase
    {
        public bool IsQuest = true;

        protected override string info
        {
            get { return IsQuest ? "퀘스트 채집물 채집" : "채집물 채집"; }
        }

        protected override void OnExecute()
        {
            if (!(agent is ZPawnMyPc myEntity))
            {
                EndAction(false);
                return;
            }

            uint searchTid = IsQuest ? blackboard.GetVariableValue<uint>(ZBlackbloardKey.TargetTid) : 0;
            ZObject target = ZPawnTargetHelper.ScanGatherObject(myEntity, searchTid);

            if (target == null)
            {
                EndAction(false);
                return;
            }

            myEntity.ObjectGathering(target);
            EndAction(true);
        }
    }
}