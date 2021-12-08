using UnityEngine;

/// <summary> Dirty Flag 처리 </summary>
public class ZDirty
{
    public float LerpTime = 1f;
    public bool IsDirty
    {
        get
        {
            return bDirty;
        }
        set
        {
            bDirty = value;
            ElapsedLerpTime = 0f;
            StartValue = CurrentValue;
        }
    }
    public float GoalValue;
    public float CurrentValue;

    private bool bDirty;
    private float ElapsedLerpTime;
    private float StartValue;

    private float Tolerance = 0.01f;

    public ZDirty(float lerpTime, float tolerance = 0.01f)
    {
        LerpTime = lerpTime;
        Tolerance = tolerance;
    }

    public bool Approximately()
    {
        //return Mathf.Approximately(GoalValue, CurrentValue);
        return Mathf.Abs(GoalValue - CurrentValue) <= 0.01f;
    }

    public bool Update()
    {
        if (false == IsDirty)
            return false;

        if (LerpTime > 0)
        {
            ElapsedLerpTime += Time.deltaTime;
            float perc = ElapsedLerpTime / LerpTime;

            CurrentValue = Mathf.Lerp(StartValue, GoalValue, perc);
        }
        else
        {
            CurrentValue = Mathf.Lerp(CurrentValue, GoalValue, Time.deltaTime);
        }

        if (Approximately())
        {
            bDirty = false;
            CurrentValue = GoalValue;
        }

        return true;
    }
}
