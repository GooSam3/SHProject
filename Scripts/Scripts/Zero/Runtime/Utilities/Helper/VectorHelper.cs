using UnityEngine;

/// <summary>
/// Vector헬퍼
/// </summary>
public static class VectorHelper
{
    public static float Axis2Angle(Vector2 value, bool inDegree = true)
    {
        float angle = Mathf.Atan2(value.x, value.y);

        if (inDegree)
            return angle * Mathf.Rad2Deg;
        else
            return angle;
    }

    public static float Axis3Angle(Vector3 value, bool inDegree = true)
    {
        return Axis2Angle(new Vector2(value.x, value.z), inDegree);
    }

    public static float XZSqrMagnitude(Vector3 v1, Vector3 v2)
    {
        float dx = v2.x - v1.x;
        float dz = v2.z - v1.z;
        return dx * dx + dz * dz;
    }

    public static Vector3 XZForward(Vector3 from, Vector3 to)
    {
        Vector3 forward = to - from;
        forward.y = 0;

        return forward.normalized;
    }

    public static float XZMagnitude(Vector3 v1, Vector3 v2)
    {
        v1.y = 0;
        v2.y = 0;
        return Vector3.Magnitude(v1 - v2);
    }

    public static float XYSqrMagnitude(Vector3 v1, Vector3 v2)
    {
        float dx = v2.x - v1.x;
        float dy = v2.y - v1.y;
        return dx * dx + dy * dy;
    }

    /// <summary> 중심점 기준으로 원안에 랜덤 위치를 리턴해준다 </summary>
    public static Vector3 RandomCircleXZ(Vector3 centerPos, float radius)
    {
        Vector2 randPos = Random.insideUnitCircle * radius;
        return centerPos + new Vector3(randPos.x, 0, randPos.y);
    }

    public static Quaternion RandomAxisY()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    /// <summary> {x}:{y}:{z} 형식으로 문자열 생성 </summary>
    public static string ToString(Vector3 vec)
    {
        return $"{vec.x.ToString("N2")}:{vec.y.ToString("N2")}:{vec.z.ToString("N2")}";
    }

    /// <summary> {x}:{y}:{z} 형식의 문자열에서 Vector3로 변환된 문자열 리턴 </summary>
    public static Vector3 ToVector3(string strVec)
    {
        string[] vec3 = strVec.Split(':');
        if (vec3.Length == 3)
        {
            return new Vector3(float.Parse(vec3[0]), float.Parse(vec3[1]), float.Parse(vec3[2]));
        }
        else
            return Vector3.zero;
    }

    public static float Angle(Vector3 fwd, Vector3 targetDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        if (AngleDir(fwd, targetDir, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
            return angle;
        }
        else
            return angle;
    }

    public static int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }

    //Calculate signed angle (ranging from -180 to +180) between '_vector_1' and '_vector_2';
    public static float GetAngle(Vector3 _vector1, Vector3 _vector2, Vector3 _planeNormal)
    {
        //Calculate angle and sign;
        float _angle = Vector3.Angle(_vector1, _vector2);
        float _sign = Mathf.Sign(Vector3.Dot(_planeNormal, Vector3.Cross(_vector1, _vector2)));

        //Combine angle and sign;
        float _signedAngle = _angle * _sign;

        return _signedAngle;
    }

    //Returns the length of the part of a vector that points in the same direction as '_direction' (i.e., the dot product);
    public static float GetDotProduct(Vector3 _vector, Vector3 _direction)
    {
        //Normalize vector if necessary;
        if (_direction.sqrMagnitude != 1)
            _direction.Normalize();

        float _length = Vector3.Dot(_vector, _direction);

        return _length;
    }

    //Remove all parts from a vector that are pointing in the same direction as '_direction';
    public static Vector3 RemoveDotVector(Vector3 _vector, Vector3 _direction)
    {
        //Normalize vector if necessary;
        if (_direction.sqrMagnitude != 1)
            _direction.Normalize();

        float _amount = Vector3.Dot(_vector, _direction);

        _vector -= _direction * _amount;

        return _vector;
    }

    //Extract and return parts from a vector that are pointing in the same direction as '_direction';
    public static Vector3 ExtractDotVector(Vector3 _vector, Vector3 _direction)
    {
        //Normalize vector if necessary;
        if (_direction.sqrMagnitude != 1)
            _direction.Normalize();

        float _amount = Vector3.Dot(_vector, _direction);

        return _direction * _amount;
    }

    //Rotate a vector onto a plane defined by '_planeNormal'; 
    public static Vector3 RotateVectorOntoPlane(Vector3 _vector, Vector3 _planeNormal, Vector3 _upDirection)
    {
        //Calculate rotation;
        Quaternion _rotation = Quaternion.FromToRotation(_upDirection, _planeNormal);

        //Apply rotation to vector;
        _vector = _rotation * _vector;

        return _vector;
    }

    //Project a point onto a line defined by '_lineStartPosition' and '_lineDirection';
    public static Vector3 ProjectPointOntoLine(Vector3 _lineStartPosition, Vector3 _lineDirection, Vector3 _point)
    {
        //Caclculate vector pointing from '_lineStartPosition' to '_point';
        Vector3 _projectLine = _point - _lineStartPosition;

        float dotProduct = Vector3.Dot(_projectLine, _lineDirection);

        return _lineStartPosition + _lineDirection * dotProduct;
    }

    //Increments a float value from '_currentValue' toward '_targetValue', using '_speed' and '_deltaTime';
    public static float IncrementValueTowardTarget(float _currentValue, float _speed, float _deltaTime, float _targetValue)
    {
        if (_currentValue == _targetValue)
            return _currentValue;

        //Calculate sign and remaining distance;
        float _sign = Mathf.Sign(_targetValue - _currentValue);
        float _remainingDistance = Mathf.Abs(_targetValue - _currentValue);

        if (Mathf.Abs(_speed * Time.deltaTime * _sign) > _remainingDistance)
            return _targetValue;
        else
            return _currentValue + _speed * Time.deltaTime * _sign;
    }

    //Increments a vector's length toward '_targetLength', using '_speed' and '_deltaTime';
    public static Vector3 IncrementVectorLengthTowardTargetLength(Vector3 _currentVector, float _speed, float _deltaTime, float _targetLength)
    {
        //Calculate length;
        float _currentLength = _currentVector.magnitude;
        Vector3 _normalizedVector = _currentVector / _currentLength;

        if (_currentLength == _targetLength)
            return _currentVector;

        //Increment length;
        float _newLength = IncrementValueTowardTarget(_currentLength, _speed, _deltaTime, _targetLength);

        return _normalizedVector * _newLength;
    }
}
