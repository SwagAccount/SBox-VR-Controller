using Sandbox;
using System;
namespace XMovement;

public partial class PlayerMovement : Component
{
	[Range( 0, 200 )]
	[Property] public float Radius { get; set; } = 16.0f;

	[Range( 0, 200 )]
	[Property] public float Height { get; set; } = 72.0f;

	[Range( 0, 50 )]
	[Property] public float StepHeight { get; set; } = 18.0f;

	[Range( 0, 90 )]
	[Property] public float GroundAngle { get; set; } = 46.0f;

	[Range( 0, 64 )]
	[Property] public float Acceleration { get; set; } = 10.0f;

	/// <summary>
	/// When jumping into walls, should we bounce off or just stop dead?
	/// </summary>
	[Range( 0, 1 )]
	[Property] public float Bounciness { get; set; } = 0.0f;

	/// <summary>
	/// If enabled, determine what to collide with using current project's collision rules for the <see cref="GameObject.Tags"/>
	/// of the containing <see cref="GameObject"/>.
	/// </summary>
	[Property, Group( "Collision" ), Title( "Use Project Collision Rules" )] public bool UseCollisionRules { get; set; } = false;

	[Property, Group( "Collision" ), HideIf( nameof( UseCollisionRules ), true )]
	public TagSet IgnoreLayers { get; set; } = new();

	public BBox BoundingBox => new BBox( CenterOffset + new Vector3( -Radius, -Radius, 0 ), CenterOffset + new Vector3( Radius, Radius, Height ) );

	[ReadOnly, Property, Sync]
	public Vector3 Velocity { get; set; }

	[ReadOnly, Property, Sync]
	public Vector3 BaseVelocity { get; set; }

	[ReadOnly, Property, Sync]
	public bool IsOnGround { get; set; }

	public Vector3 PreviousPosition { get; set; }

