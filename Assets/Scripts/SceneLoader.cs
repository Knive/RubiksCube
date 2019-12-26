using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RubiksCube
{
	/// <summary>
	/// Allows to load the game scene and initialize our Rubik's Cube with the requested dimension
	/// </summary> 
	public class SceneLoader : MonoBehaviour
	{
		public string gameScene;
		public Button resumeButton; 

		private void Start()
		{
			// Checks if there is a game to resume
			resumeButton.interactable = SaveSystem.SaveFileExists();
		}

		/// <summary>
		/// Loads the game scene
		/// </summary>
		/// <param name="cubeDim">Cube dimension (e.g. 3x3x3)</param>
		public void LoadGameScene(int cubeDimension)
		{
			PlayerSettings.CubeDimension = cubeDimension;
			SceneManager.LoadScene(gameScene);
		}

		/// <summary>
		/// Resume a game
		/// </summary>
		public void ResumeGame()
		{
			PlayerSettings.LoadGame = true;
			SceneManager.LoadScene(gameScene);
		}

		/// <summary>
		/// Quits application
		/// </summary>
		public void QuitGame()
		{
			StopAllCoroutines();
			Application.Quit();
		}
	}
}
