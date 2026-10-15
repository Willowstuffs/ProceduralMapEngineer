namespace ProceduralMapPipeline.Core;

/// <summary> Kategorie określające rolę algorytmu w potoku. Używane do walidacji. </summary>
public enum StepCategory
{
	Generator, // Tworzy nowe dane od zera
	Modifier   // Modyfikuje istniejące dane
}

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
	
	/// <summary> Kategoria kroku wykorzystywana przez UI i Managera do zapobiegania błędom logiki. </summary>
	public StepCategory Category { get; protected set; }

	// kontrakty i/o
	/// <summary> Jakiego typu danych ten algorytm spodziewa się na wejściu. </summary>
	public MapDataType ExpectedInput { get; protected set; }

	/// <summary> Jaki typ danych ten algorytm zwraca po swoim wykonaniu. </summary>
	public MapDataType GeneratedOutput { get; protected set; }
	
	//
	public bool UseLocalSeed { get; set; } = false;
	public ulong LocalSeed { get; set; } = 12345;
	
	/// <summary> Zwraca właściwe ziarno dla tego kroku (lokalne lub globalne). </summary>
	protected ulong GetActualSeed(ulong globalSeed)
	{
		return UseLocalSeed ? LocalSeed : globalSeed;
	}
	
	/// <summary> Główna metoda wykonująca algorytm.</summary>
	/// <param name="inputData">Mapa wejściowa z poprzedniego kroku (lub pusta siatka).</param>
	/// <param name="seed">Globalne lub lokalne ziarno determinujące losowość kroku.</param>
	/// <returns>Zmodyfikowane dane mapy.</returns>
	public abstract MapData Execute(MapData inputData, ulong seed);
}
