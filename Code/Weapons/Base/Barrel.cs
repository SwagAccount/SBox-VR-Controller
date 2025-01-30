using static Sandbox.Services.Inventory;

public abstract class Barrel : Component
{
	[Property] public int Contents { get; set; } = -1;
	[Property] public BulletTypes BulletTypes { get; set; }
	[Property] public Recoiler Recoiler { get; set; }
	[Property] public SoundEvent GunShot { get; set; }
	[Property] public SoundEvent DryFire { get; set; }
	[Property] public GameObject MuzzleFlashPrefab { get; set; }
	[Property] public float VelocityMultiplier { get; set; } = 1f;
	[Property] public Vector2 SpreadMult { get; set; } = Vector3.One;

	Vector3 CalculateSpread( Bullet bullet )
	{
		return WorldTransform.Forward +
		(WorldTransform.Right * (bullet.Spread.x * SpreadMult.x) * (Game.Random.Next( -100, 100 ) / 100f)) +
		(WorldTransform.Up * (bullet.Spread.y * SpreadMult.y) * (Game.Random.Next( -100, 100 ) / 100f));
	}
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
		Sound.Play( DryFire, WorldPosition );
	}

	bool hasFired;
	public virtual void Fire()
	{
		Recoiler?.Recoil();
		hasFired = true;
		//item.Controller.TriggerHapticVibration( 1, 1, 1 );
		Bullet bullet = BulletTypes.Bullets[Contents];

		for ( int i = 0; i < bullet.Count; i++ )
		{
			GameObject bulletObject = new GameObject();
			bulletObject.WorldPosition = WorldPosition;
			bulletObject.WorldRotation = WorldRotation;
			Rigidbody bulletBody = bulletObject.Components.Create<Rigidbody>();
			bulletBody.Velocity = CalculateSpread( bullet ) * VelocityMultiplier * bullet.BaseVelocity * 12;
			BulletProjectile bulletProjectile = bulletObject.Components.Create<BulletProjectile>();
			bulletProjectile.owner = GameObject.Root;
			bulletProjectile.Firerer = GameObject.Parent;
			bulletProjectile.bullet = bullet;
		}
		Sound.Play( GunShot, WorldPosition );
		GameObject flash = MuzzleFlashPrefab.Clone( new CloneConfig()
		{
			Parent = GameObject,
			Transform = new(),
			StartEnabled = true
		} );
		flash.DestroyAsync( 5 );
	}


}
