using Godot;
using ProceduralMapPipeline.Core;

namespace ProceduralMapPipeline.Modules.Noise;

/// <summary>
/// Krok generujący spójny szum ciągły (np. Perlin/Simplex) 
/// jako bazę pod ukształtowanie terenu.
/// </summary>
public class NoiseGenerationStep : GenerationStep
{
	private readonly FastNoiseLite _noise;

	public NoiseGenerationStep()
	{
		StepName = "Generator Szumu Spójnego";
		
		// Konfiguracja bazowa szumu (w przyszłości parametryzowana z UI)
		_noise = new FastNoiseLite();
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		_noise.Frequency = 0.02f; // Częstotliwość - określa "gęstość" struktur
	}

	public override MapData Execute(MapData inputData, ulong seed)
	{
		// FastNoiseLite przyjmuje int jako seed, więc bezpiecznie rzutujemy rozważając zakres
		_noise.Seed = (int)(seed % int.MaxValue);

		for (int x = 0; x < inputData.Width; x++)
		{
			for (int y = 0; y < inputData.Height; y++)
			{
				// GetNoise2d zwraca wartości z zakresu [-1.0, 1.0]
				float rawNoise = _noise.GetNoise2D(x, y);

				// Normalizacja do standardowego zakresu analitycznego [0.0, 1.0]
				float normalizedValue = (rawNoise + 1f) / 2f;

				inputData.SetValue(x, y, normalizedValue);
			}
		}

		return inputData;
	}
}
