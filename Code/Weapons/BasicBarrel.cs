using Sandbox;

public sealed class BasicBarrel : Barrel
{
	[Property] public PistolSlide Slide { get; set; }

	public override void Fire()
	{
		base.Fire();
		Contents = -2;
		Slide.PullBack = 1;
	}
}
