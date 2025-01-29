using Sandbox;

public sealed class Slide : Component
{
	[Property] private GrabPoint GrabPoint { get; set; }
	[Property] private GrabPoint MainGrabPoint { get; set; }
	[Property] private GameObject ReferencePoint { get; set; }
	[Property] private Barrel Barrel { get; set; }
	[Property] private MagazineLoader MagazineLoader { get; set; }
	[Property] private ModelRenderer BulletVisual { get; set; }
	[Property] private float Distance { get; set; }
	[Property] private float BulletPickupPoint { get; set; } = 0.78f;
	[Property] public float PullBack { get; set; }
	[Property] private int HoldingBullet { get; set; }
	[Property] private string CasingGroup { get; set; } = "9mm_bullet";

	Vector3 origin;

	float minPullBack;
	protected override void OnStart()
	{
		origin = LocalPosition;
	}

	[Property] float visualPullBack;

	float lastPullBack;
	protected override void OnUpdate()
	{
		BulletVisual.Enabled = Barrel.Contents != -1 || HoldingBullet != -1;

		BulletVisual.SetBodyGroup( CasingGroup, HoldingBullet == -2 ? 1 : 0 );

		visualPullBack = MathX.Clamp( visualPullBack + (PullBack - visualPullBack) * 60 * Time.Delta, 0, 1 );

		LocalPosition = origin - ReferencePoint.LocalTransform.Forward * Distance * visualPullBack;

		if ( PullBack > 0 && lastPullBack == 0 )
			StartPullBack();
		if ( PullBack == 1 && lastPullBack < 1 )
			PulledBack();
		if ( PullBack < BulletPickupPoint && lastPullBack >= BulletPickupPoint )
			ExitPullBack();
		if ( PullBack == 0 && lastPullBack > 0 )
			Load();

		lastPullBack = PullBack;

		if (PullBack == 1 && MainGrabPoint.Held)
		{
			var controller = MainGrabPoint.Hand.Equals(VrhandInteraction.HandEnum.Left) ? Input.VR.LeftHand : Input.VR.RightHand;
			if ( controller.JoystickPress.IsPressed )
			{
				PullBack = 0;
				minPullBack = 0;
			}
		}

		if ( !GrabPoint.Held )
			PullBack = minPullBack;
		else
		{
			PullBack = MathX.Clamp( Vector3.Dot( -ReferencePoint.WorldTransform.Forward * Distance, GrabPoint.GrabbedHand.Reference.WorldPosition - ReferencePoint.WorldPosition ), 0, 1 );
			if(PullBack < 1)
				minPullBack = 0;
		}
	}

	void StartPullBack()
	{
		HoldingBullet = Barrel.Contents;

		Barrel.Contents = -1;
	}

	void PulledBack()
	{
		if ( HoldingBullet == -2 )
		{
			//eject casing
			HoldingBullet = -1;
		}
		if ( HoldingBullet != -1 )
		{
			//eject bullet
			HoldingBullet = -1;
		}
	}

	void ExitPullBack()
	{
		if ( !TryGetBullet() )
		{
			minPullBack = 1;
		}
	}

	bool TryGetBullet()
	{
		if ( !MagazineLoader.Magazine.IsValid() )
			return false;

		if ( MagazineLoader.Magazine.Contents.Count <= 0 )
			return false;

		HoldingBullet = MagazineLoader.Magazine.Contents[0];

		MagazineLoader.Magazine.Contents.RemoveAt( 0 );

		MagazineLoader.Magazine.UpdateVisuals();

		return true;
	}

	void Load()
	{
		if ( HoldingBullet == -1 )
			return;

		Barrel.Contents = HoldingBullet;
		HoldingBullet = -1;
	}

}
