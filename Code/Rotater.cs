using Sandbox;

public sealed class Rotater : Component
{
	[Property] public Angles Rotation { get; set; }
	protected override void OnUpdate()
	{
		WorldRotation = WorldRotation.Angles() + Rotation ;	
	}
}
