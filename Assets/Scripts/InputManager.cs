using RubiksCube;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	#region Public variables
	public GameObject cameraOrbit;
	public GameObject menu;
	#endregion

	private readonly int cubeLayerMask = 1 << 8;
	
	public static bool isCameraOrbiting;

	#region User Interaction
	private GameManager gameManager;
	private bool cubeTouched;
	private GameObject firstHitObject;
	private Vector3 firstHitNormal;
	private Vector3 initMousePos;
	private RotationAxis currRotationAxis;
	private int currRotationIndex;
	private float deltaValue;
	#endregion

	private void Start()
	{
		gameManager = transform.GetComponent<GameManager>();

		// Initial zoom depends on cube dimension selection
		switch (PlayerSettings.CubeDimension)
		{
			case 2:
				cameraOrbit.transform.localScale = Vector3.one * 5f;
				break;
			case 3:
				cameraOrbit.transform.localScale = Vector3.one * 6f;
				break;
			case 4:
				cameraOrbit.transform.localScale = Vector3.one * 7f;
				break;
			case 5:
				cameraOrbit.transform.localScale = Vector3.one * 9f;
				PlayerSettings.MinDistance = 5f;
				break;
			case 6:
				cameraOrbit.transform.localScale = Vector3.one * 12f;
				PlayerSettings.MinDistance = 6f;
				break;
		}
	}

	private void LateUpdate()
	{
		// If in menu, no interaction or if manipulations not allowed
		if (menu.activeSelf || !PlayerSettings.AllowManipulations)
		{
			return;
		}

		// Game not already won and scrambling not in processing ?
		if (!GameManager.isGameWon && !GameManager.isScrambling)
		{
			// Button down
			if (Input.GetMouseButtonDown(0))
			{
				// Check if Cube was clicked on
				Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit = new RaycastHit();
				cubeTouched = Physics.Raycast(r, out hit, cubeLayerMask);

				// Effectively touching the cube, and cube is not currently rotating ?
				if (cubeTouched && !isCameraOrbiting)
				{
					// Update status
					GameManager.isFaceRotating = true;

					// Get Cubie GO
					firstHitObject = hit.transform.parent.gameObject;
					firstHitNormal = new Vector3(Mathf.RoundToInt(hit.normal.x), Mathf.RoundToInt(hit.normal.y), Mathf.RoundToInt(hit.normal.z));

					// Initial mouse position (in world space)
					initMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
				}
			}

			// Left button up
			if (Input.GetMouseButtonUp(0))
			{
				if (GameManager.isFaceRotating && GameManager.isCurrentlyRotating)
				{
					// Round to nearest angle
					float finalRotation = RoundToNearest(deltaValue, 90f);

					// Rotate
					gameManager.RotatePivot(currRotationAxis, finalRotation);

					// Finish rotation
					gameManager.FinishRotation(currRotationAxis, finalRotation, currRotationIndex);
				}

				cubeTouched = false;
			}
		}

		// Cube touched
		if (cubeTouched)
		{
			// Pressed
			if (Input.GetMouseButton(0) && !isCameraOrbiting)
			{
				this.DetermineRotationAxis();
			}
		}
		// Rubik's cube rotation
		else
		{
			CameraManagement();
		}
	}

	/// <summary>
	/// Camera input management
	/// </summary>
	private void CameraManagement()
	{
		// Mouse left button down
		if (Input.GetMouseButtonDown(0)) {
			isCameraOrbiting = true;
		}

		// Mouse left button pressed
		if (Input.GetMouseButton(0))
		{
			float rotationX = Input.GetAxis("Mouse X") * PlayerSettings.RotationSpeed;
			float rotationY = Input.GetAxis("Mouse Y") * PlayerSettings.RotationSpeed;

			if (cameraOrbit.transform.eulerAngles.z + rotationY <= 0.1f || cameraOrbit.transform.eulerAngles.z + rotationY >= 179.9f) {
				rotationY = 0;
			}

			cameraOrbit.transform.eulerAngles = new Vector3(cameraOrbit.transform.eulerAngles.x, cameraOrbit.transform.eulerAngles.y + rotationX, cameraOrbit.transform.eulerAngles.z + rotationY);
		}	

		// Mouse left button up
		if (Input.GetMouseButtonUp(0))
		{
			isCameraOrbiting = false;
		}

		// Scroll
		float scrollFactor = Input.GetAxis("Mouse ScrollWheel");

		if (scrollFactor < 0 && cameraOrbit.transform.localScale.x <= PlayerSettings.MaxDistance || scrollFactor > 0 && cameraOrbit.transform.localScale.x >= PlayerSettings.MinDistance)
		{
			cameraOrbit.transform.localScale = cameraOrbit.transform.localScale * (1f - scrollFactor);
		}
	}

	/// <summary>
	/// Determine rotation axis by checking raycast hit normal
	/// </summary>
	private void DetermineRotationAxis()
	{
		// Project mouse position on screen to world value position (as our camera orbits around the object, we really need world position value)
		Vector3 currMousePosInWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
		Vector3 delta = (currMousePosInWorld - initMousePos) * PlayerSettings.FaceRotationSpeed;
		float offset = PlayerSettings.CubeDimension / 2f - 0.5f;

		// Rotation along X or Z
		if (firstHitNormal == Vector3.up || firstHitNormal == Vector3.down)
		{
			// Not currently rotating
			if (!GameManager.isCurrentlyRotating)
			{
				// Rotate along Z
				if (Mathf.Abs(delta.x) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.Z;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.z + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
				// Rotate along X
				else if (Mathf.Abs(delta.z) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.X;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.x + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
			}
			// Rotation already initialized
			else
			{
				// Delta value
				deltaValue = (currRotationAxis == RotationAxis.Z ? -delta.x : delta.z);

				// Clockwise or anticlockwise rotation ?
				if (firstHitNormal == Vector3.down)
				{
					deltaValue = -deltaValue;
				}

				// Rotate pivot
				gameManager.RotatePivot(currRotationAxis, deltaValue);
			}
		}

		// Rotation along Y or Z
		else if (firstHitNormal == Vector3.left || firstHitNormal == Vector3.right)
		{
			// Not currently rotating
			if (!GameManager.isCurrentlyRotating)
			{
				// Rotate along Z
				if (Mathf.Abs(delta.y) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.Z;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.z + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
				// Rotate along Y
				else if (Mathf.Abs(delta.z) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.Y;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.y + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
			}
			// Rotation already initialized
			else
			{
				// Delta value
				deltaValue = (currRotationAxis == RotationAxis.Z ? -delta.y : delta.z);

				// Clockwise or anticlockwise rotation ?
				if (firstHitNormal == Vector3.right)
				{
					deltaValue = -deltaValue;
				}

				// Rotate pivot
				gameManager.RotatePivot(currRotationAxis, deltaValue);
			}
		}

		// Rotation along X or Y
		else if (firstHitNormal == Vector3.forward || firstHitNormal == Vector3.back)
		{
			// Not currently rotating
			if (!GameManager.isCurrentlyRotating)
			{
				// Rotate along X
				if (Mathf.Abs(delta.y) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.X;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.x + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
				// Rotate along Y
				else if (Mathf.Abs(delta.x) >= PlayerSettings.DragThreshold)
				{
					currRotationAxis = RotationAxis.Y;
					currRotationIndex = Mathf.RoundToInt(firstHitObject.transform.position.y + offset);
					gameManager.InitRotation(currRotationAxis, currRotationIndex);
				}
			}
			// Rotation already initialized
			else
			{
				// Delta value
				deltaValue = (currRotationAxis == RotationAxis.X ? -delta.y : delta.x);

				// Clockwise or anticlockwise rotation ?
				if (firstHitNormal == Vector3.back)
				{
					deltaValue = -deltaValue;
				}

				// Rotate pivot
				gameManager.RotatePivot(currRotationAxis, deltaValue);
			}
		}
	}

	/// <summary>
	/// Round value to nearest multiple of another value
	/// </summary>
	/// <param name="value">Value to round</param>
	/// <param name="multiple">Multiple</param>
	/// <returns>Rounded value</returns>
	private float RoundToNearest(float value, float multiple)
	{
		return Mathf.Round(value / multiple) * multiple;
	}
}
