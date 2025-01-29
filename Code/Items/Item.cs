using Sandbox;

public sealed class Item : Component
{
	[Property] public string Name { get; set; }
	[Property] public Rigidbody Body { get; set; }
	[Property] public List<GrabPoint> GrabPoints { get; set; }

	public bool Held()
	{
		foreach ( var grabPoint in GrabPoints )
		{
			if(grabPoint.Held)
				return true;
		}
		return false;
	}
}
