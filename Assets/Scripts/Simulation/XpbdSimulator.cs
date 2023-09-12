using System;
using System.Linq;
using UnityEngine;

public class XpbdSimulator : ISimulator
{
	[Serializable]
	public class Parameter : ISimulationParameter
	{
		public float flexibility = 0.001f;
		public int iterations = 10;
		public Vector3 gravity = Physics.gravity;
	}

	class Particle
	{
		public bool isFixed;
		public Vector3 x;
		public Vector3 xi;
		public Vector3 v;
		public float m;
		public float w;
		public Vector3 initialPosition;
	}

	class Constraint
	{
		// λ = -α~^{-1} C(x) でCに対応
		public float λ;
		public float α;

		public readonly Particle p1;
		public readonly Particle p2;
		public readonly float d;

		public Constraint(Particle p1, Particle p2)
		{
			this.p1 = p1;
			this.p2 = p2;
			d = (p2.x - p1.x).magnitude;
		}

		public float ComputeΔλ(float dt)
		{
			var vector = p2.xi - p1.xi;
			var distance = vector.magnitude;
			var C = distance - d;
			var αTilda = α / (dt * dt);
			var ΔC = vector / distance;
			float invM = p1.w + p2.w;
			return (C - αTilda * λ) / (Vector3.Dot(ΔC, invM * ΔC) + αTilda);
		}

		public Vector3 ComputeΔx(float Δλ)
		{
			var ΔC = (p2.xi - p1.xi).normalized;
			return ΔC * Δλ;
		}
	}



	readonly Parameter _parameter;
	readonly (ParticleObject particleObject, Particle particle)[] _particles;
	readonly Constraint[] _constraints;


	public XpbdSimulator(Parameter parameter, RopeObject ropeObject)
	{
		ropeObject.gizmosLabel = $"XPBD\niteration: {parameter.iterations}";
		ropeObject.gizmosColor = Color.blue;

		_parameter = parameter;
		_particles = ropeObject.particles.Select(p => (p, new Particle())).ToArray();

		for (int i = 0; i < _particles.Length; i++)
		{
			var (particleObject, particle) = _particles[i];
			particle.isFixed = i == 0;
			particle.initialPosition = particleObject.GetPosition();

			particle.x = particle.initialPosition;
			particle.m = particleObject.mass;
			particle.w = 1f / particle.m;
			particle.v = Vector3.zero;
		}

		_constraints = new Constraint[_particles.Length - 1];
		for (int i = 0; i < _particles.Length - 1; i++)
		{
			_constraints[i] = new Constraint(_particles[i].particle, _particles[i + 1].particle);
		}
	}

	public void Step(float dt)
	{
		// 固定点の位置をデータに反映
		foreach (var (particleObject, particle) in _particles)
		{
			if (!particle.isFixed) { continue; }
			particle.x = particleObject.GetPosition();
		}

		Simulate(dt);

		// Transformに反映
		foreach (var (particleObject, particle) in _particles)
		{
			particleObject.SetPosition(particle.x);
		}
	}

	void Simulate(float dt)
	{
		// x~ = x + Δt v + Δt^2 M^-1 f_ext(x)
		foreach (var (_, p) in _particles)
		{
			// predict position
			var xTilda = p.x + dt * p.v;

			// initialize solve;
			p.xi = xTilda;
		}

		foreach (var c in _constraints)
		{
			// initialize multipliers
			c.λ = 0;
			c.α = _parameter.flexibility;
		}


		int i = 0;
		while (i < _parameter.iterations)
		{
			foreach (var c in _constraints)
			{
				// compute
				var Δλ = c.ComputeΔλ(dt);
				var Δx = c.ComputeΔx(Δλ);

				// update
				c.λ = c.λ + Δλ;
				c.p1.xi = c.p1.xi + c.p1.w * Δx;
				c.p2.xi = c.p2.xi - c.p2.w * Δx;
			}

			i = i + 1;
		}


		foreach (var (_, p) in _particles)
		{
			if (p.isFixed) { continue; }
			// update velocity
			p.v = (p.xi - p.x) / dt + _parameter.gravity * dt;
			// update position
			p.x = p.xi;
		}
	}
}

