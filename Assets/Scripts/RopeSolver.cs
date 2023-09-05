using System;
using UnityEngine;

/// <summary>
/// アルゴリズムの選択と実行サイクルを管理する
/// </summary>
[RequireComponent(typeof(RopeObject))]
public class RopeSolver : MonoBehaviour
{
	public enum Algorithm
	{
		PBD,
		XPBD,
	}

	[SerializeField] bool _playOnAwake = true;
	[SerializeField, HideInInspector] Algorithm _algorithm = Algorithm.PBD;
	[SerializeReference] ISimulationParameter _parameter;

	public Algorithm algorithm => _algorithm;

	bool _initialized;
	RopeObject _ropeObject;
	ISimulator _simulator;


	void Awake()
	{
		if (_playOnAwake) { Initialize(); }
	}

	void FixedUpdate()
	{
		if (_playOnAwake) { UpdateManually(Time.deltaTime); }
	}


	public void Initialize()
	{
		if (_initialized) { return; }
		_initialized = true;

		_ropeObject = GetComponent<RopeObject>();
		_simulator = GenerateSimulator(_ropeObject, _parameter);
	}

	public void UpdateManually(float dt)
	{
		_simulator?.Step(dt);
	}

	public void SetAlgorithm(Algorithm algorithm)
	{
		if (_initialized)
		{
			Debug.LogWarning("初期化後にアルゴリズムを変更することはできません");
			return;
		}

		_algorithm = algorithm;

		switch (algorithm)
		{
			case Algorithm.PBD:
				_parameter = new PbdSimulator.Parameter();
				break;
			case Algorithm.XPBD:
				_parameter = new XpbdSimulator.Parameter();
				break;
		}
	}

	ISimulator GenerateSimulator(RopeObject ropeObject, ISimulationParameter parameter)
	{
		switch (_algorithm)
		{
			case Algorithm.PBD:
				return new PbdSimulator((PbdSimulator.Parameter)parameter, ropeObject);
			case Algorithm.XPBD:
				return new XpbdSimulator((XpbdSimulator.Parameter)parameter, ropeObject);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RopeSolver))]
public class RopeSolverEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		RopeSolver ropeSolver = (RopeSolver)target;

		UnityEditor.EditorGUILayout.Space(30);

		UnityEditor.EditorGUI.BeginChangeCheck();
		var algorithm = (RopeSolver.Algorithm)UnityEditor.EditorGUILayout.EnumPopup("Algorithm", ropeSolver.algorithm);
		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			ropeSolver.SetAlgorithm(algorithm);
			UnityEditor.EditorUtility.SetDirty(target);
		}
	}
}
#endif
