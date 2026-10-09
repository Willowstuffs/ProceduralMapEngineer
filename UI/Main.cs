using Godot;
using System;
using ProceduralMapPipeline.Core;
using ProceduralMapPipeline.Modules.Noise;
using ProceduralMapPipeline.Modules.CellularAutomata;

namespace ProceduralMapPipeline.UI;

public partial class Main : Control
{
	private TextureRect _mapDisplay;
	private SpinBox _seedInput;
	private Button _generateButton;
	private Button _exportButton;
	private Button _randomSeedButton;

	//menedżer potoku
	private PipelineManager _pipelineManager;

	public override void _Ready()
	{
		_pipelineManager = new PipelineManager();
		//hardcoded krok szumu Simplex
		_pipelineManager.AddStep(new NoiseGenerationStep());
		//hardcoded konwersja do algorytmu automatów komórkowych: sąsiedztwo Moore'a i podwójne buforowanie
		_pipelineManager.AddStep(new CellularAutomataStep(iterations: 5, threshold: 0.5f));

		//Pobieranie referencji do UI
		_mapDisplay = GetNode<TextureRect>("MainLayout/MapContainer/MapDisplay");
		var sidebarVBox = GetNode<VBoxContainer>("MainLayout/Sidebar/VBoxContainer");
		_seedInput = sidebarVBox.GetNode<SpinBox>("SeedContainer/SeedInput");
		_generateButton = sidebarVBox.GetNode<Button>("GenerateButton");
		_exportButton = sidebarVBox.GetNode<Button>("ExportButton");
		_randomSeedButton = sidebarVBox.GetNode<Button>("SeedContainer/RandomSeedButton");

		_generateButton.Pressed += OnGenerateButtonPressed;
		_exportButton.Pressed += OnExportButtonPressed;
		_randomSeedButton.Pressed += OnRandomSeedButtonPressed;

		//Pierwsze uruchomienie
		RunPipelineAndRender();
	}
	
	//przycisk wywołujący render
	private void OnGenerateButtonPressed() => RunPipelineAndRender();
	
	//przycisk randomizujący ziarno
	private void OnRandomSeedButtonPressed()
	{
		_seedInput.Value = GD.Randi();
		RunPipelineAndRender();
	}

	//todo: export
	private void OnExportButtonPressed()
	{
		GD.Print("Export SVG triggerowany - logika wkrótce!");
	}

	/// <summary>
	/// Odpala potok przetwarzania danych, a następnie renderuje wynik matematyczny na ekranie.
	/// </summary>
	private void RunPipelineAndRender()
	{
		int mapWidth = 256;
		int mapHeight = 256;
		ulong currentSeed = (ulong)_seedInput.Value;

		//Wykonanie potoku , odseparowane od UI
		MapData finalMapData = _pipelineManager.ExecutePipeline(mapWidth, mapHeight, currentSeed);

		//Konwersja danych analitycznych (MapData) na reprezentację wizualną (Image)
		var image = Image.CreateEmpty(mapWidth, mapHeight, false, Image.Format.Rgb8);

		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				float val = finalMapData.GetValue(x, y);
				//Mapa w skali szarości na podstawie znormalizowanych danych [0, 1]
				image.SetPixel(x, y, new Color(val, val, val));
			}
		}

		_mapDisplay.Texture = ImageTexture.CreateFromImage(image);
	}
}
