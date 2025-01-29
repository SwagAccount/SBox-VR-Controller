public sealed class TimedDestroyComponent : Component
{
	[Property] public float Time { get; set; } = 1f;

	[Property, ReadOnly] TimeUntil TimeUntilDestroy { get; set; } = 0;

	protected override void OnStart()
	{
		TimeUntilDestroy = Time;
	}

	protected override void OnUpdate()
	{
		if ( TimeUntilDestroy )
		{
			GameObject.Destroy();
		}
	}
}

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Creates a <see cref="TimedDestroyComponent"/> which will deferred delete the <see cref="GameObject"/>.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="seconds"></param>
	public static void DestroyAsync( this GameObject self, float seconds = 1.0f )
	{
		var component = self.Components.Create<TimedDestroyComponent>();
		component.Time = seconds;
	}
}
