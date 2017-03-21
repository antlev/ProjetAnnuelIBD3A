using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// Classe principale à utiliser pour implémenter vos algorithmes
/// Si vous souhaitez utiliser plusieurs scripts (1 par algorithme), 
/// vous le pouvez aussi.
/// </summary>
public class MainScript : MonoBehaviour
{

	/// <summary>
	/// Indique si un algorithme est en cours d'exécution
	/// </summary>
	private bool _isRunning = false;

	/// <summary>
	/// Indique si une evaluation de solution est en cours
	/// </summary>
	private bool _inSimulation = false;

	/// <summary>
	/// Méthode utilisée pour gérer les informations et 
	/// boutons de l'interface utilisateur
	/// </summary>
	public void OnGUI()
	{
		
//		Debug.Log();

		// Démarrage d'une liste de composants visuels verticale
		GUILayout.BeginVertical();

		// Affiche un bouton permettant le lancement de la recherche locale naéve
		if (GUILayout.Button("DEMARRAGE RECHERCHE LOCALE NAIVE"))
		{
			// Le bouton est inactif si un algorithme est en cours d'exécution
			if (!_isRunning)
			{
				// Démarrage de la recherche locale naéve en pseudo asynchrone
				StartCoroutine("NaiveLocalSearch");
			}
		}

		// Affiche un bouton permettant le lancement de la recherche locale naéve
		if (GUILayout.Button("DEMARRAGE RECUIT SIMULE"))
		{
			// Le bouton est inactif si un algorithme est en cours d'exécution
			if (!_isRunning)
			{
				// Démarrage du recuit simulé en pseudo asynchrone
				StartCoroutine("SimulatedAnnealing");
			}
		}

		// Affiche un bouton permettant le lancement de l'algorithme génétique
		if (GUILayout.Button("DEMARRAGE ALGORITHME GENETIQUE"))
		{
			// Le bouton est inactif si un algorithme est en cours d'exécution
			if (!_isRunning)
			{
				// Démarrage de l'algorithme génétique en pseudo asynchrone
				StartCoroutine("GeneticAlgorithm");
			}
		}

		// Affiche un bouton permettant le lancement de l'algorithme de Djikstra
		if (GUILayout.Button("DEMARRAGE DJIKSTRA"))
		{
			// Le bouton est inactif si un algorithme est en cours d'exécution
			if (!_isRunning)
			{
				// Démarrage de l'algorithme de Djikstra en pseudo asynchrone
				StartCoroutine("Djikstra");
			}
		}

		// Affiche un bouton permettant le lancement de l'algorithme A*
		if (GUILayout.Button("DEMARRAGE A*"))
		{
			// Le bouton est inactif si un algorithme est en cours d'exécution
			if (!_isRunning)
			{
				// Démarrage de l'algorithme A* en pseudo asynchrone
				StartCoroutine("AStar");
			}
		}

		// Fin de la liste de composants visuels verticale
		GUILayout.EndVertical();
	}

	/// <summary>
	/// Initialisation du script
	/// </summary>
	void Start()
	{
		// Pour faire en sorte que l'algorithme puisse continuer d'étre actif méme
		// en téche de fond.
		Application.runInBackground = true;
	}

