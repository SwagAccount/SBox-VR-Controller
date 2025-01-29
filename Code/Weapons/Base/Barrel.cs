public abstract class Barrel : Component
{
	[Property] public int Contents { get; set; } = -1;
	[Property] private BulletTypes BulletTypes { get; set; }
	[Property] public Recoiler Recoiler { get; set; }
	public virtual void TryFire()
	{
		if ( Contents < 0 )
		{
			BlankFire();
			return;
		}
		Fire();
	}

	public virtual void BlankFire()
	{
		Log.Info( "BlankFire" );
	}

	public virtual void Fire()
	{
		Recoiler?.Recoil();
		Log.Info( "Fire" );
	}
}
