using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パーティクル管理と描画を行うだけ
/// </summary>
public class RopeObject : MonoBehaviour
{
	[SerializeField] List<ParticleObject> _particles = new();

	public IReadOnlyList<ParticleObject> particles => _particles;
	public Color gizmosColor { get; set; } = Color.green;
	public string gizmosLabel { get; set; }



	public void CollectParticles()
	{
		_particles.Clear();
		_particles.AddRange(GetComponentsInChildren<ParticleObject>());
	}


#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (_particles.Count == 0) { return; }

		const float radius = 0.1f;
		UnityEditor.Handles.Label(
			_particles[0].transform.position + 3.5f * radius * Vector3.up,
			gizmosLabel
		);

		for (int i = 0; i < _particles.Count; i++)
		{
			Gizmos.color = gizmosColor;
			Gizmos.DrawSphere(_particles[i].transform.position, radius);
			if (i < _particles.Count - 1)
			{
				Gizmos.DrawLine(_particles[i].transform.position, _particles[i + 1].transform.position);
			}
		}
	}
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RopeObject))]
public class RopeEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		var rope = (RopeObject)target;
		if (GUILayout.Button("Particle収集"))
		{
			rope.CollectParticles();
			UnityEditor.EditorUtility.SetDirty(target);
		}
	}
}
#endif