	/// <summary>
	/// Implémentation possible de la recherche locale naéve
	/// sous forme de coroutine pour le mode pseudo asynchone
	/// </summary>
	/// <returns></returns>
	public IEnumerator NaiveLocalSearch()
	{
		// Indique que l'algorithme est en cours d'exécution
		_isRunning = true;

		const int nbMoveInSol = 6;

		// Génére une solution initiale au hazard (ici une séquence
		// de 42 mouvements)
		var currentSolution = new PathSolutionScript(nbMoveInSol);

		// Récupére le score de la solution initiale
		// Sachant que l'évaluation peut nécessiter une 
		// simulation, pour pouvoir la visualiser nous
		// avons recours à une coroutine
		var scoreEnumerator = GetError(currentSolution);
		yield return StartCoroutine(scoreEnumerator);
		float currentError = scoreEnumerator.Current;

		// Nous récupérons l'erreur minimum atteignable
		// Ceci est optionnel et dépendant de la fonction
		// d'erreur
		var minimumError = GetMinError();

		// Affichage de l'erreur initiale
		Debug.Log("start currentError >" + currentError + "< - minimumError >" + minimumError + "<");
		logResults("Results for NaiveLocalSearch -- number of moves in solution : " );
		logResults ("start currentError >" + currentError + " - minimumError >" + minimumError + "<");

		// Initialisation du nombre d'itérations
		int iterations = 0;

		// Tout pendant que l'erreur minimale n'est pas atteinte
		while (currentError != GetMinError())
		{
			// On obtient une copie de la solution courante
			// pour ne pas la modifier dans le cas ou la modification
			// ne soit pas conservée.
			var newsolution = CopySolution(currentSolution);

			// On procéde à une petite modification de la solution
			// courante.
			RandomChangeInSolution(newsolution);

			// Récupére le score de la nouvelle solution
			// Sachant que l'évaluation peut nécessiter une 
			// simulation, pour pouvoir la visualiser nous
			// avons recours à une coroutine
			var newscoreEnumerator = GetError(newsolution);
			yield return StartCoroutine(newscoreEnumerator);
			float newError = newscoreEnumerator.Current;

			// On affiche pour des raisons de Debug et de suivi
			// la comparaison entre l'erreur courante et la
			// nouvelle erreur
			Debug.Log("[" + iterations + "] currentError >" + currentError + "< - newError >" + newError + "<- change Solution >" + (newError <= currentError) + "<");

			// Si la solution a été améliorée
			if (newError <= currentError)
			{
				// On met à jour la solution courante
				currentSolution = newsolution;

				// On met à jour l'erreur courante
				currentError = newError;
			}

			// On incrémente le nombre d'itérations
			iterations++;

			// On rend la main au moteur Unity3D
			yield return 0;
		}

		// Fin de l'algorithme, on indique que son exécution est stoppée
		_isRunning = false;

		// On affiche le nombre d'itérations nécessaire à l'algorithme pour trouver la solution
		Debug.Log("CONGRATULATIONS !!! Solution Found : iterations >" + iterations + "< - error >" + currentError + "<");
	}

	// Coroutine à utiliser pour implémenter l'algorithme de Djikstra
	public IEnumerator Djikstra()
	{
		// Récupération de l'environnement sous forme de matrice
		var matrix = MatrixFromRaycast.CreateMatrixFromRayCast();

		bool[][] booleanGrid = new bool[matrix.Length][];

		// Concersion de la grille proposée par le probléme en grille booléenne (case vide / obstacle)
		for (int i = 0; i < matrix.Length; i++)
		{
			booleanGrid[i] = new bool[matrix[i].Length];
			for (int j = 0; j < matrix[i].Length; j++)
			{
				booleanGrid[i][j] = (matrix[i][j] == LayerMask.NameToLayer("Obstacle")) ? true : false;
			}
		}

		// Récupération des positions de départ et d'arrivée
		var startPosX = PlayerScript.StartXPositionInMatrix;
		var startPosY = PlayerScript.StartYPositionInMatrix;
		var endPosX = PlayerScript.GoalXPositionInMatrix;
		var endPosY = PlayerScript.GoalYPositionInMatrix;

		// Lancement de l'algorithme de Djikstra
		var path = DjikstraAlgorithm.RunOn2DGrid(booleanGrid, startPosX, startPosY, endPosX, endPosY);

		// Si l'algorithme de Djikstra n'a pas trouvé de chemin possible.
		if (path == null || path.Count() < 1)
		{
			Debug.Log("NO SOLUTION FOUND");
		}
		else
		{
			var patharray = path.ToArray();

			// Convertion du chemin trouvé en ensemble d'actions
			var solution = new PathSolutionScript(patharray.Length - 1);
			for (int i = 0; i < patharray.Length - 1; i++)
			{
				// Conversion d'un mouvement entre deux case en action
				solution.Actions[i] =
					new ActionSolutionScript()
				{
					Action = new Vector3(
						(float)(patharray[i + 1].x - patharray[i].x),
						0f,
						(float)(patharray[i + 1].y - patharray[i].y)
					)
				};
			}

			// Simulation de la solution trouvée
			var scoreEnumerator = GetError(solution);
			yield return StartCoroutine(scoreEnumerator);
			float currentError = scoreEnumerator.Current;
		}
		yield return null;
	}

