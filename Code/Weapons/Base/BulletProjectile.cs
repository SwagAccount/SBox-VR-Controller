using System;
using Sandbox;

public sealed class BulletProjectile : Component
{
	public Bullet bullet;
	public GameObject owner;
	public GameObject Firerer;
    private Rigidbody rB;
	Vector3 lastPos;

	//List<Vector3> poss;
	protected override void OnStart()
    {
		//poss = new List<Vector3> { WorldPosition };
		rB = Components.GetOrCreate<Rigidbody>();
        lastPos = WorldPosition;
    }
	bool hitSomething;
	protected override void OnUpdate()
	{
		//poss.Add(WorldPosition);
		if(hitSomething) return;
		
        var ray = Scene.Trace.Ray(lastPos,WorldPosition).Radius(bullet.Diameter/2).UseHitboxes().IgnoreGameObjectHierarchy(owner).Run();

		if(ray.Hit)
		{
			
			hitSomething = true;
			
			HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
			if(healthComponent != null)
			{
				float damageMult = 1;
				if(ray.Hitbox != null)
				{
					IEnumerable<string> tags = ray.Hitbox.Tags.TryGetAll();
					
					foreach(string s in tags)
					{
						if(float.TryParse(s, out damageMult)) break;
					}
				}
				float damage = CalcDamage(bullet.Grain,rB.Velocity.Length,bullet.Diameter)*damageMult;
				
				healthComponent.DoDamage(damage, owner);
			}
			rB.MotionEnabled = false;

			ray.Surface?.DoBulletImpact( ray );
			/*
			GameObject debugBox = new GameObject();
			debugBox.Components.Create<ModelRenderer>();
			debugBox.Transform.Scale = Vector3.One/50;
			debugBox.WorldPosition = ray.HitPosition;*/
			GameObject.Destroy();
		}
        lastPos = WorldPosition;
	}
	/*
	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Red;
        for (int i = 0; i < poss.Count - 1; i++)
        {
            Gizmo.Draw.Line(Transform.World.PointToLocal(poss[i]), Transform.World.PointToLocal(poss[i + 1]));
        }
	}*/
	public static float CalcDamage(float grain, float velocity, float diameter)
	{
		return MathF.Pow(grain,2f)*(velocity*12)/(700000*MathF.Pow(diameter,2f))*0.0012f;
	}
}
public static partial class ParticleExtentions
{
	public static LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, float decay = 5f )
	{
		var gameObject = Game.ActiveScene.CreateObject();
		gameObject.WorldPosition = pos;
		gameObject.Transform.Rotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		gameObject.Transform.ClearInterpolation();

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return p;
	}
	public static LegacyParticleSystem DoBulletImpact( this Surface self, SceneTraceResult tr )
	{
		//
		// Drop a decal
		//
		var decalPath = Game.Random.FromList( self.ImpactEffects.BulletDecal );

		var surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( decalPath ) && surf != null )
		{
			decalPath = Game.Random.FromList( surf.ImpactEffects.BulletDecal );
			surf = surf.GetBaseSurface();
		}

		if ( !string.IsNullOrWhiteSpace( decalPath ) )
		{
			if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decal ) )
			{
				var go = new GameObject
				{
					Name = decalPath,
					Parent = tr.GameObject,
					WorldPosition = tr.EndPosition,
					WorldRotation = Rotation.LookAt( -tr.Normal )
				};

				if ( tr.Bone > -1 )
				{
					var renderer = tr.GameObject.GetComponentInChildren<SkinnedModelRenderer>();
					var bone = renderer.GetBoneObject( tr.Bone );

					go.SetParent( bone );
				}

				var randomDecal = Game.Random.FromList( decal.Decals );

				var decalRenderer = go.AddComponent<DecalRenderer>();
				decalRenderer.Material = randomDecal.Material;
				decalRenderer.Size = new Vector3( randomDecal.Width.GetValue(), randomDecal.Height.GetValue(), randomDecal.Depth.GetValue() );

				go.NetworkSpawn( null );
				go.Network.SetOrphanedMode( NetworkOrphaned.Host );
				go.DestroyAsync( 10f );
			}
		}

		//
		// Make an impact sound
		//
		var sound = self.Sounds.Bullet;

		surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( sound ) && surf != null )
		{
			sound = surf.Sounds.Bullet;
			surf = surf.GetBaseSurface();
		}

		//
		// Get us a particle effect
		//

		string particleName = Game.Random.FromList( self.ImpactEffects.Bullet );
		if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Game.Random.FromList( self.ImpactEffects.Regular );

		surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( particleName ) && surf != null )
		{
			particleName = Game.Random.FromList( surf.ImpactEffects.Bullet );
			if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Game.Random.FromList( surf.ImpactEffects.Regular );

			surf = surf.GetBaseSurface();
		}

		if ( !string.IsNullOrWhiteSpace( particleName ) )
		{
			var go = new GameObject
			{
				Name = particleName,
				Parent = tr.GameObject,
				WorldPosition = tr.EndPosition,
				WorldRotation = Rotation.LookAt( tr.Normal )
			};

			var legacyParticleSystem = go.AddComponent<LegacyParticleSystem>();
			legacyParticleSystem.Particles = ParticleSystem.Load( particleName );
			legacyParticleSystem.ControlPoints = new()
			{
				new ParticleControlPoint { GameObjectValue = go, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
			};

			go.NetworkSpawn( null );
			go.Network.SetOrphanedMode( NetworkOrphaned.Host );
			go.DestroyAsync( 5f );

			return legacyParticleSystem;
		}

		return default;
	}
}



