using UnityEngine;

public class Handle : MonoBehaviour
{
	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(transform.position, 0.2f * Vector3.one);
	}
}
