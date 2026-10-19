using System.Collections.Generic;
using System.Diagnostics;

namespace ProceduralMapPipeline.Core;

/// <summary> Zarządza potokiem generacji. Przechowuje kolekcję kroków algorytmicznych
/// i wykonuje je sekwencyjnie przekazując dane z jednego kroku do kolejnego. </summary>
public class PipelineManager
{
	private readonly List<GenerationStep> _steps = new();
	
	// Przechowuje wyniki po każdym uruchomionym kroku 
	public List<MapData> StepResults { get; private set; } = new();
	
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
	
	/// <summary> Usuwa dowolny krok z potoku na podstawie jego indeksu. </summary>
	public void RemoveStepAt(int index)
	{
		if (index >= 0 && index < _steps.Count)
		{
			_steps.RemoveAt(index);
			StepResults.Clear(); // Zdezaktualizowane dane, usuwanie snapshotów
		}
	}

	/// <summary> Czyszczenie potoku </summary>
	public void ClearSteps() => _steps.Clear();

	/// <summary> Uruchamia wszystkie kroki potoku sekwencyjnie. </summary>
	/// <returns> Ostatecznie przetworzone dane mapy </returns>
	public MapData ExecutePipeline(int width, int height, ulong globalSeed)
	{
		if (!ValidatePipeline(out string error))
		{
			throw new System.InvalidOperationException(error);
		}
		
		StepResults.Clear(); // Czyszczenie historii potoku
		//Tworzy czystą mapę początkową
		MapData currentData = new MapData(width, height);

		//Wykonuje każdy krok po kolei, nadpisując referencję do danych
		foreach (var step in _steps)
		{
			//todo: stoper, benchmarki
			// Przetwarzanie danych
			currentData = step.Execute(currentData, globalSeed);
			
			// Zapisywanie kopii do historii
			StepResults.Add(currentData.Clone());
		}

		return currentData;
	}
	
	/// <summary> Sprawdza zgodność kontraktów we/wy między kolejnymi algorytmami by pilnować logiczną poprawność kolejki. </summary>
	public bool ValidatePipeline(out string errorMessage)
	{
		errorMessage = string.Empty;
		if (_steps.Count == 0)
		{
			errorMessage = "Potok jest pusty. Dodaj przynajmniej jeden algorytm generujący.";
			return false;
		}

		// Śledzenie typów danych płynących przez potok, pierwszy pusty
		MapDataType currentDataType = MapDataType.Empty;

		for (int i = 0; i < _steps.Count; i++)
		{
			var step = _steps[i];
			// Jeśli algorytm oczekuje czegoś innego, niż dostarczono i nie jest to pierwsze wejście
			if (step.ExpectedInput != currentDataType)
			{
				errorMessage = $"Błąd w kroku {i + 1} ({step.StepName}): Oczekiwano danych typu '{step.ExpectedInput}', ale otrzymano '{currentDataType}' z poprzedniego kroku.";
				return false;
			}

			// Aktualizacja typu danych na wyjście z obecnego algorytmu
			currentDataType = step.GeneratedOutput;
		}

		return true;
	}
}
