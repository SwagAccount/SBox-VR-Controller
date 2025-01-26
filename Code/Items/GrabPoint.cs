using Sandbox;

public sealed class GrabPoint : Component
{
	[Property] public bool Held { get; set; }
	[Property] public VrhandInteraction.HandEnum Hand { get; set; }
	[Property] public Rigidbody Body { get; set; }

	[Property] public GameObject LeftHand { get; set; }
	[Property] public GameObject RightHand { get; set; }
	protected override void OnStart()
	{

	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.SolidSphere( Vector3.Zero, 0.5f );
	}
}
