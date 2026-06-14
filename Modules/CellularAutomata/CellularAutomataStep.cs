using ProceduralMapPipeline.Core;
using System;

namespace ProceduralMapPipeline.Modules.CellularAutomata;

/// <summary>
/// Krok wykorzystujący automaty komórkowe do wygładzania kształtów 
/// i generowania struktur przypominających jaskinie.
/// </summary>
public class CellularAutomataStep : GenerationStep
{
	private int _iterations;
	private float _threshold;

	public CellularAutomataStep(int iterations = 4, float threshold = 0.5f)
	{
		StepName = "Automat Komórkowy (Wygładzanie)";
		_iterations = iterations;
		_threshold = threshold;
	}

	public override MapData Execute(MapData inputData, ulong seed)
	{
		int width = inputData.Width;
		int height = inputData.Height;

		// ETAP 1: Binaryzacja. 
		// Zamieniamy płynne wartości szumu [0.0 - 1.0] na twarde stany dyskretne: 0 (pusta) lub 1 (ściana).
		for (int i = 0; i < inputData.Grid.Length; i++)
		{
			inputData.Grid[i] = inputData.Grid[i] >= _threshold ? 1f : 0f;
		}

		// ETAP 2: Symulacja (Iteracje Automatu)
		for (int step = 0; step < _iterations; step++)
		{
			// Tworzymy bufor, by czytać stary stan podczas modyfikowania nowego
			float[] oldGrid = new float[inputData.Grid.Length];
			Array.Copy(inputData.Grid, oldGrid, inputData.Grid.Length);

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int neighborWallCount = GetSurroundingWallCount(oldGrid, x, y, width, height);

					// Reguła przeżycia (Conway's style / Cave smoothing)
					if (oldGrid[y * width + x] == 1f)
					{
						// Jeśli ściana ma za mało sąsiadów, umiera (staje się podłogą)
						if (neighborWallCount < 4)
							inputData.SetValue(x, y, 0f);
					}
					else
					{
						// Jeśli podłoga ma dużo sąsiadujących ścian, staje się ścianą
						if (neighborWallCount > 4)
							inputData.SetValue(x, y, 1f);
					}
				}
			}
		}

		return inputData;
	}

	/// <summary>
	/// Zlicza żywych sąsiadów w promieniu 1 komórki (Sąsiedztwo Moore'a).
	/// </summary>
	private int GetSurroundingWallCount(float[] grid, int gridX, int gridY, int width, int height)
	{
		int wallCount = 0;
		
		for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
		{
			for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
			{
				// Pomijamy sprawdzanie samej siebie
				if (neighborX == gridX && neighborY == gridY)
					continue;

				// Zabezpieczenie krawędzi: jeśli wychodzimy poza mapę, traktujemy to jako ścianę, 
				// dzięki temu mapa "zamyka się" na brzegach.
				if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height)
				{
					wallCount++;
				}
				else
				{
					// Płaskie indeksowanie dla bufora
					if (grid[neighborY * width + neighborX] == 1f)
					{
						wallCount++;
					}
				}
			}
		}

		return wallCount;
	}
}
