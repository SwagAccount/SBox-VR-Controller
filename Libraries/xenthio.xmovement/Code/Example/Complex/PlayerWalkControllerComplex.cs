using Sandbox;
namespace XMovement;

public partial class PlayerWalkControllerComplex : Component, Component.ExecuteInEditor
{
	[RequireComponent] public PlayerMovement Controller { get; set; }
	protected override void OnStart()
	{
		base.OnStart();
		if ( !IsProxy )
		{
			SetupBody();
			SetupHead();
			SetupCamera();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !Game.IsPlaying ) return;

		if ( !IsProxy )
		{
			UpdateCamera();
			DoEyeLook();
			BuildFrameInput();
			if ( Controller.MovementFrequency == PlayerMovement.MovementFrequencyMode.PerUpdate ) DoMovement();
		}
		Animate();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( !Game.IsPlaying ) return;

		if ( !IsProxy )
		{
			if ( Controller.MovementFrequency == PlayerMovement.MovementFrequencyMode.PerFixedUpdate ) DoMovement();
		}
		Animate();
	}
}
