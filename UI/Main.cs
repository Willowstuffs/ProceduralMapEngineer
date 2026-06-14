using Godot;
using System;

namespace ProceduralMapPipeline.UI;

/// <summary>
/// Główny kontroler widoku aplikacji. Odpowiada za spinanie interfejsu 
/// z logiką generacji i wyświetlanie wyników w przestrzeni wizualnej.
/// </summary>
public partial class Main : Control
{
	private TextureRect _mapDisplay;
	private SpinBox _seedInput;
	private Button _generateButton;
	private Button _exportButton;

	public override void _Ready()
	{
		// 1. Pobieranie referencji do węzłów
		_mapDisplay = GetNode<TextureRect>("MainLayout/MapContainer/MapDisplay");
		
		var sidebarVBox = GetNode<VBoxContainer>("MainLayout/Sidebar/VBoxContainer");
		_seedInput = sidebarVBox.GetNode<SpinBox>("SeedContainer/SeedInput");
		_generateButton = sidebarVBox.GetNode<Button>("GenerateButton");
		_exportButton = sidebarVBox.GetNode<Button>("ExportButton");

		// 2. Subskrypcja sygnałów (Eventów UI)
		_generateButton.Pressed += OnGenerateButtonPressed;
		_exportButton.Pressed += OnExportButtonPressed;

		// Wygenerowanie pierwszej mapy na start
		GenerateMockNoise();
	}

	private void OnGenerateButtonPressed()
	{
		GenerateMockNoise();
	}

	private void OnExportButtonPressed()
	{
		// Na ten moment tylko logujemy akcję w konsoli na dole ekranu
		GD.Print("Export SVG triggerowany - tutaj później podepniemy logikę zapisu!");
	}

	/// <summary>
	/// Generuje deterministyczny szum bieli na podstawie Seeda z UI.
	/// </summary>
	private void GenerateMockNoise()
	{
		int mapWidth = 256;
		int mapHeight = 256;

		// Pobieramy wartość Seeda z kontrolki UI
		ulong currentSeed = (ulong)_seedInput.Value;
		
		// Tworzymy dedykowany generator dla mapy, aby mieć pełną kontrolę nad determinizmem
		var rng = new RandomNumberGenerator();
		rng.Seed = currentSeed;

		var image = Image.CreateEmpty(mapWidth, mapHeight, false, Image.Format.Rgb8);

		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				// rng.Randf() generuje wartości dzięki stałemu Seedowi zawsze w ten sam sposób
				float colorValue = rng.Randf() > 0.5f ? 1f : 0f;
				image.SetPixel(x, y, new Color(colorValue, colorValue, colorValue));
			}
		}

		_mapDisplay.Texture = ImageTexture.CreateFromImage(image);
	}
}
