using Godot;
using System;
using ProceduralMapPipeline.Core;
using ProceduralMapPipeline.Modules.Noise;
using ProceduralMapPipeline.Modules.CellularAutomata;

namespace ProceduralMapPipeline.UI;

public partial class Main : Control
{
	private TextureRect _mapDisplay;
	// Seed i generacja
	private SpinBox _seedInput;
	private Button _generateButton;
	private Button _randomSeedButton;
	
	// Zarządzanie listą kroków
	private VBoxContainer _stepListContainer;
	private OptionButton _addStepDropdown;
	private Button _addStepButton;
	
	// Misc
	private Button _exportButton;

	// Menedżer potoku
	private PipelineManager _pipelineManager;

	public override void _Ready()
	{
		_pipelineManager = new PipelineManager();
		
		// Pobieranie referencji do UI
		_mapDisplay = GetNode<TextureRect>("MainLayout/MapContainer/MapDisplay");
		var sidebarVBox = GetNode<VBoxContainer>("MainLayout/Sidebar/VBoxContainer");
		
		_seedInput = sidebarVBox.GetNode<SpinBox>("SeedContainer/SeedInput");
		_generateButton = sidebarVBox.GetNode<Button>("GenerateButton");
		_randomSeedButton = sidebarVBox.GetNode<Button>("SeedContainer/RandomSeedButton");
		
		_stepListContainer = sidebarVBox.GetNode<VBoxContainer>("StepScrollContainer/StepsListContainer");
		_addStepDropdown = sidebarVBox.GetNode<OptionButton>("AddStepContainer/AddStepDropdown");
		_addStepButton = sidebarVBox.GetNode<Button>("AddStepContainer/AddStepButton");

		// Subskrypcja sygnałów
		_generateButton.Pressed += OnGenerateButtonPressed;
		_randomSeedButton.Pressed += OnRandomSeedButtonPressed;
		_addStepButton.Pressed += OnAddStepButtonPressed;

		// Inicjalizacja dropdownu z dostępnymi algorytmami
		PopulateAddStepDropdown();

		// Domyślne na start
		_pipelineManager.AddStep(new NoiseGenerationStep());
		_pipelineManager.AddStep(new CellularAutomataStep(iterations: 5, threshold: 0.5f));
		
		//Pierwsze uruchomienie
		RefreshStepListUI();
		RunPipelineAndRender();
	}
	
	///<summary> Dodawanie opcji wyboru algorytmów generacji proceduralnej do dropdowna</summary>
	private void PopulateAddStepDropdown()
	{
		_addStepDropdown.Clear();
		_addStepDropdown.AddItem("Szum Simplex (FastNoiseLite)", 0);
		_addStepDropdown.AddItem("Automat Komórkowy (Wygładzanie)", 1);
	}
	
	private void OnAddStepButtonPressed()
	{
		int selectedId = _addStepDropdown.GetSelectedId();
		
		// Zależnie od wyboru, tworzy instancję odpowiedniego algorytmu
		GenerationStep newStep = selectedId switch
		{
			0 => new NoiseGenerationStep(),
			1 => new CellularAutomataStep(),
			_ => null
		};

		if (newStep != null)
		{
			_pipelineManager.AddStep(newStep);
			RefreshStepListUI();
		}
	}
	
	/// <summary>
	/// Czyści i na nowo buduje listę kroków w UIku na podstawie stanu w PipelineManager
	/// </summary>
	private void RefreshStepListUI()
	{
		// Czyszczenie obecnych dzieci kontenera
		foreach (Node child in _stepListContainer.GetChildren())
		{
			child.QueueFree(); // Usunięcie węzła
		}

		var steps = _pipelineManager.Steps;
		for (int i = 0; i < steps.Count; i++)
		{
			int index = i; // Kopia lokalna dla domknięcia w wyrażeniu lambda
			var step = steps[i];

			// Wiersz UI dla danego kroku
			HBoxContainer stepRow = new HBoxContainer();
			
			Label stepLabel = new Label
			{
				Text = $"{index + 1}. {step.StepName}",
				SizeFlagsHorizontal = SizeFlags.ExpandFill // zmuszenie etykiety do wypełnienia miejsca
			};
			
			Button removeButton = new Button { Text = "X" };
			removeButton.Pressed += () => 
			{
				_pipelineManager.RemoveStepAt(index);
				RefreshStepListUI();
			};

			stepRow.AddChild(stepLabel);
			stepRow.AddChild(removeButton);
			_stepListContainer.AddChild(stepRow);
		}
	}
	
	//przycisk wywołujący render
	private void OnGenerateButtonPressed() => RunPipelineAndRender();
	//przycisk losujący ziarno
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
		// walidowanie potoku
		if (!_pipelineManager.ValidatePipeline(out string errorMessage))
		{
			// Na razie logowanie błędu do konsoli, todo: popup ui
			GD.PrintErr($"[BŁĄD POTOKU]: {errorMessage}");
			return; // Generacja stop
		}
		
		//todo: wybór wielkości mapy, na razie hardcoded do szybkich testów
		int mapWidth = 256;
		int mapHeight = 256;
		ulong currentSeed = (ulong)_seedInput.Value;

		//Wykonanie potoku odseparowane od UI
		MapData finalMapData = _pipelineManager.ExecutePipeline(mapWidth, mapHeight, currentSeed);

		//Konwersja danych analitycznych MapData na reprezentację wizualną będącą Image
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
