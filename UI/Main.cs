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
	
	// Prawy panel
	private int _selectedStepIndex = -1;
	private VBoxContainer _propertiesContainer;
	private Label _inspectorTitle;
	
	// Podgląd/timeline
	private HSlider _timelineSlider;
	private Label _timelineLabel;

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
		
		_propertiesContainer = GetNode<VBoxContainer>("MainLayout/RightPanel/InspectorLayout/ScrollContainer/PropertiesContainer");
		_inspectorTitle = GetNode<Label>("MainLayout/RightPanel/InspectorLayout/InspectorTitle");
		
		_timelineSlider = GetNode<HSlider>("MainLayout/MapContainer/TimelineSlider");
		_timelineLabel = GetNode<Label>("MainLayout/MapContainer/TimelineLabel");
		
		// Subskrypcja sygnałów
		_generateButton.Pressed += OnGenerateButtonPressed;
		_randomSeedButton.Pressed += OnRandomSeedButtonPressed;
		_addStepButton.Pressed += OnAddStepButtonPressed;
		_timelineSlider.ValueChanged += OnTimelineSliderChanged;

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
	/// Czyści i na nowo buduje listę kroków w UI na podstawie stanu w PipelineManager
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
			
			// Tworzenie pzycisku z nazwą algorytmu
			Button stepSelectButton = new Button
				{
					Text = $"{index + 1}. {step.StepName}",
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					// wizualne wyróżnienie jeśli to ten kliknięty
					Flat = _selectedStepIndex != index 
				};

			stepSelectButton.Pressed += () => 
				{
					_selectedStepIndex = index;
					RefreshStepListUI(); // Odświeżamy listę, by zaktualizować podświetlenie
					UpdateRightPanel();  // Budujemy inspektor właściwości
				};
			// Przycisk usuwania
			Button removeButton = new Button { Text = "X" };
			removeButton.Pressed += () => 
			{
				_pipelineManager.RemoveStepAt(index);
				
				// Aktualizacja zaznaczenia po usunięciu
				if (_selectedStepIndex == index)
					_selectedStepIndex = -1;
				else if (_selectedStepIndex > index)
					_selectedStepIndex--;

				RefreshStepListUI();
				UpdateRightPanel();
				RunPipelineAndRender();
			};

			stepRow.AddChild(stepSelectButton);
			stepRow.AddChild(removeButton);
			_stepListContainer.AddChild(stepRow);
		}
	}
	
	/// <summary> Generuje kontrolki edycji właściwości w prawym panelu dla zaznaczonego algorytmu.</summary>
	private void UpdateRightPanel()
	{
		//Czyszczenie poprzednich kontrolek
		foreach (Node child in _propertiesContainer.GetChildren())
		{
			child.QueueFree();
		}

		// Zabezpieczenie jeśli nic nie jest zaznaczone
		if (_selectedStepIndex < 0 || _selectedStepIndex >= _pipelineManager.Steps.Count)
		{
			_inspectorTitle.Text = "Właściwości: Brak zaznaczenia";
			return;
		}

		var step = _pipelineManager.Steps[_selectedStepIndex];
		_inspectorTitle.Text = $"Właściwości: {step.StepName}";

		// Sekcja 1 Ziarno Lokalne (Wspólne dla wszystkich kroków)
		_propertiesContainer.AddChild(new Label { Text = " Konfiguracja Ziarna " });

		var useLocalSeedCheck = new CheckBox
		{
			Text = "Użyj lokalnego ziarna",
			ButtonPressed = step.UseLocalSeed
		};
		useLocalSeedCheck.Toggled += (toggled) =>
		{
			step.UseLocalSeed = toggled;
			UpdateRightPanel(); // Przeładowanie by pokazać/ukryć pole
			RunPipelineAndRender();
		};
		_propertiesContainer.AddChild(useLocalSeedCheck);

		if (step.UseLocalSeed)
		{
			var seedRow = new HBoxContainer();
			seedRow.AddChild(new Label { Text = "Lokalne ziarno: " });

			var seedInput = new SpinBox
			{
				MinValue = 0,
				MaxValue = 9999999,
				Value = step.LocalSeed,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			seedInput.ValueChanged += (val) =>
			{
				step.LocalSeed = (ulong)val;
				RunPipelineAndRender();
			};

			seedRow.AddChild(seedInput);
			_propertiesContainer.AddChild(seedRow);
		}

		// Sekcja 2 Parametry dedykowane konkretnym algorytmom
		_propertiesContainer.AddChild(new Label { Text = "\n--- Parametry Algorytmu ---" });

		// Pattern matching
		if (step is NoiseGenerationStep noiseStep)
		{
			var freqRow = new HBoxContainer();
			freqRow.AddChild(new Label { Text = "Częstotliwość: " });

			var freqInput = new SpinBox
			{
				MinValue = 0.001f,
				MaxValue = 0.1f,
				Step = 0.001f,
				Value = noiseStep.Frequency,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			freqInput.ValueChanged += (val) =>
			{
				noiseStep.Frequency = (float)val;
				RunPipelineAndRender();
			};

			freqRow.AddChild(freqInput);
			_propertiesContainer.AddChild(freqRow);
		}
		else if (step is CellularAutomataStep caStep)
		{
			// Edycja iteracji
			var iterRow = new HBoxContainer();
			iterRow.AddChild(new Label { Text = "Iteracje: " });

			var iterInput = new SpinBox
			{
				MinValue = 1,
				MaxValue = 20,
				Step = 1,
				Value = caStep.Iterations,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			iterInput.ValueChanged += (val) =>
			{
				caStep.Iterations = (int)val;
				RunPipelineAndRender();
			};

			iterRow.AddChild(iterInput);
			_propertiesContainer.AddChild(iterRow);

			// Edycja progu
			var threshRow = new HBoxContainer();
			threshRow.AddChild(new Label { Text = "Próg: " });

			var threshInput = new SpinBox
			{
				MinValue = 0.0f,
				MaxValue = 1.0f,
				Step = 0.05f,
				Value = caStep.Threshold,
				SizeFlagsHorizontal = SizeFlags.ExpandFill
			};
			threshInput.ValueChanged += (val) =>
			{
				caStep.Threshold = (float)val;
				RunPipelineAndRender();
			};

			threshRow.AddChild(threshInput);
			_propertiesContainer.AddChild(threshRow);
		}
	}
	
	// Przycisk wywołujący render
	private void OnGenerateButtonPressed() => RunPipelineAndRender();
	// Przycisk losujący ziarno
	private void OnRandomSeedButtonPressed()
	{
		_seedInput.Value = GD.Randi();
		RunPipelineAndRender();
	}
	
	/// <summary> Wyciąganie mapy z historii po zmianie suwaka </summary>
	private void OnTimelineSliderChanged(double value)
	{
		int stepIndex = (int)value;
		// Jeśli nie ma żadnych algorytmów w kolejce
		if (_pipelineManager.StepResults.Count == 0 || stepIndex < 0 || stepIndex >= _pipelineManager.StepResults.Count)
			return;

		// Pobieranie snapshotu z historii
		MapData snapshotData = _pipelineManager.StepResults[stepIndex];
		string stepName = _pipelineManager.Steps[stepIndex].StepName;

		_timelineLabel.Text = $"Podgląd kroku {stepIndex + 1}: {stepName}";
		// Renderowanie wybranego snapshotu
		RenderMapData(snapshotData);
	}
	
	/// <summary> Wydzielona metoda renderująca by nie dublować kodu, dopis do historii </summary>
	private void RenderMapData(MapData dataToRender)
	{
		int mapWidth = dataToRender.Width;
		int mapHeight = dataToRender.Height;
		
		//Konwersja danych analitycznych MapData na reprezentację wizualną będącą Image
		var image = Image.CreateEmpty(mapWidth, mapHeight, false, Image.Format.Rgb8);

		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				float val = dataToRender.GetValue(x, y);
				//Mapa w skali szarości na podstawie znormalizowanych danych [0, 1]
				image.SetPixel(x, y, new Color(val, val, val));
			}
		}

		_mapDisplay.Texture = ImageTexture.CreateFromImage(image);
	}
	

	//todo: export
	private void OnExportButtonPressed()
	{
		GD.Print("Export SVG triggerowany - logika wkrótce!");
	}

	/// <summary> Odpala potok przetwarzania danych a potem renderuje wynik matematyczny na ekranie </summary>
	private void RunPipelineAndRender()
	{
		// Walidowanie potoku
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
		
		// Konfigurowanie historii snapshotów
		int stepCount = _pipelineManager.StepResults.Count;
		if (stepCount > 0)
		{
			_timelineSlider.MinValue = 0;
			_timelineSlider.MaxValue = stepCount - 1;
			_timelineSlider.Value = stepCount - 1; // Ustawiane na sam koniec
			_timelineSlider.TickCount = stepCount;
		}

		// Render ostatecznej mapy
		OnTimelineSliderChanged(_timelineSlider.Value);
	}
}
