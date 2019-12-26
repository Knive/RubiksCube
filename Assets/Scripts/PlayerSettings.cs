using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RubiksCube
{
	/// <summary>
	/// Manages settings
	/// </summary>
	public static class PlayerSettings
	{
		/// <summary>
		/// Cube dimension, e.g. 2x2x2 or 6x6x6
		/// </summary>
		public static int CubeDimension;
		/// <summary>
		/// Display timer or not
		/// </summary>
		public static bool ShowTimer = true;
		/// <summary>
		/// Allows camera manipulation
		/// </summary>
		public static bool AllowManipulations = true;
		/// <summary>
		/// Indicates that we are loading a game
		/// </summary>
		public static bool LoadGame = false;
		/// <summary>
		/// Camera rotation speed using a mouse
		/// </summary>
		public static float RotationSpeed = 15f;
		/// <summary>
		/// Rubik' cube face rotation speed using a mouse
		/// </summary>
		public static float FaceRotationSpeed = 1000f;
		/// <summary>
		/// Camera zoom min distance using mouse
		/// </summary>
		public static float MinDistance = 4f;
		/// <summary>
		/// Camera zoom max distance using mouse
		/// </summary>
		public static float MaxDistance = 15f;
		/// <summary>
		/// Minimum drag distance to initiate a face rotation
		/// </summary>
		public static float DragThreshold = 3f;
	}
}
