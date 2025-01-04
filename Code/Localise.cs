using Sandbox;

public sealed class Localise : Component
{
	CameraComponent CameraComponent { get; set; }
	[Property] private SkinnedModelRenderer MainBody { get; set; }
	[Property] private SkinnedModelRenderer ShadowBody { get; set; }

	protected override void OnStart()
	{
		CameraComponent = GetComponentInChildren<CameraComponent>();
		if (IsProxy)
		{
			ShadowBody.GameObject.Destroy();
		}
	}
	protected override void OnUpdate()
	{
		CameraComponent.Enabled = !IsProxy;
		MainBody.RenderType = ModelRenderer.ShadowRenderType.On;

		if ( !IsProxy )
			MainBody.SetBodyGroup( "Head", 3 );
	}
}
