using Sandbox;

public sealed class MagazineLoader : Component, Component.ITriggerListener
{
	[Property] public GrabPoint GrabPoint { get; set; }
	[Property] public Magazine Magazine { get; set; }
	[Property] public GameObject MagParent { get; set; }
	[Property] public GameObject MagDrop { get; set; }
	[Property] public List<string> AcceptedMags { get; set; }
	[Property] public float MagTime { get; set; } = 0.1f;

	public void OnTriggerEnter( Collider other )
	{
		
		if ( Magazine.IsValid() )
			return;

		var item = other.GetComponent<Item>();

		if ( !item.IsValid() )
			return;

		if ( !item.Held() )
			return;

		if ( !AcceptedMags.Contains( item.Name ) )
			return;

		var magazine = other.GetComponent<Magazine>();

		if ( !magazine.IsValid() )
			return;

		foreach ( GrabPoint grabPoint in item.GrabPoints )
		{
			grabPoint.GrabbedHand.Drop();
		}

		Magazine = magazine;

		item.Body.MotionEnabled = false;

		other.GameObject.SetParent( MagParent );

		other.LocalPosition = MagDrop.LocalPosition;

		other.LocalRotation = Rotation.Identity;

		other.Tags.Add( "uninteractable" );

		SlideT = 0;
		
	}

	bool Dropping;
	RealTimeSince SlideT;

	protected override void OnFixedUpdate()
	{
		if (Dropping)
		{

			Magazine.LocalPosition = Vector3.Lerp(MagParent.LocalPosition, MagDrop.LocalPosition,SlideT / MagTime);

			if(SlideT >= MagTime)
			{
				Magazine.Item.Body.MotionEnabled = true;
				Magazine.GameObject.SetParent( null );
				Magazine.GameObject.Tags.Remove( "uninteractable" );
				Magazine = null;
				Dropping = false;
			}
			return;
		}

		if ( !Magazine.IsValid() )
			return;

		if ( Magazine.GameObject.Parent != MagParent )
		{
			Magazine = null;
			return;
		}

		float lerp = MathX.Clamp( SlideT / MagTime, 0, 1 );

		Magazine.LocalPosition = Vector3.Lerp( MagDrop.LocalPosition, Vector3.Zero, lerp );

		if ( GrabPoint.Hand.Equals(VrhandInteraction.HandEnum.Left) ? Input.VR.LeftHand.ButtonB.IsPressed : Input.VR.RightHand.ButtonB.IsPressed )
		{
			Dropping = true;
		}
	}
}
