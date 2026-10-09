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
	
	//Właściwość do odczytywania obecnego kroku
	public IReadOnlyList<GenerationStep> Steps => _steps.AsReadOnly();

	/// <summary> Dodaje nowy krok do potoku. </summary>
	public void AddStep(GenerationStep step)
	{
		_steps.Add(step);
	}
	
	/// <summary> Usuwanie kroku ze stosu </summary>
	public void RemoveLastStep()
	{
		if (_steps.Count > 0)
			_steps.RemoveAt(_steps.Count - 1);
	}

	/// <summary> Czyszczenie potoku.</summary>
	public void ClearSteps() => _steps.Clear();

	/// <summary> Sprawdza, czy obecna konfiguracja potoku jest logicznie poprawna.
	/// Zwraca false i komunikat błędu, jeśli wykryje anomalię. </summary>
	public bool ValidatePipeline(out string errorMessage)
	{
		errorMessage = string.Empty;

		if (_steps.Count == 0)
		{
			errorMessage = "Potok jest pusty. Dodaj przynajmniej jeden krok (Generator).";
			return false;
		}

		//Pierwszy krok musi być kategorii Generator
		if (_steps[0].Category != StepCategory.Generator)
		{
			errorMessage = $"Błąd: Pierwszy krok to '{_steps[0].StepName}' (Modyfikator). Potok musi zaczynać się od Generatora.";
			return false;
		}
		return true;
	}

	/// <summary>
	/// Uruchamia wszystkie kroki potoku sekwencyjnie.
	/// </summary>
	/// <returns>Ostatecznie przetworzone dane mapy.</returns>
	public MapData ExecutePipeline(int width, int height, ulong globalSeed)
	{
		if (!ValidatePipeline(out string error))
		{
			throw new System.InvalidOperationException(error);
		}
		
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
