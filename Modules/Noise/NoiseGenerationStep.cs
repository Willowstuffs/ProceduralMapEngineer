using Godot;
using ProceduralMapPipeline.Core;

namespace ProceduralMapPipeline.Modules.Noise;

/// <summary>
/// MOCK (na razie, z biblioteki): Krok generujący spójny szum ciągły (np. Perlin/Simplex) 
/// jako bazę pod ukształtowanie terenu.
/// </summary>
public class NoiseGenerationStep : GenerationStep
{
	private readonly FastNoiseLite _noise;
		
	public float Frequency
	{
		get => _noise.Frequency;
		set => _noise.Frequency = value;
	}

	public NoiseGenerationStep()
	{
		StepName = "Generator Szumu Spójnego";
		Category = StepCategory.Generator; //kategoria do blokowania akcji
		ExpectedInput = MapDataType.Empty;      // Nie potrzebuje niczego na wejściu
		GeneratedOutput = MapDataType.Continuous; // Zwraca płynne wartości
		
		//Konfiguracja bazowa szumu
		//todo: parametry w ui
		_noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		//Częstotliwość, tzn gęstość struktur. Im mniejsza, tym większe
		_noise.Frequency = 0.02f;
	}

	public override MapData Execute(MapData inputData, ulong seed)
	{
		//FastNoiseLite przyjmuje int jako seed, więc rzutowanie
		ulong actualSeed = GetActualSeed(seed);
		_noise.Seed = (int)(actualSeed % int.MaxValue);

		for (int x = 0; x < inputData.Width; x++)
		{
			for (int y = 0; y < inputData.Height; y++)
			{
				//GetNoise2d zwraca wartości z zakresu [-1.0, 1.0]
				float rawNoise = _noise.GetNoise2D(x, y);

				//Normalizacja do standardowego zakresu analitycznego [0.0, 1.0]
				float normalizedValue = (rawNoise + 1f) / 2f;

				inputData.SetValue(x, y, normalizedValue);
			}
		}
		inputData.DataType = GeneratedOutput;

		return inputData;
	}
}
