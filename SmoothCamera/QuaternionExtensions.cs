using UnityEngine;

namespace SmoothCamera
{
	public static class QuaternionExtensions
	{
		public static Vector3 Axis(this Quaternion quaternion)
		{
			float _;
			Vector3 axis;
			quaternion.ToAngleAxis(out _, out axis);
			return axis;
		}
	}
}
