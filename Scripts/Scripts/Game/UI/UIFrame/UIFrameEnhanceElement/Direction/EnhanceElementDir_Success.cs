using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceElementDir_Success : ElementDirectionControllerBase 
{
    public override bool SupportSpeedControl => true;
    public override bool AutoStopOnResourceDeactived => true;

    public override void Play(ElementDirectionHandler.DirectionParam param)
    {
        base.Play(param);
    }

    public override void Reset()
    {
        base.Reset();
        base.SetResourceLocalPos(Vector2.zero);
    }
    public override void Update()
    {
        base.Update();
    }

    public override void Stop()
    {
        base.Stop();
    }

    public override void ProcessUpdate()
    {

    }
}