	public GameObject PreviousGroundObject { get; set; }
	public GameObject GroundObject { get; set; }
	public Collider GroundCollider { get; set; }
	public Vector3 GroundNormal { get; set; }
	public float SurfaceFriction { get; set; } = 1.0f;

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineBBox( BoundingBox );
	}

	/// <summary>
	/// Move up and leave the ground, great for jumping.
	/// </summary>
	public void LaunchUpwards( float amount )
	{
		ClearGround();
		Velocity += Vector3.Up * amount;
		Velocity -= Gravity * Time.Delta * 0.5f;
	}

	/// <summary>
	/// Add acceleration to the current velocity. 
	/// No need to scale by time delta - it will be done inside.
	/// </summary>
	public void Accelerate( Vector3 vector )
	{
		//Velocity = Velocity.WithAcceleration( vector, Acceleration * Time.Delta );
		Accelerate( vector, Acceleration );
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public virtual void Accelerate( Vector3 vector, float acceleration )
	{
		// This gets overridden because some games (CSPort) want to allow dead (observer) players
		// to be able to move around.
		// if ( !CanAccelerate() )
		//     return; 

		var wishdir = vector.Normal;
		var wishspeed = vector.Length;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = acceleration * wishspeed * Time.Delta * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Apply an amount of friction to the current velocity.
	/// No need to scale by time delta - it will be done inside.
	/// </summary>
	public Vector3 ApplyFriction( Vector3 velocity, float frictionAmount, float stopSpeed = 140.0f )
	{
		var speed = velocity.Length;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < stopSpeed) ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return velocity;

		newspeed /= speed;
		return velocity * newspeed;
	}

	public SceneTrace BuildTrace( Vector3 from, Vector3 to, float liftFeet = 0.0f )
	{
		var box = BoundingBox;
		if ( liftFeet > 0 )
		{
			from += Vector3.Up * liftFeet;
			box.Maxs = box.Maxs.WithZ( box.Maxs.z - liftFeet );
		}
		box.Mins *= WorldScale;
		box.Maxs *= WorldScale;

		var source = Scene.Trace.Ray( from, to );
		var trace = source.Size( box ).IgnoreGameObjectHierarchy( GameObject );

		return UseCollisionRules ? trace.WithCollisionRules( Tags ) : trace.WithoutTags( IgnoreLayers );
	}

	/// <summary>
	/// Trace the controller's current position to the specified delta
	/// </summary>
	public SceneTraceResult TraceDirection( Vector3 direction )
	{
		return BuildTrace( GameObject.WorldPosition, GameObject.WorldPosition + direction ).Run();
	}
	public void MoveBy( Vector3 delta, bool step )
	{
		if ( step && IsOnGround )
		{
			//Velocity = Velocity.WithZ( 0 );
		}


		var pos = GameObject.WorldPosition;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, delta );
		mover.Bounce = Bounciness;
		mover.MaxStandableAngle = GroundAngle;

		if ( step && IsOnGround )
		{
			mover.TryMoveWithStep( Time.Delta, StepHeight );
		}
		else
		{
			mover.TryMove( Time.Delta );
		}

		WorldPosition = mover.Position;
	}
	void Move( bool step )
	{
		if ( step && IsOnGround )
		{
			//Velocity = Velocity.WithZ( 0 );
		}


		var pos = GameObject.WorldPosition;

		Velocity *= WorldScale;

		Velocity += BaseVelocity;

		Velocity += PhysicsBodyVelocity;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, Velocity );
		mover.Bounce = Bounciness;
		mover.MaxStandableAngle = GroundAngle;

		if ( step && IsOnGround )
		{
			mover.TryMoveWithStep( Time.Delta, StepHeight * WorldScale.z );
		}
		else
		{
			mover.TryMove( Time.Delta );
		}

		WorldPosition = mover.Position;
		Velocity = mover.Velocity;
		Velocity -= BaseVelocity;
		Velocity -= PhysicsBodyVelocity;

		Velocity /= WorldScale;
	}

	void CategorizePosition()
	{
		var point = WorldPosition + ((Vector3.Down * 2f) * WorldScale.z);
		var vBumpOrigin = WorldPosition;
		var moveToEndPos = IsOnGround;

		// We're flying upwards too fast, never land on ground
		if ( Velocity.z + PhysicsBodyVelocity.z > 140.0f )
		{
			ClearGround();
			return;
		}

		point.z -= (IsOnGround ? StepHeight : 0.1f);

		var pm = BuildTrace( vBumpOrigin, point, 0.0f ).Run();

		//
		// we didn't hit - or the ground is too steep to be ground
		//

		if ( !pm.Hit || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGround();

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;

			return;
		}

		//
		// we are on ground
		//
		ChangeGround( pm.GameObject, pm.Shape?.Collider as Collider, pm.Normal );

		var posDelta = (WorldPosition - PreviousPosition);

		//
		// move to this ground position, if we moved, and hit
		//
		if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f && posDelta.z <= 3f )
		{
			WorldPosition = pm.EndPosition;
		}
	}

	/// <summary>
	/// Disconnect from ground and punch our velocity. This is useful if you want the player to jump or something.
	/// </summary>
	public void Punch( in Vector3 amount )
	{
		ClearGround();
		Velocity += amount;
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public virtual void ClearGround()
	{
		if ( IsOnGround )
		{
			Velocity += PhysicsBodyVelocity;
			PhysicsBodyVelocity = Vector3.Zero;
			PhysicsBodyRigidbody.Velocity = Vector3.Zero;
		}

		PreviousGroundObject = GroundObject;
		IsOnGround = false;
		GroundObject = default;
		GroundCollider = default;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// We have a new ground
	/// </summary>
	public virtual void ChangeGround( GameObject gameObject, Collider collider, Vector3 normal )
	{
		PreviousGroundObject = GroundObject;
		IsOnGround = true;
		GroundObject = gameObject;
		GroundCollider = collider;
		GroundNormal = normal;
		if ( collider.IsValid() )
		{
			BaseVelocity = collider.SurfaceVelocity * collider.WorldRotation;
			// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
			// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
			// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
			SurfaceFriction = collider?.Surface?.Friction ?? 0.8f * 1.25f;
			if ( SurfaceFriction > 1 ) SurfaceFriction = 1;
		}
		else
		{
			BaseVelocity = Vector3.Zero;
			SurfaceFriction = 1;
		}
	}

	/// <summary>
	/// Move from our current position to this target position, but using tracing an sliding.
	/// This is good for different control modes like ladders and stuff.
	/// </summary>
	public void MoveTo( Vector3 targetPosition, bool useStep )
	{
		if ( TryUnstuck() )
			return;

		var pos = WorldPosition;
		var delta = targetPosition - pos;

		var mover = new CharacterControllerHelper( BuildTrace( pos, pos ), pos, delta );
		mover.MaxStandableAngle = GroundAngle;

		if ( useStep )
		{
			mover.TryMoveWithStep( 1.0f, StepHeight );
		}
		else
		{
			mover.TryMove( 1.0f );
		}

		WorldPosition = mover.Position;
	}

	int _stuckTries;

	bool IsStuck()
	{
		var result = BuildTrace( WorldPosition, WorldPosition ).Run();
		return result.StartedSolid;
	}
	[ConVar] public static bool debug_playermovement_unstick { get; set; } = false;
	Transform _previousTransform;
	bool TryUnstuck()
	{

		var result = BuildTrace( WorldPosition, WorldPosition ).Run();

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			_previousTransform = Transform.World;
			return false;
		}

		/*using ( Gizmo.Scope( "unstuck", Transform.World ) )
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.LineBBox( BoundingBox );
		}*/

		int AttemptsPerTick = 150;

		var normal = Vector3.Zero;
		var pos = WorldPosition;
		var startpos = WorldPosition;
		for ( int i = 0; i < AttemptsPerTick; i++ )
		{

			// First try the where ever the physics body is, if we have one.
			/*if ( i <= 1 && PhysicsBodyRigidbody.IsValid() )
			{
				pos = PhysicsBodyRigidbody.WorldPosition + ((PhysicsBodyRigidbody.Velocity * Time.Delta) * i);
				if ( debug_playermovement_unstick ) DebugOverlay.Box( BoundingBox, Color.Cyan, 2, Transform.World.WithRotation( Rotation.Identity ) );
			}*/

			// this can solve so many issues super quickly so do this first.
			if ( i <= 2 )
			{
				pos = WorldPosition + Vector3.Up * ((i) * 0.2f);
				if ( debug_playermovement_unstick ) DebugOverlay.Box( BoundingBox, Color.Cyan, 2, Transform.World.WithRotation( Rotation.Identity ) );
			}
			// Try base velocity 
			if ( (PhysicsBodyVelocity.Length > 0 || (PhysicsBodyRigidbody.IsValid() && PhysicsBodyRigidbody.WorldPosition != WorldPosition)) && i < 80 )
			{
				normal = PhysicsBodyVelocity.Normal * Time.Delta;
				normal.z = Math.Max( 0, normal.z );
				normal *= 1f;
				if ( i < 0 )
				{
					pos = PhysicsBodyRigidbody.WorldPosition;
				}
				else
				{
					var searchdistance = 0.2f;
					if ( i > 70 ) searchdistance = 1f;
					if ( i > 75 ) searchdistance = 3f;
					normal *= searchdistance;
					pos += normal;
					if ( debug_playermovement_unstick ) DebugOverlay.Line( WorldPosition, pos, Color.Yellow, 2 );
					/*using ( Gizmo.Scope( "unstuck2", new Transform() ) )
					{
						Gizmo.Draw.Color = Color.Yellow;
						Gizmo.Draw.Line( WorldPosition, WorldPosition + normal * 12 );
					}*/

				}
				if ( debug_playermovement_unstick ) DebugOverlay.Box( BoundingBox, Color.Green, 2, Transform.World.WithRotation( Rotation.Identity ) );

				/*using ( Gizmo.Scope( "unstuck3", pos ) )
				{
					Gizmo.Draw.Color = Color.Green;
					Gizmo.Draw.LineBBox( BoundingBox );
				}*/
			}
			// Second try the up direction for moving platforms
			else if ( i < 4 )
			{
				pos = WorldPosition + Vector3.Up * ((i) * 3f);
				if ( debug_playermovement_unstick ) DebugOverlay.Box( BoundingBox, Color.Yellow, 2, Transform.World.WithRotation( Rotation.Identity ) );
			}
			else
			{
				normal = Vector3.Random.Normal * (((float)_stuckTries) * 1.25f);
				if ( debug_playermovement_unstick ) DebugOverlay.Line( WorldPosition, pos, Color.Blue, 2 );
				pos = WorldPosition + normal;
				normal *= 0.25f;
				//normal.ClampLength( 0, 10 );
				if ( debug_playermovement_unstick ) DebugOverlay.Box( BoundingBox, Color.Magenta, 2, Transform.World.WithRotation( Rotation.Identity ) );
			}
			/*using ( Gizmo.Scope( "unstuck4", new Transform() ) )
			{
				Gizmo.Draw.Color = Color.Blue;
				Gizmo.Draw.Line( WorldPosition, pos );
			}*/
			result = BuildTrace( pos, pos ).Run();

			if ( !result.StartedSolid )
			{
				//Log.Info( $"unstuck after {_stuckTries} tries ({_stuckTries * AttemptsPerTick} tests)" );
				Velocity += normal / Time.Delta;
				WorldPosition = pos;
				_previousTransform = Transform.World;
				return false;
			}
		}

		_stuckTries++;

		_previousTransform = Transform.World;
		return true;
	}
}
