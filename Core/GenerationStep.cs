namespace ProceduralMapPipeline.Core;

/// <summary>
/// Abstrakcyjna klasa bazowa dla wszystkich algorytmów w potoku generacji.
/// Każdy krok musi przyjąć dane wejściowe, przetworzyć je i zwrócić wynik.
/// </summary>
public abstract class GenerationStep
{
	/// <summary>
	/// Nazwa kroku, przydatna do wyświetlania w UI lub logowania w benchmarkach.
	/// </summary>
	public string StepName { get; protected set; } = "Base Step";

	/// <summary>
	/// Główna metoda wykonująca algorytm.
	/// </summary>
	/// <param name="inputData">Mapa wejściowa z poprzedniego kroku (lub pusta siatka).</param>
	/// <param name="seed">Globalne lub lokalne ziarno determinujące losowość kroku.</param>
	/// <returns>Zmodyfikowane dane mapy.</returns>
	public abstract MapData Execute(MapData inputData, ulong seed);
}
