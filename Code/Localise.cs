using Sandbox;

public sealed class Localise : Component
{
	CameraComponent CameraComponent { get; set; }
	[Property] private SkinnedModelRenderer MainBody { get; set; }
	[Property] private SkinnedModelRenderer ShadowBody { get; set; }
	[Property] private PlayerDresser Dresser { get; set; }
	[Property] private float EyeCheckDistance { get; set; } = 2.5f;

	protected override void OnStart()
	{
		CameraComponent = GetComponentInChildren<CameraComponent>(true);
		Dresser.Dress();
		if (IsProxy)
		{
			ShadowBody.GameObject.Destroy();
			return;
		}
		
		var clothes = MainBody.GetComponentsInChildren<ModelRenderer>().ToList();
		BBox eyeBox = BBox.FromPositionAndSize( CameraComponent.WorldPosition, EyeCheckDistance );
		
		foreach ( var clothing in clothes )
		{
			if ( clothing == MainBody )
				continue;

			BBox box = clothing.Model.Bounds;

			box = box.Transform( clothing.WorldTransform );

			if ( !eyeBox.Overlaps( box ) )
				continue;

			clothing.Enabled = false;
		}
	}
	protected override void OnUpdate()
	{
		CameraComponent.Enabled = !IsProxy;
		MainBody.RenderType = ModelRenderer.ShadowRenderType.On;

		if(!IsProxy)
			MainBody.SetBodyGroup( "Head", 3 );
	}
}
