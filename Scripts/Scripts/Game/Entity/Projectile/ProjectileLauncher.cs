using GameDB;
using UnityEngine;

public static class ProjectileLauncher
{
    /// <summary> TODO :: 발사체 관련 처리해야함.  </summary>
    public static ProjectileBase Fire(ZPawn self, uint targetEntityId, uint skillId, Vector3 dir)
    {
        return Fire(self, ZPawnManager.Instance.GetEntity(targetEntityId), skillId, dir);
    }

    /// <summary> TODO :: 발사체 관련 처리해야함.  </summary>
    public static ProjectileBase Fire(ZPawn self, EntityBase targetEntity, uint skillId, Vector3 dir)
    {
        GameObject go = new GameObject($"Projectile_Skill_{skillId}");

        go.transform.position = self.GetSocket(E_ModelSocket.Projectile).position;

        ProjectileBase projectile = go.AddComponent<ProjectileBase>();

        projectile.Fire(self, targetEntity, skillId, dir);

        return projectile;
    }
}
