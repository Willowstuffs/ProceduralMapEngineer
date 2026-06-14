using System;

namespace ProceduralMapPipeline.Core;

/// <summary>
/// Kontener na dane wygenerowanej mapy. Przechowuje siatkę wartości w strukturze 1D
/// dla optymalizacji dostępu do pamięci (Cache Locality).
/// </summary>
public class MapData
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	
	// Tablica jednowymiarowa reprezentująca mapę 2D
	public float[] Grid { get; private set; }

	public MapData(int width, int height)
	{
		Width = width;
		Height = height;
		Grid = new float[width * height];
	}

	/// <summary>
	/// Pobiera wartość komórki używając płaskiego indeksowania.
	/// </summary>
	public float GetValue(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			throw new IndexOutOfRangeException($"Błąd dostępu: [{x},{y}] jest poza mapą.");
			
		return Grid[y * Width + x];
	}

	/// <summary>
	/// Ustawia wartość komórki używając płaskiego indeksowania.
	/// </summary>
	public void SetValue(int x, int y, float value)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return; // Bezpieczny powrót (lub rzucenie wyjątku, zależnie od preferencji)
			
		Grid[y * Width + x] = value;
	}
}
