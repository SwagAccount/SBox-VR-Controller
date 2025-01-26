using Sandbox;

public sealed class RecoilTest : Component
{
	[Property] private GameObject Barrel { get; set; }
	float lastTrigger;
	protected override void OnUpdate()
	{
		if ( Input.VR.RightHand.Trigger > 0.75f && lastTrigger <= 0.75f )
		{
			GetComponent<Rigidbody>().ApplyImpulseAt( Barrel.WorldPosition, -WorldTransform.Forward * 5000 );
			Gizmo.Draw.Arrow( WorldPosition, WorldPosition + WorldTransform.Forward * 20 );
		}
		lastTrigger = Input.VR.RightHand.Trigger;
	}

	
}
