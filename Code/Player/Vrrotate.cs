using Sandbox;
using System;

public sealed class Vrrotate : Component
{
	[Property] public bool DoSnap { get; set; } = false;
	[Property] public float SnapAngle { get; set; } = 30;
	[Property] public float RotateSpeed { get; set; } = 30;

	[Property] public GameObject Head { get; set; }

	float snapRotateTimer;

	protected override void OnPreRender()
	{
		if ( DoSnap )
		{
			bool Snap = false;
			if ( MathF.Abs( Input.VR.RightHand.Joystick.Value.x ) < 0.5 )
				snapRotateTimer = 0.4f;
			else
			{
				snapRotateTimer += Time.Delta;
				if ( snapRotateTimer >= 0.4f )
				{
					Snap = true;
					snapRotateTimer = 0;
				}
			}

			if ( Snap )
				RotateAroundPoint( GameObject, Head.WorldPosition, Vector3.Up, MathF.Round( -Input.VR.RightHand.Joystick.Value.x ) * SnapAngle );
		}
		else
			RotateAroundPoint( GameObject, Head.WorldPosition, Vector3.Up, Input.VR.RightHand.Joystick.Value.x * Time.Delta * -RotateSpeed );
	}

	static void RotateAroundPoint( GameObject objectToRotate, Vector3 point, Vector3 axis, float angle )
	{
		Vector3 dir = objectToRotate.WorldPosition - point;

		Rotation rotation = Rotation.FromAxis( axis, angle );

		dir = rotation * dir;

		objectToRotate.WorldPosition = point + dir;

		objectToRotate.WorldRotation = rotation * objectToRotate.WorldRotation;
	}
}
