public sealed class BulletTypes : Component
{
	[Property] public List<Bullet> Bullets { get; set; }

	public int GetType( string name )
	{
		for ( int i = 0; i < Bullets.Count; i++ )
		{
			if ( Bullets[i].ResourceName == name )
				return i;
		}
		return -1;
	}
}
