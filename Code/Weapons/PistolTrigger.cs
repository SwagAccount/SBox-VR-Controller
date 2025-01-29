using Sandbox;

public sealed class PistolTrigger : Component
{
	[Property] public GrabPoint GrabPoint { get; set; }
	[Property] public Slide Slide { get; set; }
	[Property] public Barrel Barrel { get; set; }
	[Property] public GameObject LeftIndex { get; set; }
	[Property] public GameObject RightIndex { get; set; }
	[Property] public GameObject OffTrigger { get; set; }
	[Property] public GameObject OnTrigger { get; set; }
	[Property] public GameObject TriggerDown { get; set; }

	float lastPullBack;
	protected override void OnUpdate()
	{
		if ( !GrabPoint.Held )
			return;

		var controller = GrabPoint.Hand.Equals(VrhandInteraction.HandEnum.Left) ? Input.VR.LeftHand : Input.VR.RightHand;

		if ( controller.GetFingerCurl( 1 ) < 0.1f )
			OffTriggerPose();
		else
			OnTriggerPose(controller.Trigger);

		if ( controller.Trigger >= 0.9f && lastPullBack <= 0.9f && Slide.PullBack == 0)
			Barrel.TryFire();

		lastPullBack = controller.Trigger;
	}

	void OffTriggerPose()
	{
		if ( GrabPoint.Hand.Equals( VrhandInteraction.HandEnum.Left ) )
		{
			VrhandInteraction.CopyTransformRecursive( OffTrigger, LeftIndex, new Vector3( 1, 1, -1 ), new Angles( -1, 1, -1 ), 10*Time.Delta );
		}
		else
		{
			VrhandInteraction.CopyTransformRecursive( OffTrigger, RightIndex, new Vector3( 1, 1, 1 ), new Angles( 1, 1, 1 ), 10 * Time.Delta );
		}
	}
	void OnTriggerPose(float pullBack)
	{
		if ( GrabPoint.Hand.Equals( VrhandInteraction.HandEnum.Left ) )
		{
			VrhandInteraction.CopyTransformRecursiveLerp( OnTrigger, TriggerDown, LeftIndex, new Vector3( 1, 1, -1 ), new Angles( -1, 1, -1 ), pullBack, 10 * Time.Delta );
		}
		else
		{
			VrhandInteraction.CopyTransformRecursiveLerp( OnTrigger, TriggerDown, RightIndex, new Vector3( 1, 1, 1 ), new Angles( 1, 1, 1 ), pullBack, 10 * Time.Delta );
		}
	}
}
