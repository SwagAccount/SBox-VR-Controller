public sealed class PlayerDresser : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	[Property] public bool ShowHeadAlways { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );

		if(!IsProxy && !ShowHeadAlways)
			BodyRenderer.SetBodyGroup( "Head", 3 );
	}
}
