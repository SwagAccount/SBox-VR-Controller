using Sandbox;

public sealed class MagazineVisualManager : Component, Component.ExecuteInEditor
{
	int _ammoCount;
	[Property] public int AmmoCount 
	{ 
		get
		{
			return _ammoCount;
		}
		
		set
		{
			_ammoCount = value;
			UpdateVisuals();
		}
	}

	[Property] public int AmmoMax { get; set; } = 15;
	[Property] public string BodyGroup { get; set; }
	[Property] private ModelRenderer ModelRenderer { get; set; }
	[Property] private GameObject Piston { get; set; }
	[Property] private Vector3 PistonEndPosition { get; set; }
	protected override void OnStart()
	{
		UpdateVisuals();
	}

	void UpdateVisuals()
	{
		if ( !ModelRenderer.IsValid() )
			return;
		
		ModelRenderer.SetBodyGroup( BodyGroup, AmmoCount );

		if ( !Piston.IsValid() )
			return;

		Piston.LocalPosition = PistonEndPosition * (AmmoCount / (float)AmmoMax);
	}
}
