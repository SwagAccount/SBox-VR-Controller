using Sandbox;
using System.Numerics;

public sealed class Gravityrotatetest : Component
{

	[Property] private Vector3 Gravity = Vector3.Down;
	[Property] private float GravSmoothness = 10;


	Rotation baseRotation;
	protected override void OnUpdate()
	{
		baseRotation = Rotation.Lerp( baseRotation, Rotation.LookAt( Gravity ), GravSmoothness * Time.Delta );

		WorldRotation = baseRotation;
	}
}
