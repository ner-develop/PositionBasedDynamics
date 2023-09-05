using System;
using System.Linq;
using UnityEngine;

public class PbdSimulator : ISimulator
{
	[Serializable]
	public class Parameter : ISimulationParameter
	{
		[Range(0, 1)]
		public float stiffness = 1;
		public int iterations = 10;
		public Vector3 gravity = Physics.gravity;
	}

	class Vertex
	{
		public bool isFixed;
		public Vector3 x;
		public Vector3 p;
		public Vector3 v;
		public float w;
		public Vector3 initialPosition;
	}


	readonly Parameter _parameter;
	readonly (ParticleObject particle, Vertex vertex)[] _vertexes;


	public PbdSimulator(Parameter parameter, RopeObject ropeObject)
	{
		ropeObject.gizmosLabel = $"PBD\niteration:{parameter.iterations}";
		ropeObject.gizmosColor = Color.green;

		_parameter = parameter;
		_vertexes = ropeObject.particles.Select(p => (p, new Vertex())).ToArray();


		// (1) ~ (3)
		for (var idx = 0; idx < _vertexes.Length; idx++)
		{
			var (particle, vertex) = _vertexes[idx];
			vertex.isFixed = idx == 0;
			vertex.initialPosition = particle.transform.position;

			vertex.x = vertex.initialPosition;
			vertex.v = Vector3.zero;
			vertex.w = 1f / particle.mass;
		}
	}

	public void Step(float dt)
	{
		// 固定点の位置をデータに反映
		foreach (var (particle, vertex) in _vertexes)
		{
			if (!vertex.isFixed) { continue; }
			vertex.x = particle.transform.position;
		}

		Simulate(dt);

		// Transformに反映
		foreach (var (particle, vertex) in _vertexes)
		{
			particle.SetPosition(vertex.x);
		}
	}

	void Simulate(float dt)
	{
		// (5)
		foreach (var (_, vertex) in _vertexes)
		{
			if (vertex.isFixed) { continue; }
			vertex.v = vertex.v + dt * _parameter.gravity;
		}

		// (6) スキップ
		// Damp Velocities

		// (7)
		foreach (var (_, vertex) in _vertexes)
		{
			vertex.p = vertex.x + dt * vertex.v;
		}

		// (8) スキップ
		// generateCollisionConstraints

		// (9) ~ (11)
		float k = 1 - Mathf.Pow(1 - Mathf.Clamp01(_parameter.stiffness), 1f / _parameter.iterations);
		for (int iterationCount = 0; iterationCount < _parameter.iterations; iterationCount++)
		{
			ProjectConstraints(k);
		}

		// (12) ~ (15)
		foreach (var (_, vertex) in _vertexes)
		{
			if (vertex.isFixed) { continue; }
			vertex.v = (vertex.p - vertex.x) / dt;
			vertex.x = vertex.p;
		}

		// (16) スキップ
		// velocityUpdate
	}

	void ProjectConstraints(float k)
	{
		// 拘束
		for (int i = 0; i < _vertexes.Length - 1; i++)
		{
			var (_, vertex1) = _vertexes[i];
			var (_, vertex2) = _vertexes[i + 1];

			var p1_p2 = vertex1.p - vertex2.p;
			var p1_p2_length = p1_p2.magnitude;
			var w1 = vertex1.w;
			var w2 = vertex2.w;

			var gradC = p1_p2 / p1_p2_length;
			var C = Constraint(vertex1, vertex2);
			var s = C / ((w1 + w2) * Vector3.Dot(gradC, gradC));

			var Δp1 = -s * w1 * gradC;
			var Δp2 = +s * w2 * gradC;

			vertex1.p += Δp1 * k;
			vertex2.p += Δp2 * k;
		}
	}

	float Constraint(Vertex v1, Vertex v2)
	{
		float d = (v1.initialPosition - v2.initialPosition).magnitude;
		return (v1.p - v2.p).magnitude - d;
	}
}
