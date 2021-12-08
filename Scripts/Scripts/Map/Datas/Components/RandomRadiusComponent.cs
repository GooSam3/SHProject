using UnityEngine;

public class RandomRadiusComponent : MonoBehaviour
{
	public MovementData_RandomRadius Data;

	public void Assign(MovementData_RandomRadius _data)
	{
		this.Data = _data;
	}
}