	// Coroutine à utiliser pour implémenter l'algorithme d' A*
	public IEnumerator AStar()
	{
		//TODO
		yield return null;
	}

	// Coroutine à utiliser pour implémenter l'algorithme du recuit simulé
	public IEnumerator SimulatedAnnealing()
	{
		// Indique que l'algorithme est en cours d'exécution
		_isRunning = true;

		const int nbSolutionMoves = 42;

		// Génére une solution initiale au hazard (ici une séquence
		// de 42 mouvements)
		var currentSolution = new PathSolutionScript(nbSolutionMoves);

		// Récupére le score de la solution initiale
		// Sachant que l'évaluation peut nécessiter une 
		// simulation, pour pouvoir la visualiser nous
		// avons recours à une coroutine
		var scoreEnumerator = GetError(currentSolution);
		yield return StartCoroutine(scoreEnumerator);
		float currentError = scoreEnumerator.Current;

		// Nous récupérons l'erreur minimum atteignable
		// Ceci est optionnel et dépendant de la fonction
		// d'erreur
		var minimumError = GetMinError();

		// Affichage de l'erreur initiale
		Debug.Log("start currentError >" + currentError + "< - minimumError >" + minimumError + "<");

		// Initialisation du nombre d'itérations
		int iterations = 0;

		// Initialisation du nombre d'itérations maximum
		const int iterationsMax = 1000;

		// Tout pendant que l'erreur minimale n'est pas atteinte
		while (currentError != GetMinError() && iterations <= iterationsMax)
		{
			// On obtient une copie de la solution courante
			// pour ne pas la modifier dans le cas ou la modification
			// ne soit pas conservée.
			var newsolution = CopySolution(currentSolution);

			// On procéde à une petite modification de la solution
			// courante.
			RandomChangeInSolution(newsolution);

			// Récupére le score de la nouvelle solution
			// Sachant que l'évaluation peut nécessiter une 
			// simulation, pour pouvoir la visualiser nous
			// avons recours à une coroutine
			var newscoreEnumerator = GetError(newsolution);
			yield return StartCoroutine(newscoreEnumerator);
			float newError = newscoreEnumerator.Current;

			// On affiche pour des raisons de Debug et de suivi
			// la comparaison entre l'erreur courante et la
			// nouvelle erreur
			float rdm =  Random.value; 
			float prob = Prob(newError - currentError ,temp(iterations,iterationsMax));
			Debug.Log("[" + iterations + "] currentError >" + currentError + "< - newError >" + newError + "< - rdm >" + rdm.ToString() + "< - prob>" + "< - Solution change >" + (newError <= currentError || rdm < prob) + "< - iterationsMax >" + iterationsMax + "<");

			if (newError <= currentError || rdm < prob)
			{
				// On met à jour la solution courante
				currentSolution = newsolution;

				// On met à jour l'erreur courante
				currentError = newError;
			}

			// On incrémente le nombre d'itérations
			iterations++;

			// On rend la main au moteur Unity3D
			yield return 0;
		}

		// Fin de l'algorithme, on indique que son exécution est stoppée
		_isRunning = false;

		if (currentError <= minimumError) {
			// On affiche le nombre d'itérations nécessaire à l'algorithme pour trouver la solution
			Debug.Log ("CONGRATULATIONS !!! Solution Found : iterations >" + iterations + "< - error >" + currentError + "< - iterationsMax >" + iterationsMax + "<");
		} else {
			Debug.Log ("Sorry. no solution found - best solution : iterations >" + iterations + "< - error >" + currentError + "< - minimumError >" + minimumError + "<");
		}
	}
	//Fonction de probabilité
	//Renvoie un entier entre 0 et 1
	//Plus l'entier renvoyé est proche de 0, moins l'exploration sera permise 
	private float Prob(float diffError, float temperature){
		return (float)Mathf.Exp (-diffError / temperature);
	}

	private float temp(int iterations, int iterationsMax){
		if (iterationsMax > iterations) {
			return 0;
		} else {
			// Variations linéaires de la température
			//return (float)iterationsMax - iterations;
			//Variations exponentielles
			return (float)Mathf.Exp(iterations-iterationsMax);
		}

	}

