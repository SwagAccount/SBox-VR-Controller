public sealed class Magazine : Component, Component.ITriggerListener
{
	[Property] public bool CantLoad { get; set; }
	[RequireComponent, Property] public Item Item { get; set; }
	[RequireComponent, Property] private BulletTypes BulletTypes { get; set; }
	[RequireComponent, Property] private MagazineVisualManager MagazineVisuals { get; set; }
	[Property] private GrabPoint GrabPoint { get; set; }
	[Property] public List<int> Contents { get; set; } = new();

	public void OnTriggerEnter( Collider other )
	{
		if ( !GrabPoint.Held )
			return;

		var item = other.GetComponent<Item>();

		var bulletType = BulletTypes.GetType(item.Name);

		if ( bulletType == -1 )
			return;

		Contents.Add( bulletType );

		MagazineVisuals.AmmoCount = Contents.Count;

		item.GrabPoints[0]?.GrabbedHand?.Drop();

		item.GameObject.Destroy();
	}

	public void UpdateVisuals()
	{
		MagazineVisuals.AmmoCount = Contents.Count;
	}
}
