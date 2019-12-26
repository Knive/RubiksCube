using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RubiksCube
{
	public class Face : MonoBehaviour
	{
		public Color color;

		private void Start()
		{
			// Apply color
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", color);
		}
	}
}