	// Coroutine à utiliser pour implémenter un algorithme génétique
	public IEnumerator GeneticAlgorithm()
	{
		// Indique que l'algorithme est en cours d'exécution
		_isRunning = true;

		const int popSize = 200;
		const float bestPercentage = 0.2f;
		const int bestCount = (int)(popSize * bestPercentage);
		const float mutationRate = 0.1f;

		const int nbMoveInSolution = 6;
		// INITIALISATION DE LA POPULATION
		PathSolutionScript[] population = new PathSolutionScript[popSize];

		for(var i = 0; i < popSize; i++)
		{
			population[i] = new PathSolutionScript(nbMoveInSolution);
		}

		while (true)
		{
		// EVALUATION DE LA POPULATION
			var scoredPopulation = new Dictionary<PathSolutionScript, IEnumerator<float>>();
			for(var i = 0; i < popSize; i++)
			{
				scoredPopulation.Add(population[i], GetError(population[i]));
			}

		// SELECTION DES REPRODUCTEURS
			var bests = scoredPopulation
				.OrderBy(kv => kv.Value)
				.Take(bestCount)
				.Select(kv => kv.Key)
				.ToArray();

		// CROISEMENT DE LA POPULATION
			PathSolutionScript[] newPopulation = new PathSolutionScript[popSize];

			// On sélectionne 2 solutions au hasard parmis les reproducteurs (solutions conservées)
			var sol1 = bests[Random.Range(0, popSize-bestCount)];
			var sol2 = bests[Random.Range(0, popSize-bestCount)];

			// On croise les solutions (cad on échange une action entre les deux)
			var random = Random.Range(0,nbMoveInSolution);
			var tmp = sol1.Actions[random];
			sol1.Actions[random] = sol2.Actions[random];
			sol2.Actions[random] = tmp;

		// MUTATION
			for(var i = 0;i < bestCount; i++)
			{
				var rdm = Random.Range(0f, 1f);
				if(rdm < mutationRate)
				{
					var pos1 = Random.Range(0, nbMoveInSolution);
					var pos2 = Random.Range(0, nbMoveInSolution);

					var tmp2 = bests[i].Actions[pos1];
					bests[i].Actions [pos1] = bests [i].Actions [pos2];
					bests[i].Actions [pos2] = tmp2;
				}
			}

			for (var i = 0; i < bestCount; i++) 
			{
				newPopulation[i] = bests [i];
			}
			for (var i = bestCount; i < popSize; i++)
			{
				newPopulation[i] = new PathSolutionScript (nbMoveInSolution);
			}
			yield return null;
		}
	}

	/// <summary>
	/// Exemple d'erreur minimum (pas forcément toujours juste) renvoyant
	/// la distance de manhattan entre la case d'arrivée et la case de départ.
	/// </summary>
	/// <returns></returns>
	int GetMinError()
	{
		return (int)(Mathf.Abs(PlayerScript.GoalXPositionInMatrix - PlayerScript.StartXPositionInMatrix) +
			Mathf.Abs(PlayerScript.GoalYPositionInMatrix - PlayerScript.StartYPositionInMatrix));
	}

