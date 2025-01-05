public sealed class PlayerDresser : Component
{
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	public void Dress()
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( Network.Owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );
	}
}
