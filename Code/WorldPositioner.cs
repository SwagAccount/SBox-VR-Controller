using Sandbox;

public sealed class WorldPositioner : Component
{
	[Property] public Vector3 World_Position;
	protected override void OnUpdate()
	{
		WorldPosition = World_Position;
	}
}
