using UnityEngine;

public class ParticleObject : MonoBehaviour
{
	[SerializeField] float _mass = 1f;

	public float mass => _mass;
	Transform _transform;


	public Transform GetTransform()
	{
		if (_transform == null) { _transform = transform; }
		return _transform;
	}

	public void SetPosition(Vector3 position)
	{
		GetTransform().position = position;
	}

	public Vector3 GetPosition()
	{
		return GetTransform().position;
	}
}
