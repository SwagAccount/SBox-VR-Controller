using Sandbox;
namespace XMovement;

public partial class PlayerWalkControllerComplex : Component
{
	[Property, Group( "Head" )] public GameObject Head { get; set; }
	[Property, Group( "Head" )] public float HeadHeight { get; set; } = 64f;
	[Property, Group( "Config" )] public float Height { get; set; } = 72f;
	[Sync] public Angles LocalEyeAngles { get; set; }
	public Angles EyeAngles
	{
		get
		{
			return LocalEyeAngles + GameObject.LocalRotation.Angles();
		}
		set
		{
			LocalEyeAngles = value - GameObject.LocalRotation.Angles();
		}
	}

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public virtual Ray AimRay => new( Head.WorldPosition + Camera.WorldRotation.Forward, Camera.WorldRotation.Forward );

	protected void SetupHead()
	{
		if ( !Head.IsValid() )
		{
			Head = Scene.CreateObject();
			Head.SetParent( GameObject );
			Head.Name = "Head";
			PositionHead();
		}
	}
	protected virtual void PositionHead()
	{
		if ( Head.IsValid() )
		{
			Head.WorldRotation = EyeAngles.ToRotation();
			Head.LocalPosition = new Vector3( 0, 0, HeadHeight + _smoothEyeHeight );
		}
	}

	private float _smoothEyeHeight;
	public float AimSensitivityScale = 1.0f;
	float LastSmoothEyeHeight = 0;
	public virtual void DoEyeLook()
	{
		if ( !IsProxy )
		{
			var eyeHeightOffset = GetEyeHeightOffset();
			_smoothEyeHeight = _smoothEyeHeight.LerpTo( eyeHeightOffset, Time.Delta * 10f );
			Controller.Height = Height + _smoothEyeHeight;

			LocalEyeAngles += Input.AnalogLook * AimSensitivityScale;
			LocalEyeAngles = LocalEyeAngles.WithPitch( LocalEyeAngles.pitch.Clamp( -89f, 89f ) );

			// This moves our feet up when crouching in air
			var delta = _smoothEyeHeight - LastSmoothEyeHeight;
			if ( !delta.AlmostEqual( 0 ) && !Controller.IsOnGround )
			{
				var delvel = Controller.Velocity;
				if ( Controller.MovementFrequency == PlayerMovement.MovementFrequencyMode.PerUpdate ) delvel *= Time.Delta;
				delvel *= WorldScale;
				var vel = delvel - new Vector3( 0, 0, (delta * WorldScale.z) / Time.Delta );
				Controller.MoveBy( vel, true );
			}
			LastSmoothEyeHeight = _smoothEyeHeight;
			PositionHead();
		}
	}
	protected float GetEyeHeightOffset()
	{
		if ( IsCrouching ) return -36f;
		return 0f;
	}
}