	/// <summary>
	/// Exemple d'oracle nous renvoyant un score que l'on essaye de minimiser
	/// Ici est utilisé la position de la case d'arrivée, la position finale
	/// atteinte par la solution. Il est recommandé d'essayer plusieurs oracles
	/// pour étudier le comportement des algorithmes selon la qualité de ces
	/// derniers
	/// 
	/// Parmi les paramétres pouvant étre utilisés pour calculer le score/erreur :
	/// 
	///  - position de la case d'arrivée    : PlayerScript.GoalXPositionInMatrix
	///                                       PlayerScript.GoalYPositionInMatrix
	///  - position du joueur               : player.PlayerXPositionInMatrix
	///                                       player.PlayerYPositionInMatrix
	///  - position de départ du joueur     : PlayerScript.StartXPositionInMatrix
	///                                       PlayerScript.StartYPositionInMatrix
	///  - nombre de cases explorées        : player.ExploredPuts
	///  - nombre d'actions exécutées       : player.PerformedActionsNumber
	///  - vrai si le la balle a touché la case d'arrivée : player.FoundGoal
	///  - vrai si le la balle a touché un obstacle : player.FoundObstacle
	///  - interrogation de la matrice      :
	///       - la case de coordonnée (i, j) est elle un obstacle (i et j entre 0 et 49) :
	///           player.GetPutTypeAtCoordinates(i, j) == LayerMask.NameToLayer("Obstacle")
	///       - la case de coordonnée (i, j) est elle explorée (i et j entre 0 et 49) :
	///           player.GetPutTypeAtCoordinates(i, j) == 1
	///       - la case de coordonnée (i, j) est elle inexplorée (i et j entre 0 et 49) :
	///           player.GetPutTypeAtCoordinates(i, j) == 0
	/// </summary>
	/// <param name="solution"></param>
	/// <returns></returns>
	IEnumerator<float> GetError(PathSolutionScript solution)
	{
		// On indique que l'on s'appréte à lancer la simulation
		_inSimulation = true;

		// On créé notre objet que va exécuter notre séquence d'action
		var player = PlayerScript.CreatePlayer();

		// Pour pouvoir visualiser la simulation (moins rapide)
		player.RunWithoutSimulation = false;

		// On lance la simulation en spécifiant
		// la séquence d'action à exécuter
		player.LaunchSimulation(solution);

		// Tout pendant que la simulation n'est pas terminée
		while (player.InSimulation)
		{
			// On rend la main au moteur Unity3D
			yield return -1f;
		}

		// Calcule la distance de Manhattan entre la case d'arrivée et la case finale de
		// notre objet, la pondére (la multiplie par zéro si le but a été trouvé) 
		// et ajoute le nombre d'actions jouées
//	    var error = (Mathf.Abs(PlayerScript.GoalXPositionInMatrix - player.PlayerXPositionInMatrix)
//			+ Mathf.Abs(PlayerScript.GoalYPositionInMatrix - player.PlayerYPositionInMatrix))
//			* (player.FoundGoal ? 0 : 100) +
//			player.PerformedActionsNumber;

		// Erreur prenant en compte les obstacles
		var error = (Mathf.Abs(PlayerScript.GoalXPositionInMatrix - player.PlayerXPositionInMatrix)
			+ Mathf.Abs(PlayerScript.GoalYPositionInMatrix - player.PlayerYPositionInMatrix))
			* (player.FoundGoal ? 0 : 100) 
			+ player.PerformedActionsNumber
			+ (player.FoundObstacle ? 1000 : 0);
		
		Debug.Log("play.FoundGoal >" + player.FoundGoal + "<");

		// Détruit  l'objet de la simulation
		Destroy(player.gameObject);

		// Renvoie l'erreur précédemment calculée
		yield return error;

		// Indique que la phase de simulation est terminée
		_inSimulation = false;
	}

	/// <summary>
	/// Execute un changement aléatoire sur une solution
	/// ici, une action de la séquence est tirée au hasard et remplacée
	/// par une nouvelle au hasard.
	/// </summary>
	/// <param name="sol"></param>
	public void RandomChangeInSolution(PathSolutionScript sol)
	{
		sol.Actions[Random.Range(0, sol.Actions.Length)] = new ActionSolutionScript();
	}

	/// <summary>
	/// Fonction utilitaire ayant pour but de copier
	/// dans un nouvel espace mémoire une solution
	/// </summary>
	/// <param name="sol">La solution à copier</param>
	/// <returns>Une copie de la solution</returns>
	public PathSolutionScript CopySolution(PathSolutionScript sol)
	{
		// Initialisation de la nouvelle séquence d'action
		// de la méme longueur que celle que l'on souhaite copier
		var newSol = new PathSolutionScript(sol.Actions.Length);

		// Pour chaque action de la séquence originale,
		// on copie le type d'action.
		for (int i = 0; i < sol.Actions.Length; i++)
		{
			newSol.Actions[i].Action = sol.Actions[i].Action;
		}

		// Renvoi de la solution copiée
		return newSol;
	}
	public string getTempPath()
	{
		string path = System.Environment.GetEnvironmentVariable("TEMP");
		if (!path.EndsWith("\\")) path += "\\";
		return path;
	}

	public void logResults(string msg)
	{
		System.IO.StreamWriter sw = System.IO.File.AppendText(
			getTempPath() + "projet_annuel.log");
		try
		{
			string logLine = System.String.Format(
				"{0:G}: {1}.", System.DateTime.Now, msg);
			sw.WriteLine(logLine);
		}
		finally
		{
			sw.Close();
		}
	}
}




