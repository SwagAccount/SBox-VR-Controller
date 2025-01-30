using Sandbox;
using System;

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

	[Property] public int AmmoMax { get; set; }
	[Property] public int AmmoVisualMax { get; set; }
	[Property] public string BodyGroup { get; set; } = "Bullets";
	[Property] private ModelRenderer ModelRenderer { get; set; }
	[Property] private GameObject Piston { get; set; }
	[Property] private Vector3 PistonEndPosition { get; set; }
	[Property] private int PistonEndBullet { get; set; }
	protected override void OnStart()
	{
		UpdateVisuals();
	}

	void UpdateVisuals()
	{
		if ( !ModelRenderer.IsValid() )
			return;
		
		ModelRenderer.SetBodyGroup( BodyGroup,  Math.Clamp(AmmoCount,0,AmmoVisualMax) );

		if ( !Piston.IsValid() )
			return;

		if(AmmoCount <= PistonEndBullet)
			Piston.LocalPosition = PistonEndPosition * ( AmmoCount / (float)PistonEndBullet);

		Piston.Enabled = AmmoCount <= PistonEndBullet;
	}
}
