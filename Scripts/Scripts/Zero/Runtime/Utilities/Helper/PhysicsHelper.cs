using System.Collections.Generic;
using UnityEngine;

public class PhysicsHelper
{
    public class RaycastHitDistanceComparer : IComparer<RaycastHit>
    {
        private static RaycastHitDistanceComparer _default;
        public static RaycastHitDistanceComparer Default
        {
            get
            {
                if (_default == null)
                    _default = new RaycastHitDistanceComparer();
                return _default;
            }
        }

        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}