using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RubiksCube
{
	/// <summary>
	/// Cubie class
	/// </summary>
	public class Cubie : MonoBehaviour
	{
		/// <summary>
		/// Each cubie are displayed like a 1 dimension Rubik's Cube with 6 faces: blue, green, white, yellow, orange and red
		/// </summary>
		private Face[] faces = new Face[6];

		void Start()
		{
			// Color
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);

			// Get all faces
			faces = GetComponentsInChildren<Face>();

			faces[0].color = Color.green;
			faces[1].color = Color.white;
			faces[2].color = Color.blue;
			faces[3].color = Color.yellow;
			faces[4].color = Color.red;
			faces[5].color = new Color(1.0f, 0.4f, 0.0f);
		}
	}
}
