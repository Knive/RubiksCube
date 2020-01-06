using RubiksCube;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region Public variables
	public GameObject cubiePrefab;
	public float rotationTime;
	public int scrambleTimes;
	public float scrambleRotationTime;
	#endregion

	#region Game variables
	public static bool isFaceRotating;
	public static bool isCurrentlyRotating;
	public static bool isScrambling;
	public static bool isGameWon;

	/// <summary>
	/// Rubik's cube data structure
	/// </summary>
	private Cubie[,,] cube;
	/// <summary>
	/// Solved cube state
	/// </summary>
	private List<string[,,]> solvedCubes;
	/// <summary>
	/// Cube root
	/// </summary>
	private GameObject cubeRoot;
	/// <summary>
	/// Game object used to rotate faces
	/// </summary>
	private GameObject pivot;

	private Stack<Move> movesHistory = new Stack<Move>();
	private Stack<Move> redoMovesHistory = new Stack<Move>();

	private Queue<Move> savedMoves = new Queue<Move>();

	#endregion

	#region UI
	public Text timer;
	public Button undoButton;
	public Button redoButton;
	public GameObject confirmationModal;
	public GameObject menuButton;
	public GameObject solvedText;
	public GameObject resultsScreen;
	private float time;
	#endregion

	// Start is called before the first frame update
	void Start()
	{
		// New game
		if (!PlayerSettings.LoadGame)
		{
			if (PlayerSettings.CubeDimension == 0)
			{
				PlayerSettings.CubeDimension = 3;
			}

			// Instantiate cube
			InstantiateRubiksCube();

			// Scramble cube
			StartCoroutine(ScrambleCube(scrambleTimes, scrambleRotationTime));
		}

		// Load data
		else
		{
			// Retrieve data
			MoveData data = SaveSystem.LoadMoves();
			Queue<Move> loadedMoves = data.ToQueue();

			// Set cube dimension
			PlayerSettings.CubeDimension = data.cubeDimension;

			// Timer
			this.time = data.time;

			// Instantiate cube
			InstantiateRubiksCube();

			// Apply every loaded move
			foreach (Move move in loadedMoves)
			{
				isFaceRotating = true;

				// Rotate
				SimpleRotateAlong(move.Axis, move.Angle, move.Index, true, false);
			}

			PlayerSettings.LoadGame = false;
		}
	}

	// Update is called once per frame
	void Update()
	{
		// Timer
		if (!isGameWon && !isScrambling)
		{
			time += Time.deltaTime;
		}

		if (PlayerSettings.ShowTimer)
		{
			TimeSpan t = TimeSpan.FromSeconds(time);
			timer.text = t.Hours > 0 ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds) : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
		}
		else
		{
			timer.text = "";
		}
	}

	/// <summary>
	/// Instantiates Rubik's cube
	/// </summary>
	private void InstantiateRubiksCube()
	{
		cube = new Cubie[PlayerSettings.CubeDimension, PlayerSettings.CubeDimension, PlayerSettings.CubeDimension];
		cubeRoot = new GameObject("Rubik's Cube");

		float offset = PlayerSettings.CubeDimension / 2f - 0.5f;

		for (int z = 0; z < PlayerSettings.CubeDimension; z++) {
			for (int y = 0; y < PlayerSettings.CubeDimension; y++) {
				for (int x = 0; x < PlayerSettings.CubeDimension; x++)
				{
					// Instantiate and name each cubie
					GameObject cubie = UnityEngine.Object.Instantiate(cubiePrefab, new Vector3(x - offset, y - offset, z - offset), Quaternion.identity, cubeRoot.transform);
					cubie.name = x.ToString() + y.ToString() + z.ToString();

					cube[x, y, z] = cubie.GetComponent<Cubie>();
				}
			}
		}

		// Determine every solved cube states
		DetermineSolvedCubeStates();
	}

	/// <summary>
	/// Determine the different solved cube states (init state with different rotations should be 6 (cube faces count) times 4 (rotations count along an axis))
	/// </summary>
	private void DetermineSolvedCubeStates()
	{
		// Init
		solvedCubes = new List<string[,,]>();

		// Every 90 angle (X axis)
		for (int angleX = 0; angleX < 4; angleX++) {
			// For every cubie in a row
			for (int i = 0; i < PlayerSettings.CubeDimension; i++)
			{
				// Rotate without storing any move or checking if game was won
				SimpleRotateAlong(RotationAxis.X, 90, i, true, false);
			}

			// Add solved cube state
			solvedCubes.Add(GetCubeState());

			// Every 90 angle (Y axis)
			for (int angleY = 0; angleY < 4; angleY++) {

				// For every cubie in a row
				for (int j = 0; j < PlayerSettings.CubeDimension; j++)
				{
					// Rotate without storing any move or checking if game was won
					SimpleRotateAlong(RotationAxis.Y, 90, j, true, false);
				}

				// Add solved cube state
				solvedCubes.Add(GetCubeState());
			}

			// Add solved cube state
			solvedCubes.Add(GetCubeState());
		}
	}

	/// <summary>
	/// Get cube current state
	/// </summary>
	/// <returns>3d array containing cubie's names</returns>
	private string[,,] GetCubeState()
	{
		string[,,] cubeState = new string[PlayerSettings.CubeDimension, PlayerSettings.CubeDimension, PlayerSettings.CubeDimension];

		for (int z = 0; z < PlayerSettings.CubeDimension; z++) {
			for (int y = 0; y < PlayerSettings.CubeDimension; y++) {
				for (int x = 0; x < PlayerSettings.CubeDimension; x++)
				{
					// Cubie name
					cubeState[x, y, z] = cube[x, y, z].name;
				}
			}
		}

		return cubeState;
	}

	/// <summary>
	/// Logs cube state
	/// </summary>
	/// <param name="cube">Array of cubie's name</param>
	public void LogCubeState(string[,,] cube)
	{
		string cubeString = "";

		for (int z = 0; z < PlayerSettings.CubeDimension; z++) {
			for (int y = 0; y < PlayerSettings.CubeDimension; y++) {
				for (int x = 0; x < PlayerSettings.CubeDimension; x++)
				{
					// Cubie name
					cubeString+= " " + cube[x, y, z];
				}
			}
		}

		Debug.Log(cubeString);
	}

	/// <summary>
	/// Logs cube state
	/// </summary>
	/// <param name="cube">Array of cubie</param>
	public void LogCubeState(Cubie[,,] cube)
	{
		string cubeString = "";

		for (int z = 0; z < PlayerSettings.CubeDimension; z++) {
			for (int y = 0; y < PlayerSettings.CubeDimension; y++) {
				for (int x = 0; x < PlayerSettings.CubeDimension; x++)
				{
					// Cubie name
					cubeString+= " " + cube[x, y, z].name;
				}
			}
		}

		Debug.Log(cubeString);
	}

	/// <summary>
	/// Rotates a face along a given axis set in the method parameters
	/// </summary>
	/// <param name="axis">Axis (X, Y, Z)</param>
	/// <param name="angle">Rotate angle (clockwise or anti-clockwise)</param>
	/// <param name="rotationIndex">Column/row rotation index</param>
	/// <returns></returns>
	public IEnumerator RotateAlong(RotationAxis axis, float angle, int rotationIndex)
	{
		if (!isCurrentlyRotating && isFaceRotating)
		{
			float elapsedTime = 0;

			InitRotation(axis, rotationIndex);

			// Rotate the group
			Quaternion quaternion = Quaternion.identity;
			switch (axis)
			{
				case RotationAxis.X:
					quaternion = Quaternion.Euler(angle, 0f, 0f);
					break;
				case RotationAxis.Y:
					quaternion = Quaternion.Euler(0f, angle, 0f);
					break;
				case RotationAxis.Z:
					quaternion = Quaternion.Euler(0f, 0f, angle);
					break;
			}
			
			while (elapsedTime < rotationTime && pivot != null)
			{
				pivot.transform.rotation = Quaternion.Lerp(pivot.transform.rotation, quaternion, (elapsedTime / rotationTime));
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			pivot.transform.rotation = quaternion;

			FinishRotation(axis, angle, rotationIndex);
			yield return new WaitForSeconds(0.1f);
		}
	}

	/// <summary>
	/// Resets cubie position in the Rubik's Cube data structure
	/// </summary>
	private void ResetPositionAfterRotation() 
	{
		Cubie[,,] newCube = new Cubie[PlayerSettings.CubeDimension, PlayerSettings.CubeDimension, PlayerSettings.CubeDimension];
		float offset = PlayerSettings.CubeDimension / 2f - 0.5f;

		// Creating new cubies
		for (int x = 0; x < PlayerSettings.CubeDimension; x++) {
			for (int y = 0; y < PlayerSettings.CubeDimension; y++) {
				for (int z = 0; z < PlayerSettings.CubeDimension; z++) 
				{
					// Going through each cubie of the current cube state
					for (int x2 = 0; x2 < PlayerSettings.CubeDimension; x2++) {
						for (int y2 = 0; y2 < PlayerSettings.CubeDimension; y2++) {
							for (int z2 = 0; z2 < PlayerSettings.CubeDimension; z2++) 
							{
								// Check each cubie current position (based on how the cubie positions were set during the Rubiks Cube instanciation)
								if (cube[x2, y2, z2].transform.position == new Vector3(x - offset, y - offset, z - offset))
								{
									newCube[x, y, z] = cube[x2, y2, z2];
								}
							}
						}
					}

				}
			}
		}

		// Set new array
		cube = newCube;
	}

	/// <summary>
	/// Scrambles the Rubik's cube
	/// </summary>
	/// <param name="scrambleTimes">Scrambles count</param>
	/// <param name="scrambleRotationTime">Scramble rotation time</param>
	/// <returns></returns>
	public IEnumerator ScrambleCube(int scrambleTimes, float scrambleRotationTime)
	{
		// Currently scrambling
		isScrambling = true;

		float oldRotationTime = rotationTime;
		rotationTime = scrambleRotationTime;

		for (int i = 0; i < scrambleTimes; i++)
		{
			isFaceRotating = true;

			// Randomize which axis, which column/row and which angle 
			int rotationType = UnityEngine.Random.Range(0, 3);
			int rotationIndex = UnityEngine.Random.Range(0, PlayerSettings.CubeDimension);
			int rotationAngle = UnityEngine.Random.Range(-1, 1) < 0 ? -90 : 90;

			yield return StartCoroutine(RotateAlong((RotationAxis)rotationType, rotationAngle, rotationIndex));
		}

		rotationTime = oldRotationTime;

		// Not scrambling anymore
		isScrambling = false;
	}

	/// <summary>
	/// Checks whether the Rubik's cube was solved or not
	/// </summary>
	/// <returns></returns>
	private bool isCubeSolved()
	{
		bool cubeSolved = true;

		foreach (string[,,] sCube in solvedCubes)
		{
			// New solved cube check, reset
			cubeSolved = true;

			// Cube is solved if each cubie position in the array is the same as the solved cube state
			for (int z = 0; z < PlayerSettings.CubeDimension; z++)
			{
				for (int y = 0; y < PlayerSettings.CubeDimension; y++)
				{
					for (int x = 0; x < PlayerSettings.CubeDimension; x++)
					{
						if (sCube[x, y, z] != cube[x, y, z].name)
						{
							cubeSolved = false;
						}
					}
				}
			}

			// Bool is still true after one state was check ? then it's solved
			if (cubeSolved)
			{
				return true;
			}
		}

		return cubeSolved;
	}

	/// <summary>
	/// Init a manual rotation done by the player
	/// </summary>
	/// <param name="axis">Axis rotation</param>
	/// <param name="rotationIndex">Rotation index</param>
	public void InitRotation(RotationAxis axis, int rotationIndex)
	{
		isCurrentlyRotating = true;

		// Create an empty GameObject to group cubies
		pivot = new GameObject();
		pivot.transform.position = new Vector3(0f, 0f, 0f);

		// Parent the targeted cubies to the newly created group
		for (int i = 0; i < PlayerSettings.CubeDimension; i++) {
			for (int j = 0; j < PlayerSettings.CubeDimension; j++) 
			{
				switch (axis) {
					case RotationAxis.X:
						cube[rotationIndex, i, j].transform.parent = pivot.transform;
						break;
					case RotationAxis.Y:
						cube[i, rotationIndex, j].transform.parent = pivot.transform;
						break;
					case RotationAxis.Z:
						cube[i, j, rotationIndex].transform.parent = pivot.transform;
						break;
						
				}
			}
		}
	}

	/// <summary>
	/// Rotation the pivot game object 
	/// </summary>
	/// <param name="axis">Axis rotation</param>
	/// <param name="angle">Angle</param>
	public void RotatePivot(RotationAxis axis, float angle)
	{
		// Rotate the group
		Quaternion quaternion = Quaternion.identity;
		switch (axis)
		{
			case RotationAxis.X:
				quaternion = Quaternion.Euler(angle, 0f, 0f);
				break;
			case RotationAxis.Y:
				quaternion = Quaternion.Euler(0f, angle, 0f);
				break;
			case RotationAxis.Z:
				quaternion = Quaternion.Euler(0f, 0f, angle);
				break;
		}
		pivot.transform.rotation = quaternion;
	}

	/// <summary>
	/// Finish manual rotation done by the player
	/// </summary>
	/// <param name="axis">Rotation axis</param>
	/// <param name="angle">Angle</param>
	/// <param name="rotationIndex">Rotation index</param>
	public void FinishRotation(RotationAxis axis, float angle, int rotationIndex, bool undo = false, bool gameWonCheck = true)
	{
		// Parent back the rotated cubies
		for (int i = 0; i < PlayerSettings.CubeDimension; i++) {
			for (int j = 0; j < PlayerSettings.CubeDimension; j++)
			{
				switch (axis)
				{
					case RotationAxis.X:
						cube[rotationIndex, i, j].transform.parent = cubeRoot.transform;
						break;
					case RotationAxis.Y:
						cube[i, rotationIndex, j].transform.parent = cubeRoot.transform;
						break;
					case RotationAxis.Z:
						cube[i, j, rotationIndex].transform.parent = cubeRoot.transform;
						break;
				}
			}
		}

		// Fix the location of the rotated cubes in the array and set the directions they now face
		ResetPositionAfterRotation();

		Destroy(pivot);
		isCurrentlyRotating = false;

		// Check if Rubik's cube was solved
		isGameWon = !isScrambling && gameWonCheck && isCubeSolved();

		// Rubik's cube solved
		if (isGameWon)
		{
			OnGameWon();
		}
		else if (!isScrambling && angle != 0 && !undo)
		{
			// Store move
			movesHistory.Push(new Move(axis, angle, rotationIndex));

			// Enable undo button
			if (!undoButton.interactable)
				undoButton.interactable = true;
		}

		// If redo moves history was not empty, clear
		if (redoMovesHistory.Count != 0 && !undo)
		{
			redoMovesHistory.Clear();
			redoButton.interactable = false;	
		}

		// Save data
		savedMoves.Enqueue(new Move(axis, angle, rotationIndex));

		isFaceRotating = false;
	}

	/// <summary>
	/// Simple rotation of a certain angle along an axis
	/// </summary>
	/// <param name="axis">Axis</param>
	/// <param name="angle">Angle</param>
	/// <param name="rotationIndex">Rotation index</param>
	/// <param name="undo">Whether the move should be stored (false) or not (true)</param>
	/// <param name="gameWonCheck">Whether the win condition should be checked or not</param>
	public void SimpleRotateAlong(RotationAxis axis, float angle, int rotationIndex, bool undo = false, bool gameWonCheck = true)
	{
		// Rotate
		InitRotation(axis, rotationIndex);
		RotatePivot(axis, angle);
		FinishRotation(axis, angle, rotationIndex, undo, gameWonCheck);
	}


	/// <summary>
	/// Undo move
	/// </summary>
	public void UndoMove()
	{
		// Not empty
		if (movesHistory.Count > 0)
		{
			// Pop move
			Move move = movesHistory.Pop();

			isFaceRotating = true;

			// Rotate
			InitRotation(move.Axis, move.Index);
			RotatePivot(move.Axis, -move.Angle);
			FinishRotation(move.Axis, -move.Angle, move.Index, true);

			// Store move in redo history
			redoMovesHistory.Push(new Move(move.Axis, -move.Angle, move.Index));

			// Enable redo button
			if (!redoButton.interactable)
				redoButton.interactable = true;
		}

		// Empty
		if (movesHistory.Count == 0)
		{
			undoButton.interactable = false;
		}
	}

	/// <summary>
	/// Redo move
	/// </summary>
	public void RedoMove()
	{
		// Not Empty
		if (redoMovesHistory.Count > 0)
		{
			// Pop move
			Move move = redoMovesHistory.Pop();

			isFaceRotating = true;

			// Rotate
			InitRotation(move.Axis, move.Index);
			RotatePivot(move.Axis, -move.Angle);
			FinishRotation(move.Axis, -move.Angle, move.Index, true);

			// Store move in redo history
			movesHistory.Push(new Move(move.Axis, -move.Angle, move.Index));

			// Enable redo button
			if (!undoButton.interactable)
				undoButton.interactable = true;
		}

		// Empty
		if (redoMovesHistory.Count == 0)
		{
			redoButton.interactable = false;
		}
	}

	/// <summary>
	/// Display or hide timer
	/// </summary>
	public void ToggleTimer()
	{
		PlayerSettings.ShowTimer = !PlayerSettings.ShowTimer;
	}

	/// <summary>
	/// Reload with confirmation modal
	/// </summary>
	public void ConfirmationReload()
	{
		// Show confirmation Modal
		confirmationModal.SetActive(true);

		// Setup modal
		confirmationModal.transform.Find("Text").GetComponent<Text>().text = "Are you sure you want to restart the game ?";

		Button yesButton = confirmationModal.transform.Find("YesButton").GetComponent<Button>();
		yesButton.onClick = new Button.ButtonClickedEvent();
		yesButton.onClick.AddListener(() =>
		{
			this.ReloadLevel();
		});
	}

	/// <summary>
	/// Return to title screen with confirmation modal
	/// </summary>
	public void ConfirmationToTitleScreen()
	{
		// Show confirmation Modal
		confirmationModal.SetActive(true);

		// Setup modal
		confirmationModal.transform.Find("Text").GetComponent<Text>().text = "Are you sure you want to quit the game ?";

		Button yesButton = confirmationModal.transform.Find("YesButton").GetComponent<Button>();
		yesButton.onClick = new Button.ButtonClickedEvent();
		yesButton.onClick.AddListener(() =>
		{
			this.ReturnToTitleScreenMenu();
		});
	}

	/// <summary>
	/// Reload level
	/// </summary>
	public void ReloadLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// Return to title screen
	/// </summary>
	public void ReturnToTitleScreenMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	/// <summary>
	/// On game won
	/// </summary>
	public void OnGameWon()
	{
		// Hide buttons
		undoButton.gameObject.SetActive(false);
		redoButton.gameObject.SetActive(false);
		timer.gameObject.SetActive(false);
		menuButton.SetActive(false);
		solvedText.SetActive(true);

		// Disable camera manipulation 
		PlayerSettings.AllowManipulations = false;

		// Get camera orbit
		GameObject cameraOrbit = GetComponent<InputManager>().cameraOrbit;
		StartCoroutine(ScaleOverTime(cameraOrbit, 1f));
	}


	/// <summary>
	/// Scale camera orbit over a certain time
	/// </summary>
	/// <param name="cameraOrbit">Camera Orbit object</param>
	/// <param name="time">Duration</param>
	/// <returns></returns>
	public IEnumerator ScaleOverTime(GameObject cameraOrbit, float time)
	{
		Vector3 originalScale = cameraOrbit.transform.localScale;
		Vector3 destinationScale = Vector3.one;

		// Initial zoom depends on cube dimension selection
		switch (PlayerSettings.CubeDimension)
		{
			case 2:
				destinationScale *= 5f;
				break;
			case 3:
				destinationScale *= 6f;
				break;
			case 4:
				destinationScale *= 7f;
				break;
			case 5:
				destinationScale *= 9f;
				break;
			case 6:
				destinationScale *= 12f;
				break;
		}

		float currentTime = 0.0f;

		do
		{
			// Animation
			cameraOrbit.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
			currentTime += Time.deltaTime;

			yield return null;
		} while (currentTime <= time);

		// Then rotate around Cube
		StartCoroutine(RotateFewTime(cameraOrbit, 2, 5));
	}

	/// <summary>
	/// Rotate camera around the cube a specified number of time and a certain duration
	/// </summary>
	/// <param name="cameraOrbit">Camera Orbit</param>
	/// <param name="rotationsNumber">Rotations number</param>
	/// <param name="time">Duration</param>
	/// <returns></returns>
	public IEnumerator RotateFewTime(GameObject cameraOrbit, int rotationsNumber, float time)
	{
		Quaternion startRot = cameraOrbit.transform.rotation;
		float currentTime = 0.0f;

		do
		{
			currentTime += Time.deltaTime;

			// Rotation
			cameraOrbit.transform.rotation = startRot * Quaternion.AngleAxis(currentTime / time * 360f * rotationsNumber, Vector3.right);

			yield return null;
		} while (currentTime <= time);

		// Hide solved text
		solvedText.SetActive(false);

		// Show results screen
		resultsScreen.SetActive(true);

		// Time
		TimeSpan t = TimeSpan.FromSeconds(time);
		string text = "It took you: ";

		if (t.Hours > 0)
		{
			text += t.Hours + " hours, " + t.Minutes + " minutes and " + t.Seconds + " seconds";
		}
		else if (t.Minutes > 0)
		{
			text += t.Minutes + " minutes and " + t.Seconds + " seconds";
		}
		else
		{
			text += t.Seconds + " seconds";
		}

		resultsScreen.transform.Find("Background/TimeText").GetComponent<Text>().text = text;
	}

	/// <summary>
	/// Allows manipulations
	/// </summary>
	public void AllowManipulations()
	{
		PlayerSettings.AllowManipulations = true;
	}

	/// <summary>
	/// Save game
	/// </summary>
	public void SaveGame()
	{
		// Show confirmation Modal
		confirmationModal.SetActive(true);

		// Setup modal
		confirmationModal.transform.Find("Text").GetComponent<Text>().text = "Be careful, saving the game now will erase any previous save. Are you sure ?";

		Button yesButton = confirmationModal.transform.Find("YesButton").GetComponent<Button>();
		yesButton.onClick = new Button.ButtonClickedEvent();
		yesButton.onClick.AddListener(() =>
		{
			SaveSystem.SaveMoves(savedMoves, time);
			confirmationModal.SetActive(false);
		});
	}
}
