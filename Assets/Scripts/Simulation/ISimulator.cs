using System;

/// <summary>
/// アルゴリズムごとに実装する
/// </summary>
public interface ISimulator
{
	void Step(float dt);
}

/// <summary>
/// アルゴリズムごとに必要なパラメータを定義する
/// </summary>
public interface ISimulationParameter { }
