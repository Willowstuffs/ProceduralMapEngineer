using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralMapPipeline.Core;

/// <summary>
/// Zarządza potokiem generacji. Przechowuje kolekcję kroków algorytmicznych
/// i wykonuje je sekwencyjnie przekazując dane z jednego kroku do kolejnego.
/// </summary>
public class PipelineManager
{
	private readonly List<GenerationStep> _steps = new();

	/// <summary> Dodaje nowy krok do potoku. </summary>
	public void AddStep(GenerationStep step)
	{
		_steps.Add(step);
	}

	/// <summary> Czyszczenie potoku.</summary>
	public void ClearSteps() => _steps.Clear();

	/// <summary>
	/// Uruchamia wszystkie kroki potoku sekwencyjnie.
	/// </summary>
	/// <returns>Ostatecznie przetworzone dane mapy.</returns>
	public MapData ExecutePipeline(int width, int height, ulong globalSeed)
	{
		//Tworzy czystą mapę początkową
		MapData currentData = new MapData(width, height);

		//Wykonuje każdy krok po kolei, nadpisując referencję do danych
		foreach (var step in _steps)
		{
			//todo: stoper, benchmarki
			currentData = step.Execute(currentData, globalSeed);
		}

		return currentData;
	}
}
