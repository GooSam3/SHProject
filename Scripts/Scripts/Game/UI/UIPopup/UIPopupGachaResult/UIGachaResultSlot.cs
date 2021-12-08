using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGachaResultSlot : MonoBehaviour
{
    private const string KEY_TURN = "Start_001";
    private const string KEY_IDLE = "Idle_001";

    [SerializeField] private Image gradeBG;
    [SerializeField] private Image charIcon;
    [SerializeField] private Text charName;

    [SerializeField] private Animator anim;

    [SerializeField] private List<ParticleSystem> vfxLoop;
    [SerializeField] private List<ParticleSystem> vfxOnShow;

    public bool turnState = false;

    public bool activeState = false;

    private int grade = 0;

    public void SetDefault()
    {
        if (turnState)
            SetTurn(false);
    }

    public void ResetFX()
    {
        for (int i = 0; i < vfxLoop.Count; i++)
        {
            if (grade - 1 == i)
            {
                if (vfxLoop[i].gameObject.activeSelf == false)
                {
                    vfxLoop[i].gameObject.SetActive(true);
                    vfxLoop[i].Play();
                }
                continue;
            }

            if (vfxLoop[i].gameObject.activeSelf)
                vfxLoop[i].gameObject.SetActive(false);
        }

        vfxOnShow.ForEach(item => item.gameObject.SetActive(false));
    }

    public void PlayVFX(bool playTurnOnFX)
    {
        CancelInvoke();

        ResetFX();

        if (playTurnOnFX)
        {
            var sec = (anim.runtimeAnimatorController as AnimatorOverrideController)[KEY_TURN].length;

            this.Invoke(nameof(OnTurnEnd), sec);
        }
    }

    private void OnTurnEnd()
    {
        if (grade - 1 < 0 || grade - 1 >= vfxOnShow.Count)
            return;

        vfxOnShow[grade - 1].gameObject.SetActive(true);
        vfxOnShow[grade - 1].Play();
    }

    public void SetTurn(bool b)
    {
        CancelInvoke();
        turnState = b;
        anim.Rebind();

        if (b)
            anim.SetTrigger(KEY_TURN);
        else
            anim.SetTrigger(KEY_IDLE);

      //  PlayVFX(b);
    }

    public void SetState(bool b)
    {
        activeState = b;
        this.gameObject.SetActive(b);
    }

    public void SetData(E_PetChangeViewType type, uint tid, bool _turnState)
    {
        SetState(true);

        switch (type)
        {
            case E_PetChangeViewType.Change:
                SetChangeData(tid);
                break;
            case E_PetChangeViewType.Pet:
            case E_PetChangeViewType.Ride:
                SetPetRideData(tid);
                break;
        }

        SetTurn(_turnState);
    }

    private void SetChangeData(uint tid)
    {
        if (DBChange.TryGet(tid, out var table) == false)
            return;

        gradeBG.sprite = UICommon.GetGradeSprite(table.Grade);
        charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
        charName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(table.ChangeTextID),table.Grade);

        grade = table.Grade;
    }

    private void SetPetRideData(uint tid)
    {
        if (DBPet.TryGet(tid, out var table) == false)
            return;

        gradeBG.sprite = UICommon.GetGradeSprite(table.Grade);
        charIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
        charName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(table.PetTextID), table.Grade);


        grade = table.Grade;
    }
}
