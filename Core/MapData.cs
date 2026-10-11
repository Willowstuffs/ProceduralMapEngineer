using System;

namespace ProceduralMapPipeline.Core;

/// <summary>
/// Typy danych przechowywanych w mapie służące do weryfikacji poprawności kolejki.
/// </summary>
public enum MapDataType
{
	Empty,       // Brak danych
	Continuous,  // Płynne wartości [od 0 do 1], np mapa wysokości lub szum
	Discrete,    // Binarne wartości 0 lub 1 (np podłoga, ściana)
	Categorical  // Kategoryczne ID (do konkretnych rodzajów, np piasek, trawa, woda)
}

/// <summary>
/// Kontener na dane wygenerowanej mapy. Przechowuje siatkę wartości w strukturze 1D
/// dla optymalizacji dostępu do pamięci, tzw. cache locality
/// </summary>
public class MapData
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	//Tablica jednowymiarowa reprezentująca mapę 2D
	public float[] Grid { get; private set; }
	// Aktualny typ danych w siatce
	public MapDataType DataType { get; set; }

	public MapData(int width, int height, MapDataType dataType = MapDataType.Empty)
	{
		Width = width;
		Height = height;
		DataType = dataType;
		Grid = new float[width * height];
	}

	/// <summary> Pobiera wartość komórki używając płaskiego indeksowania.</summary>
	public float GetValue(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			throw new IndexOutOfRangeException($"Błąd dostępu: [{x},{y}] jest poza mapą.");
			
		return Grid[y * Width + x];
	}

	/// <summary> Ustawia wartość komórki używając płaskiego indeksowania.</summary>
	public void SetValue(int x, int y, float value)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height)
			return;
			
		Grid[y * Width + x] = value;
	}
}
