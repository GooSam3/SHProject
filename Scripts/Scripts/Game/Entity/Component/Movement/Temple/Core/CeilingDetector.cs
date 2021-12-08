using UnityEngine;

//This (optional) component can be added to a gameobject that also has a 'AdvancedWalkerController' attached;
//It will continuously check all collision detected by the internal physics calculation;
//If a collision qualifies as the character "hitting a ceiling" (based on surface normal), the result will be stored;
//The 'AdvancedWalkerController' then can use that information to react to ceiling hits; 
public class CeilingDetector : MonoBehaviour
{

    private bool mCeilingWasHit = false;

    //Angle limit for ceiling hits;
    [SerializeField]
    private float mCeilingAngleLimit = 10f;

    //Ceiling detection methods;
    //'OnlyCheckFirstContact' - Only check the very first collision contact. This option is slightly faster but less accurate than the other two options.
    //'CheckAllContacts' - Check all contact points and register a ceiling hit as long as just one contact qualifies.
    //'CheckAverageOfAllContacts' - Calculate an average surface normal to check against.
    public enum E_CeilingDetectionMethod
    {
        OnlyCheckFirstContact,
        CheckAllContacts,
        CheckAverageOfAllContacts
    }

    public E_CeilingDetectionMethod CeilingDetectionMethod;

    //If enabled, draw debug information to show hit positions and hit normals;
    public bool IsInDebugMode = false;
    //How long debug information is drawn on the screen;
    private float mDebugDrawDuration = 2.0f;

    private Transform mTransform;

    public void Initialize()
    {
        mTransform = transform;
    }

    void OnCollisionEnter(Collision _collision)
    {
        CheckCollisionAngles(_collision);
    }

    void OnCollisionStay(Collision _collision)
    {
        CheckCollisionAngles(_collision);
    }

    //Check if a given collision qualifies as a ceiling hit;
    private void CheckCollisionAngles(Collision _collision)
    {
        float _angle = 0f;

        if (CeilingDetectionMethod == E_CeilingDetectionMethod.OnlyCheckFirstContact)
        {
            //Calculate angle between hit normal and character;
            _angle = Vector3.Angle(-mTransform.up, _collision.contacts[0].normal);

            //If angle is smaller than ceiling angle limit, register ceiling hit;
            if (_angle < mCeilingAngleLimit)
                mCeilingWasHit = true;

            //Draw debug information;
            if (IsInDebugMode)
                Debug.DrawRay(_collision.contacts[0].point, _collision.contacts[0].normal, Color.red, mDebugDrawDuration);
        }
        if (CeilingDetectionMethod == E_CeilingDetectionMethod.CheckAllContacts)
        {
            for (int i = 0; i < _collision.contacts.Length; i++)
            {
                //Calculate angle between hit normal and character;
                _angle = Vector3.Angle(-mTransform.up, _collision.contacts[i].normal);

                //If angle is smaller than ceiling angle limit, register ceiling hit;
                if (_angle < mCeilingAngleLimit)
                    mCeilingWasHit = true;

                //Draw debug information;
                if (IsInDebugMode)
                    Debug.DrawRay(_collision.contacts[i].point, _collision.contacts[i].normal, Color.red, mDebugDrawDuration);
            }
        }
        if (CeilingDetectionMethod == E_CeilingDetectionMethod.CheckAverageOfAllContacts)
        {
            for (int i = 0; i < _collision.contacts.Length; i++)
            {
                //Calculate angle between hit normal and character and add it to total angle count;
                _angle += Vector3.Angle(-mTransform.up, _collision.contacts[i].normal);

                //Draw debug information;
                if (IsInDebugMode)
                    Debug.DrawRay(_collision.contacts[i].point, _collision.contacts[i].normal, Color.red, mDebugDrawDuration);
            }

            //If average angle is smaller than the ceiling angle limit, register ceiling hit;
            if (_angle / ((float)_collision.contacts.Length) < mCeilingAngleLimit)
                mCeilingWasHit = true;
        }
    }

    //Return whether ceiling was hit during the last frame;
    public bool HitCeiling()
    {
        return mCeilingWasHit;
    }

    //Reset ceiling hit flags;
    public void ResetFlags()
    {
        mCeilingWasHit = false;
    }
}
