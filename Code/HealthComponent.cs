using Sandbox;
public sealed class HealthComponent : Component
{
	[Property] public GameObject lastAttacker {get;set;}
	[Property] public float Health {get;set;}
	[Property] public float MaxHealth {get;set;}
	[Property] public SoundEvent Ouch {get;set;}
	[Property] public GameObject OuchObject {get;set;}
	float ouchTime;
	public void DoDamage(float Damage, GameObject from)
	{
		
		Health-=Damage;
		lastAttacker = from;
		ouchTime = Time.Now;
		if(Ouch!=null && Time.Now - ouchTime > 0.1f)
			Sound.Play(Ouch, OuchObject.WorldPosition);
	}
	protected override void OnStart()
	{
		if(OuchObject == null)
			OuchObject = GameObject;
	}
}
