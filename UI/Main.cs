using Godot;
using System;

namespace ProceduralMapPipeline.UI; //PascalCase dla projektu

/// <summary> Główny kontroler widoku aplikacji. Odpowiada za spinanie interfejsu 
/// z logiką generacji i wyświetlanie wyników w przestrzeni wizualnej. </summary>
public partial class Main : Control
{
	//reminder: prywatne pole klasy z _ na początku
	private TextureRect _mapDisplay;

	public override void _Ready()
	{
		// Pobieranie referencji do węzła w drzewie sceny po ścieżce relatywnej.
		// Jeśli struktura drzewa to Main -> MainLayout -> MapContainer -> MapDisplay
		_mapDisplay = GetNode<TextureRect>("MainLayout/MapContainer/MapDisplay");
		
		GenerateMockNoise();
	}

	/// <summary> Generuje tablicę losowych wartości i konwertuje ją na teksturę (szum bieli),
	/// służącą do testowania połączenia logiki z interfejsem graficznym. </summary>
	private void GenerateMockNoise()
	{
		// Rozdzielczość siatki wejściowej (później przeniesie sie to do globalnych ustawień)
		int mapWidth = 256;
		int mapHeight = 256;

		// Pusty obraz w pamięci. RGB 8 bitów na kanał
		var image = Image.CreateEmpty(mapWidth, mapHeight, false, Image.Format.Rgb8);

		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				// Zwraca wartość 0.0 lub 1.0 symulując czarny/biały piksel
				float colorValue = GD.Randf() > 0.5f ? 1f : 0f;
				image.SetPixel(x, y, new Color(colorValue, colorValue, colorValue));
			}
		}

		//Aplikowanie do kontrolki UI
		_mapDisplay.Texture = ImageTexture.CreateFromImage(image);
	}
}
