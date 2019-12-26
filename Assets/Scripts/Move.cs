using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RubiksCube
{
	/// <summary>
	/// Move are used to retrace user's actions
	/// </summary>
	public class Move
	{
		public RotationAxis Axis { get; private set; }
		public float Angle { get; private set; }
		public int Index { get; private set; }

		public Move(RotationAxis axis, float angle, int index)
		{
			this.Axis = axis;
			this.Angle = angle;
			this.Index = index;
		}

		public override string ToString()
		{
			return "(" + Axis + ", " + Angle + ", " + Index + ")";
		}
	}
}
