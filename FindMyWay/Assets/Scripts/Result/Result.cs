using System;

namespace AssemblyCSharp
{
	public class Result
	{
		/// <summary>
		/// Titre du test
		/// </summary>
		private string title; 

		/// <summary>
		/// Le nombre d'itérations éxecuté par le test
		/// </summary>
		private int iterations;

		/// <summary>
		/// Meilleure erreur trouvé par le test
		/// </summary>
		private int bestError;
		/// <summary>
		/// Structure de donnée créée pour pouvoir stocker les 
		/// résultats lors des simulations
		/// </summary>
		public Result()
		{

		}
		public string getTitle(){
			return title;
		}
		public void setTitle(string newTitle){
			this.title = newTitle;
		}
		public int getIterations(){
			return iterations;
		}
		public void setIterations(string newIterations){
			this.iterations = newIterations;
		}
		public int getBestError(){
			return bestError;
		}
		public void setTitle(string newBestError){
			this.bestError = newBestError;
		}

	}
}