// CODE ALGO ETUDIE EN COURS

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//
//public class MainScript : MonoBehaviour {
//	[SerializeField]
//	private Transform[] cubes;
//
//	// Use this for initialization
//	IEnumerator Start () {
//		Debug.Log("ERREUR avant mélange : " + GetError(cubes));
//		yield return StartCoroutine(Scramble(cubes));
//		Debug.Log("ERREUR après mélange : " + GetError(cubes));
//
//		yield return new WaitForSeconds(2f);
//
//		//yield return StartCoroutine(NaiveLocalSearch(cubes));
//		//yield return StartCoroutine(SimulatedAnnealing(cubes));
//		yield return StartCoroutine(GeneticAlgorithm(cubes));
//	}
//
//	public static void InstantScramble(Transform[] cubes)
//	{
//		for (var i = 0; i < cubes.Length; i++)
//		{
//			var rdm = Random.Range(i, cubes.Length);
//			Swap(cubes[i], cubes[rdm]);
//		}
//	}
//	static IEnumerator Scramble(Transform[] cubes)
//	{
//		for (var i = 0; i < cubes.Length; i++)
//		{
//			var rdm = Random.Range(i, cubes.Length);
//			Swap(cubes[i], cubes[rdm]);
//			yield return new WaitForEndOfFrame();
//		}
//	}
//
//	static void Swap(Transform cube1, Transform cube2)
//	{
//		var tmp = cube1.transform.position;
//		cube1.transform.position = cube2.transform.position;
//		cube2.transform.position = tmp;
//	}
//
//	static int GetError(Transform[] cubes)
//	{
//		var error = 0;
//
//		foreach(var cube1 in cubes)
//		{
//			foreach(var cube2 in cubes)
//			{
//				if( cube1.tag == cube2.tag &&
//					cube1.position.y != cube2.position.y)
//				{
//					++error;
//				}
//			}
//		}
//		return error;
//	}
//
//	static IEnumerator NaiveLocalSearch(Transform[] cubes)
//	{
//		var currentEror = GetError(cubes);
//
//		while (currentEror != 0)
//		{
//			Transform cube1;
//			Transform cube2;
//
//			do
//			{
//				cube1 = cubes[Random.Range(0, cubes.Length)];
//				cube2 = cubes[Random.Range(0, cubes.Length)];
//			} while (!AreNeighbours(cube1, cube2));
//
//			Swap(cube1, cube2);
//
//			var newError = GetError(cubes);
//
//			if (newError <= currentEror)
//			{
//				currentEror = newError;
//			}
//			else
//			{
//				Swap(cube1, cube2);
//			}
//			yield return new WaitForEndOfFrame();
//
//
//		}
//	}
//	static IEnumerator SimulatedAnnealing(Transform[] cubes)
//	{
//		var currentEror = GetError(cubes);
//		var temperature = 2f;
//		var stagnation = 0f;
//
//		while (currentEror != 0)
//		{
//			var previousError = currentEror;
//			Transform cube1;
//			Transform cube2;
//
//			do
//			{
//				cube1 = cubes[Random.Range(0, cubes.Length)];
//				cube2 = cubes[Random.Range(0, cubes.Length)];
//			} while (!AreNeighbours(cube1, cube2));
//
//			Swap(cube1, cube2);
//
//			var newError = GetError(cubes);
//			var rdm = Random.Range(0f, 1f);
//
//			if (rdm <= MetropolisRule(temperature, currentEror, newError))
//			{
//				currentEror = newError;
//			}
//			else
//			{
//				Swap(cube1, cube2);
//			}
//			if(previousError == currentEror)
//			{
//				stagnation++;
//			}
//			else
//			{
//				stagnation = 0;
//			}
//			if(stagnation > 1000)
//			{
//				temperature = 0f;
//				stagnation = 0f;
//			}
//
//			temperature -= 0.001f;
//
//			Debug.Log("Temperature = " + temperature + " | stagnation = " + stagnation);
//			yield return new WaitForEndOfFrame();
//
//
//		}
//	}
//	public static float MetropolisRule(float temperature, int currentError, int newError)
//	{
//		if(temperature <= 0)
//		{
//			return currentError - newError <= 0 ? 1f : 0f;
//		}
//		return Mathf.Exp((currentError - newError) / temperature);
//	}
//
//	public static bool AreNeighbours(Transform cube1,Transform cube2)
//	{
//		// Cote à cote
//		if(Mathf.Abs(cube1.position.x - cube2.position.x) == 2f && 
//			cube1.position.y == cube2.position.y)
//		{
//			return true;
//		}
//
//		// l'un au dessus de l'autre
//		if (Mathf.Abs(cube1.position.y - cube2.position.y) == 2f &&
//			cube1.position.x == cube2.position.x)
//		{
//			return true;
//		}
//		return false;
//
//
//	}
//
//	public static IEnumerator GeneticAlgorithm(Transform[] cubes)
//	{
//		const int popSize = 200;
//		const float bestPercentage = 0.2f;
//		const int bestCount = (int)(popSize * bestPercentage);
//		const float mutationRate = 0.1f;
//		// INITIALISATION DE LA POPULATION
//		var population = new Vector3[popSize][];
//
//		for(var i = 0; i < popSize; i++)
//		{
//			population[i] = new Vector3[cubes.Length];
//			InstantScramble(cubes);
//			for(var j = 0; j < cubes.Length; j++)
//			{
//				population[i][j] = cubes[j].position;
//			}
//		}
//		while (true)
//		{
//			// EVALUATION DE LA POPULATION
//			var scoredPopulation = new Dictionary<Vector3[], int>();
//
//			for(var i = 0; i < popSize; i++)
//			{
//				for(var j = 0; j < cubes.Length; j++)
//				{
//					cubes[j].position = population[i][j];
//				}
//				scoredPopulation.Add(population[i], GetError(cubes));
//			}
//
//			// SELECTION DES REPRODUCTEURS
//			var bests = scoredPopulation
//				.OrderBy(kv => kv.Value)
//				.Take(bestCount)
//				.Select(kv => kv.Key)
//				.ToArray();
//
//			for(var i = 0; i < cubes.Length; i++)
//			{
//				cubes[i].position = bests[0][i];
//			}
//
//			if(GetError(cubes) == 0)
//			{
//				break;
//			}
//
//			// CROISEMENT DE LA POPULATION
//			var newPopulation = new Vector3[popSize][];
//
//			for(var i = 0; i < popSize; i++)
//			{
//				var p1 = bests[Random.Range(0, popSize)];
//				var p2 = bests[Random.Range(0, popSize)];
//			}
//			// MUTATION
//			for(var i = 0;i < popSize; i++)
//			{
//				var rdm = Random.Range(0f, 1f);
//				if(rdm < mutationRate)
//				{
//					var pos1 = Random.Range(0, cubes.Length);
//					var pos2 = Random.Range(0, cubes.Length);
//
//					var tmp = population[i][pos1];
//					population[i][pos1] = population[i][pos2];
//					population[i][pos2] = tmp;
//				}
//			}
//
//			population = newPopulation;
//
//			yield return null;
//		}
//	}
//
//	public static Vector3[] Crossover(Vector3[] p1,Vector3[] p2)
//	{
//		var child = new Vector3[p1.Length];
//		for(var i = 0; i < child.Length;i++)
//		{
//			child[i] = new Vector3(-999999f, -999999f, -999999f);
//		}
//		var parentCnt = 0;
//		var lookingAtP1 = true;
//
//		for(var i = 0; i < child.Length; i++)
//		{
//			if (lookingAtP1)
//			{
//				if (!child.Contains(p1[parentCnt]))
//				{
//					child[i] = p1[parentCnt];
//				}
//				else if(!child.Contains(p2[parentCnt]))
//				{
//					child[i] = p2[parentCnt];
//				}
//				else
//				{
//					i--;
//				}
//			}
//			else
//			{
//				if (!child.Contains(p2[parentCnt]))
//				{
//					child[i] = p2[parentCnt];
//				}
//				else if (!child.Contains(p1[parentCnt]))
//				{
//					child[i] = p1[parentCnt];
//				}
//				else
//				{
//					i--;
//				}
//			}
//			parentCnt++;
//		}
//		return child;
//	}
//
//}
