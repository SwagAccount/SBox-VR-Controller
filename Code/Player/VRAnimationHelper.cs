using Editor;
using Sandbox.Services;
using System;
using System.Numerics;
using static Sandbox.Citizen.VRAnimationHelper;

namespace Sandbox.Citizen;

[Title( "VR Animation Helper" )]
[Category( "Citizen" )]
[Icon( "directions_run" )]
[Alias( "CitizenAnimation" )]
public sealed class VRAnimationHelper : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// The skinned model renderer that we'll apply all parameters to.
	/// </summary>
	[Property] public SkinnedModelRenderer Target { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public GameObject TargetHead { get; set; }
	[Property] public GameObject TargetHandLeft { get; set; }
	[Property] public GameObject TargetHandRight { get; set; }
	[Property] public VRHand LeftHand { get; set; }
	[Property] public VRHand RightHand { get; set; }
	[Property] public EasyIK LeftArmIK { get; set; }
	[Property] public EasyIK RightArmIK { get; set; }
	List<VRHand> hands => new List<VRHand> { LeftHand, RightHand };

	GameObject _positionReference;
	[Property,Hide]
	private GameObject PositionReference
	{ 
		get
		{
			if ( !_positionReference.IsValid() )
			{
				_positionReference = new GameObject();
				_positionReference.Name = "PosRef";
				_positionReference.SetParent( GameObject );
			}
			return _positionReference;
		}
	}

	public struct VRHand
	{
		public VRFinger Thumb { get; set; }
		public VRFinger Index { get; set; }
		public VRFinger Middle { get; set; }
		public VRFinger Ring { get; set; }
		public VRFinger Pinkie { get; set; }
		[Hide] public List<VRFinger> Fingers => new List<VRFinger> { Thumb, Index, Middle, Ring, Pinkie };

		public bool Invert { get; set; }

		[Range( 0, 1 )]
		public float Bend
		{
			get
			{
				return Thumb.Bend;
			}
			set
			{
				foreach ( var finger in Fingers )
				{
					finger.Bend = value;
				}
			}
		}

		[Button]
		public void Procedualise()
		{
			foreach ( var finger in Fingers )
			{
				finger.Procedualise();
			}
		}

		[Button]
		public void UnProcedualise()
		{
			foreach ( var finger in Fingers )
			{
				finger.UnProcedualise();
			}
		}

		[Button]
		public void SetAllOpenRots()
		{
			foreach ( var finger in Fingers )
			{
				finger.SetOpenRots();
			}
		}

		[Button]
		public void SetAllClosedRots()
		{
			foreach ( var finger in Fingers )
			{
				finger.SetClosedRots();
			}
		}

		public void CopyHand(VRHand hand, Rotation copyMod, bool copyOpen, bool copyClosed)
		{
			for(int i = 0; i < Math.Min(Fingers.Count,hand.Fingers.Count); i++ )
			{
				Fingers[i].CopyFinger( hand.Fingers[i], copyMod, copyOpen, copyClosed );
			}
		}

		public void BendFingers()
		{
			foreach ( var finger in Fingers )
			{
				finger.Invert = Invert;
				finger.BendFinger();
			}
		}

		public class VRFinger
		{
			[KeyProperty] public List<FingerBone> Bones { get; set; }

			[Range( 0, 1 )]
			[KeyProperty] public float Bend { get; set; }
			public bool Invert { get; set; }

			public class FingerBone
			{
				[KeyProperty] public GameObject Bone { get; set; }
				public Rotation OpenRotation { get; set; }
				public Rotation ClosedRotation { get; set; }
			}

			[Button]
			public void SetOpenRots()
			{
				for ( int i = 0; i < Bones.Count; i++ )
				{
					Bones[i].OpenRotation = Bones[i].Bone.LocalRotation;
				}
			}

			[Button]
			public void SetClosedRots()
			{
				for ( int i = 0; i < Bones.Count; i++ )
				{
					Bones[i].ClosedRotation = Bones[i].Bone.LocalRotation;
				}
			}

			[Button]
			public void UnProcedualise()
			{
				foreach ( var bone in Bones )
				{
					bone.Bone.Flags = GameObjectFlags.Bone;
				}
			}

			[Button]
			public void Procedualise()
			{
				foreach ( var bone in Bones )
				{
					bone.Bone.Flags = GameObjectFlags.Bone | GameObjectFlags.ProceduralBone;
				}
			}

			public void BendFinger()
			{
				foreach ( var bone in Bones )
				{
					if ( !bone.Bone.IsValid() )
						return;

					bone.Bone.LocalRotation = Rotation.Lerp( bone.OpenRotation, bone.ClosedRotation, Bend );
				}
			}

			public void CopyFinger( VRFinger finger, Rotation Modifications, bool copyOpen, bool copyClosed)
			{
				for ( int i = 0; i < Math.Min( finger.Bones.Count, Bones.Count ); i++ )
				{
					if(copyClosed)
						Bones[i].ClosedRotation = ScaleAngles( finger.Bones[i].ClosedRotation, Modifications );

					if ( copyOpen )
						Bones[i].OpenRotation = ScaleAngles( finger.Bones[i].OpenRotation, Modifications );
				}
			}
		}
	}

	protected override void OnAwake()
	{
		Camera.Enabled = !IsProxy;
	}

	public static Angles ScaleAngles(Angles target, Angles by)
	{
		return new Angles( target.pitch * by.pitch, target.yaw * by.yaw, target.roll * by.roll );
	}

	[Button]
	public void CopyToLeft()
	{
		LeftHand.CopyHand( RightHand, CopyMod, CopyOpen, CopyClosed );
	}

	[Button]
	public void CopyToRight()
	{
		RightHand.CopyHand( LeftHand, CopyMod, CopyOpen, CopyClosed );
	}

	[Property] public Rotation CopyMod { get; set; }
	[Property] public bool CopyOpen { get; set; } = true;
	[Property] public bool CopyClosed { get; set; } = true;

	[Property] public GameObject LeftHint { get; set; }
	[Property] public GameObject LeftHintAnchor { get; set; }
	[Property] public GameObject LeftHintInfluence { get; set; }
	[Property] public GameObject RightHint { get; set; }
	[Property] public GameObject RightHintAnchor { get; set; }
	[Property] public GameObject RightHintInfluence { get; set; }
	[Property] public float InfluenceVelocity { get; set; } = 120;
	[Property] public float InfluenceSmoothness { get; set; } = 10;

	[Property] public GameObject LeftTwist { get; set; }
	[Property] public GameObject RightTwist { get; set; }
	[Property] public float TwistWeight { get; set; } = 0.75f;

	[Property] public GameObject LeftShoulder { get; set; }
	[Property] public GameObject LeftArmOrigin { get; set; }
	[Property] public Rotation LeftShoulderOffset { get; set; } = new Angles( 1, 1, 90 );
	[Property, InlineEditor] public Rotation LeftShoulderDefaultRotation { get; set; } = new();
	[Property] public GameObject RightShoulder { get; set; }
	[Property] public GameObject RightArmOrigin { get; set; }
	[Property] public Rotation RightShoulderOffset { get; set; } = new Angles( 1, 1, 1 );
	[Property] public float MaxHandShoulderInfluence = 0.8f;
	[Property, InlineEditor] public Rotation RightShoulderDefaultRotation { get; set; } = new();


	[Button]
	public void MatchHeight()
	{
		if ( updatingArms )
			return;

		Height = Target.WorldTransform.PointToLocal( Head.WorldPosition ).z / HeadHeights.y;

		UpdateArms();
	}

	/// <summary>
	/// Where are the eyes of our character?
	/// </summary>
	[Property] public GameObject EyeSource { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public Vector2 HeadHeights { get; set; } = new Vector2( 27f, 60f );
	/// <summary>
	/// How tall are we?
	/// </summary>
	/// 
	[Property] public float Height = 1;

	bool updatingArms = false;

	[Button]
	async void UpdateArms()
	{
		Log.Info( updatingArms );
		updatingArms = true;
		for ( int i = 0; i < 2; i++ )
		{
			var ik = i == 0 ? LeftArmIK : RightArmIK;
			var hand = i == 0 ? TargetHandLeft : TargetHandRight;

			Log.Info( ik );
			foreach ( var bone in ik.jointTransforms )
			{
				bone.Flags = GameObjectFlags.Bone;
			}

			hand.Flags = GameObjectFlags.Bone;

			await Task.Frame();
			await Task.Frame();

			ik.jointTransforms.Last().LocalPosition = hand.LocalPosition;

			ik.Awake();

			hand.Flags = GameObjectFlags.Bone | GameObjectFlags.ProceduralBone;

			foreach ( var bone in ik.jointTransforms )
			{
				bone.Flags = GameObjectFlags.Bone | GameObjectFlags.ProceduralBone;
			}

			Log.Info( i );
		}
		updatingArms = false;
	}

	[Property, Group( "Inverse kinematics" ), Title( "Left Foot" )] public GameObject IkLeftFoot { get; set; }
	[Property, Group( "Inverse kinematics" ), Title( "Right Foot" )] public GameObject IkRightFoot { get; set; }

	float leftHandVecocity => (LeftArmIK.ikTarget.WorldPosition - lastLeftHandPos).Length / Time.Delta;

	float rightHandVecocity => (RightArmIK.ikTarget.WorldPosition - lastRightHandPos).Length / Time.Delta;

	Vector3 lastLeftHandPos;
	Vector3 lastRightHandPos;

	protected override void OnUpdate()
	{
		if ( !Target.IsValid() )
			return;

		Target.Set( "scale_height", Height );

		if ( IkLeftFoot.IsValid() && IkLeftFoot.Active ) Target.SetIk( "foot_left", IkLeftFoot.WorldTransform );
		else Target.ClearIk( "foot_left" );

		if ( IkRightFoot.IsValid() && IkRightFoot.Active ) Target.SetIk( "foot_right", IkRightFoot.WorldTransform );
		else Target.ClearIk( "foot_right" );

		MatchToHead();

		MoveAnimations();

		Fingers();

		AnimateFingers();

		MoveShoulders();

		Twist();

		AdjustHints();

		LeftArmIK.SolveIK();

		RightArmIK.SolveIK();

		SnapHands();

		lastLeftHandPos = LeftArmIK.ikTarget.WorldPosition;

		lastRightHandPos = RightArmIK.ikTarget.WorldPosition;
	}

	float leftHintLerp;
	float rightHintLerp;
	private void AdjustHints()
	{
		for ( int i = 0; i < 2; i++ )
		{
			var hint = i == 0 ? LeftHint : RightHint;
			var hintAnchor = i == 0 ? LeftHintAnchor : RightHintAnchor;
			var hintInfluence = i == 0 ? LeftHintInfluence : RightHintInfluence;
			var velocity = i == 0 ? leftHandVecocity : rightHandVecocity;
			
			var x = MathX.Clamp( Normalize( velocity, 0, InfluenceVelocity ), 0, 1);

			if( i == 0 )
			{
				leftHintLerp = MathX.Lerp( leftHintLerp, x, InfluenceSmoothness * Time.Delta);
			}
			else
			{
				rightHintLerp = MathX.Lerp( rightHintLerp, x, InfluenceSmoothness * Time.Delta);
			}

			hint.WorldPosition = Vector3.Lerp(hintAnchor.WorldPosition,hintInfluence.WorldPosition,i == 0 ? leftHintLerp : rightHintLerp);
		}
	}

	private void Twist()
	{
		LeftTwist.LocalRotation = Angles.Zero.WithRoll( TargetHandLeft.LocalRotation.Roll() * TwistWeight );

		RightTwist.LocalRotation = Angles.Zero.WithRoll( TargetHandRight.LocalRotation.Roll() * TwistWeight );
	}

	private void MoveShoulders()
	{
		for(int i = 0; i < 2; i++ )
		{
			var shoulder = i == 0 ? LeftShoulder : RightShoulder;
			var shoulderRotation = i == 0 ? LeftShoulderDefaultRotation : RightShoulderDefaultRotation;
			var shoulderOffset = i == 0 ? LeftShoulderOffset : RightShoulderOffset;
			var armOrigin = i == 0 ? LeftArmOrigin : RightArmOrigin;
			var ik = i == 0 ? LeftArmIK : RightArmIK;

			var direction = ik.ikTarget.WorldPosition - shoulder.WorldPosition;

			var lerp = MathF.Pow( MathX.Clamp( Normalize( Vector3.DistanceBetween( armOrigin.WorldPosition, ik.ikTarget.WorldPosition ), 0, ik.jointChainLength ), 0 , MaxHandShoulderInfluence), 3 );

			shoulder.WorldRotation = Rotation.Lerp( shoulder.Parent.WorldTransform.RotationToWorld( shoulderRotation ), Rotation.LookAt( direction ) * shoulderOffset, lerp );
		}
	}

	private void AnimateFingers()
	{
		if ( IsProxy )
			return;

		if ( Input.VR == null )
			return;

		var hands = new List<VRHand> { LeftHand, RightHand };
		foreach ( var hand in hands )
		{
			var input = hand.Equals( LeftHand ) ? Input.VR.LeftHand : Input.VR.RightHand;

			BroadcastFingers( hand.Equals( LeftHand ), input.GetFingerCurl( 0 ), input.GetFingerCurl( 1 ), input.GetFingerCurl( 2 ), input.GetFingerCurl( 3 ), input.GetFingerCurl( 4 ) );
		}
	}

	[Rpc.Broadcast]
	private void BroadcastFingers(bool left, float thumb, float index, float middle, float ring, float pinkie)
	{
		var hand = left ? LeftHand : RightHand;
		hand.Thumb.Bend = Normalize( thumb, 0.932f, 2.5f );

		hand.Index.Bend = Normalize( index, 0, 0.926f );

		hand.Middle.Bend = Normalize( middle, -0.01f, 1 );

		hand.Ring.Bend = Normalize( ring, 0.113f, 0.966f );

		hand.Pinkie.Bend = Normalize( pinkie, -0.079f, 2.287f );
	}

	public static float Normalize( float value, float min, float max )
	{
		if ( max == min ) return 0f;
		return (value - min) / (max - min);
	}

	private void Fingers()
	{
		var hands = new List<VRHand> { LeftHand, RightHand };
		foreach ( var hand in hands )
		{
			hand.BendFingers();
		}
	}

	private void SnapHands()
	{
		TargetHandLeft.WorldTransform = LeftArmIK.ikTarget.WorldTransform;
		TargetHandRight.WorldTransform = RightArmIK.ikTarget.WorldTransform;
	}
	float lastYaw;

	Vector3 dampedPos;
	private void MatchToHead()
	{
		dampedPos = Vector3.Lerp( dampedPos, Head.WorldPosition, Time.Delta * 10 );
		var pos = dampedPos;

		//Match Duck
		float HeadHeight = Target.WorldTransform.PointToLocal( dampedPos ).z;
		Vector2 AHeadHeights = HeadHeights * Height;
		float HeadFraction = (HeadHeight - AHeadHeights.y) / (AHeadHeights.x - AHeadHeights.y);

		DuckLevel = HeadFraction * 2;

		float targetYaw = Target.WorldRotation.Yaw();
		float headYaw = Head.WorldRotation.Yaw();

		// Calculate the shortest angle difference
		float yawDifference = NormalizeAngle( headYaw - targetYaw );

		if ( MathF.Abs( yawDifference ) > 15 && Vector3.GetAngle( Head.WorldTransform.Forward, Vector3.Down ) > 15 )
		{
			// Smoothly interpolate the yaw to match the head's yaw
			float newYaw = NormalizeAngle( targetYaw + MathX.Lerp( 0, yawDifference, 10 * Time.Delta ) );
			Target.WorldRotation = new Angles( 0, newYaw, 0 );
		}

		lastYaw = Head.WorldRotation.Yaw();

		PositionReference.WorldPosition = Target.WorldPosition;

		//Match Position	
		Vector3 childOffset = PositionReference.WorldTransform.PointToLocal( TargetHead.WorldPosition );

		Target.LocalPosition = GameObject.WorldTransform.PointToLocal(pos) - childOffset;

		Target.LocalPosition = Target.LocalPosition.WithZ( 0 );

		TargetHead.WorldRotation = Head.WorldRotation * new Angles(-90,-90,0);
	}

	public static float NormalizeAngle( float angle )
	{
		while ( angle > 180 ) angle -= 360;
		while ( angle < -180 ) angle += 360;
		return angle;
	}

	Vector3 lastHeadPos;
	private void MoveAnimations()
	{
		Vector3 headPos = Head.WorldPosition;

		Vector3 moveDirection = (headPos - lastHeadPos) / Time.Delta;

		WithVelocity( moveDirection );

		lastHeadPos = headPos;
	}

	public void ProceduralHitReaction( DamageInfo info, float damageScale = 1.0f, Vector3 force = default )
	{
		var boneId = info.Hitbox?.Bone?.Index ?? 0;
		var bone = Target.GetBoneObject( boneId );

		var localToBone = bone.LocalPosition;
		if ( localToBone == Vector3.Zero ) localToBone = Vector3.One;

		Target.Set( "hit", true );
		Target.Set( "hit_bone", boneId );
		Target.Set( "hit_offset", localToBone );
		Target.Set( "hit_direction", force.Normal );
		Target.Set( "hit_strength", (force.Length / 1000.0f) * damageScale );
	}

	/// <summary>
	/// The transform of the eyes, in world space. This is worked out from EyeSource is it's set.
	/// </summary>
	public Transform EyeWorldTransform
	{
		get
		{
			if ( EyeSource.IsValid() ) return EyeSource.WorldTransform;

			return WorldTransform;
		}
	}


	/// <summary>
	/// Have the player look at this point in the world
	/// </summary>
	public void WithLook( Vector3 lookDirection, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
	{
		Target.SetLookDirection( "aim_eyes", lookDirection, eyesWeight );
		Target.SetLookDirection( "aim_head", lookDirection, headWeight );
		Target.SetLookDirection( "aim_body", lookDirection, bodyWeight );
	}

	/// <summary>
	/// Have the player animate moving with a set velocity (this doesn't move them! Your character controller is responsible for that)
	/// </summary>
	/// <param name="Velocity"></param>
	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Target.WorldRotation.Forward.Dot( dir );
		var sideward = Target.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Target.Set( "move_direction", angle );
		Target.Set( "move_speed", Velocity.Length );
		Target.Set( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		Target.Set( "move_y", sideward );
		Target.Set( "move_x", forward );
		Target.Set( "move_z", Velocity.z );
	}

	/// <summary>
	/// Animates the wish for the character to move in a certain direction. For example, when in the air, your character will swing their arms in that direction.
	/// </summary>
	/// <param name="Velocity"></param>
	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Target.WorldRotation.Forward.Dot( dir );
		var sideward = Target.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Target.Set( "wish_direction", angle );
		Target.Set( "wish_speed", Velocity.Length );
		Target.Set( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		Target.Set( "wish_y", sideward );
		Target.Set( "wish_x", forward );
		Target.Set( "wish_z", Velocity.z );
	}

	/// <summary>
	/// Where are we aiming?
	/// </summary>
	public Rotation AimAngle
	{
		set
		{
			value = Target.WorldRotation.Inverse * value;
			var ang = value.Angles();

			Target.Set( "aim_body_pitch", ang.pitch );
			Target.Set( "aim_body_yaw", ang.yaw );
		}
	}

	/// <summary>
	/// The weight of the aim angle, but specifically for the Citizen's eyes.
	/// </summary>
	public float AimEyesWeight
	{
		get => Target.GetFloat( "aim_eyes_weight" );
		set => Target.Set( "aim_eyes_weight", value );
	}

	/// <summary>
	/// The weight of the aim angle, but specifically for the Citizen's head.
	/// </summary>
	public float AimHeadWeight
	{
		get => Target.GetFloat( "aim_head_weight" );
		set => Target.Set( "aim_head_weight", value );
	}


	/// <summary>
	/// The weight of the aim angle, but specifically for the Citizen's body.
	/// </summary>
	public float AimBodyWeight
	{
		get => Target.GetFloat( "aim_body_weight" );
		set => Target.Set( "aim_body_weight", value );
	}

	/// <summary>
	/// How much the character is rotating in degrees per second, this controls feet shuffling.
	/// If rotating clockwise this should be positive, if rotating counter-clockwise this should be negative.
	/// </summary>
	public float MoveRotationSpeed
	{
		get => Target.GetFloat( "move_rotationspeed" );
		set => Target.Set( "move_rotationspeed", value );
	}

	[Obsolete( "Use MoveRotationSpeed" )]
	public float FootShuffle
	{
		get => Target.GetFloat( "move_shuffle" );
		set => Target.Set( "move_shuffle", value );
	}

	/// <summary>
	/// The scale of being ducked (crouched) (0 - 1)
	/// </summary>
	/// 

	public float DuckLevel
	{
		get => Target.GetFloat( "duck" );
		set => Target.Set( "duck", value );
	}

	/// <summary>
	/// How loud are we talking?
	/// </summary>
	public float VoiceLevel
	{
		get => Target.GetFloat( "voice" );
		set => Target.Set( "voice", value );
	}

	/// <summary>
	/// Are we sitting down?
	/// </summary>
	public bool IsSitting
	{
		get => Target.GetBool( "b_sit" );
		set => Target.Set( "b_sit", value );
	}

	/// <summary>
	/// Are we on the ground?
	/// </summary>
	public bool IsGrounded
	{
		get => Target.GetBool( "b_grounded" );
		set => Target.Set( "b_grounded", value );
	}

	/// <summary>
	/// Are we swimming?
	/// </summary>
	public bool IsSwimming
	{
		get => Target.GetBool( "b_swim" );
		set => Target.Set( "b_swim", value );
	}

	/// <summary>
	/// Are we climbing?
	/// </summary>
	public bool IsClimbing
	{
		get => Target.GetBool( "b_climbing" );
		set => Target.Set( "b_climbing", value );
	}

	/// <summary>
	/// Are we noclipping?
	/// </summary>
	public bool IsNoclipping
	{
		get => Target.GetBool( "b_noclip" );
		set => Target.Set( "b_noclip", value );
	}

	/// <summary>
	/// Is the weapon lowered? By default, this'll happen when the character hasn't been shooting for a while.
	/// </summary>
	public bool IsWeaponLowered
	{
		get => Target.GetBool( "b_weapon_lower" );
		set => Target.Set( "b_weapon_lower", value );
	}

	public enum HoldTypes
	{
		None,
		Pistol,
		Rifle,
		Shotgun,
		HoldItem,
		Punch,
		Swing,
		RPG
	}

	/// <summary>
	/// What kind of weapon are we holding?
	/// </summary>
	public HoldTypes HoldType
	{
		get => (HoldTypes)Target.GetInt( "holdtype" );
		set => Target.Set( "holdtype", (int)value );
	}

	public enum Hand
	{
		Both,
		Right,
		Left
	}

	/// <summary>
	/// What's the handedness of our weapon? Left handed, right handed, or both hands? This is only supported by some holdtypes, like Pistol, HoldItem.
	/// </summary>
	public Hand Handedness
	{
		get => (Hand)Target.GetInt( "holdtype_handedness" );
		set => Target.Set( "holdtype_handedness", (int)value );
	}

	/// <summary>
	/// Triggers a jump animation
	/// </summary>
	public void TriggerJump()
	{
		Target.Set( "b_jump", true );
	}

	/// <summary>
	/// Triggers a weapon deploy animation
	/// </summary>
	public void TriggerDeploy()
	{
		Target.Set( "b_deploy", true );
	}

	public enum MoveStyles
	{
		Auto,
		Walk,
		Run
	}

	/// <summary>
	/// We can force the model to walk or run, or let it decide based on the speed.
	/// </summary>
	public MoveStyles MoveStyle
	{
		get => (MoveStyles)Target.GetInt( "move_style" );
		set => Target.Set( "move_style", (int)value );
	}

	public enum SpecialMoveStyle
	{
		None,
		LedgeGrab,
		Roll,
		Slide
	}

	/// <summary>
	/// We can force the model to have a specific movement state, instead of just running around.
	/// <see cref="SpecialMoveStyle.LedgeGrab"/> is good for shimmying across a ledge.
	/// <see cref="SpecialMoveStyle.Roll"/> is good for a platformer game where the character is rolling around continuously.
	/// <see cref="SpecialMoveStyle.Slide"/> is good for a shooter game or a platformer where the character is sliding.
	/// </summary>
	public SpecialMoveStyle SpecialMove
	{
		get => (SpecialMoveStyle)Target.GetInt( "special_movement_states" );
		set => Target.Set( "special_movement_states", (int)value );
	}

	//Sitting
	public enum SittingStyle
	{
		None,
		Chair,
		Floor
	}

	/// <summary>
	/// How are we sitting down?
	/// </summary>
	public SittingStyle Sitting
	{
		get => (SittingStyle)Target.GetInt( "sit" );
		set => Target.Set( "sit", (int)value );
	}

	/// <summary>
	/// How far up are we sitting down from the floor?
	/// </summary>
	public float SittingOffsetHeight
	{
		get => Target.GetFloat( "sit_offset_height" );
		set => Target.Set( "sit_offset_height", value );
	}

	/// <summary>
	/// From 0-1, how much are we actually sitting down.
	/// </summary>
	public float SittingPose
	{
		get => Target.GetFloat( "sit_pose" );
		set => Target.Set( "sit_pose", value );
	}
}
