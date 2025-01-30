using Sandbox;

public sealed class Recoiler : Component
{
	[RequireComponent, Property] private Item item { get; set; }
	[RequireComponent, Property] private GameObject Barrel { get; set; }
	[ Property] private float RecoilForce { get; set; } = 5000f;
	[Property] private float WeakenTo { get; set; } = 0.5f;
	[Property] private float WeakenRecover { get; set; } = 20f;

	RealTimeSince SleepClock { get; set; }
	public void Recoil()
	{
		SleepClock = 0;
		int heldCount = 0;
		foreach (var grabPoint in item.GrabPoints)
		{
			grabPoint.StrengthMult = WeakenTo;

			if(grabPoint.Held)
				heldCount++;
		}
		item.Body.ApplyImpulseAt( Barrel.WorldPosition, -Barrel.WorldTransform.Forward * 5000 / heldCount );
	}
	protected override void OnFixedUpdate()
	{
		if ( SleepClock > 2 )
			return;
		foreach ( var grabPoint in item.GrabPoints )
		{
			grabPoint.StrengthMult = MathX.Lerp( grabPoint.StrengthMult, 1, WeakenRecover * Time.Delta );
		}
	}
}
