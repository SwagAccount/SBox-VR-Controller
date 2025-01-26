using Sandbox;
using Sandbox.Citizen;
using Sandbox.Physics;
using Sandbox.VR;
using System;

public sealed class VrhandInteraction : Component
{
	[Property] private HandEnum Hand { get; set; }
	public enum HandEnum
	{
		Left,
		Right
	}

	[Property] private VRAnimationHelper VRAnimationHelper { get; set; }
	[Property] private GameObject Reference { get; set; }
	[Property] private HandState CurrentHandState { get; set; }
	[Property] private float SearchRadius { get; set; } = 5f;
	[Property] private float SearchDistance { get; set; } = 200f;
	[Property] private float StrengthModifier { get; set; } = 1f;

	public enum HandState
	{
		None,
		Searching,
		Holding
	}

	private VRController VRController => Hand.Equals( HandEnum.Left ) ? Input.VR.LeftHand : Input.VR.RightHand;
	private VRAnimationHelper.VRHand AnimatedHand => Hand.Equals( HandEnum.Left ) ? VRAnimationHelper.LeftHand : VRAnimationHelper.RightHand;

	private Rigidbody Body { get; set; }
	private Rigidbody JointPoint { get; set; }
	private Sandbox.Physics.FixedJoint FixedJoint { get; set; }
	private Sandbox.Physics.FixedJoint ItemJoint { get; set; }

	protected override void OnStart()
	{
		Body = GetComponent<Rigidbody>();

		JointPoint = new GameObject().AddComponent<Rigidbody>();

		JointPoint.GameObject.SetParent( GameObject.Parent );

		JointPoint.MotionEnabled = false;

		var p1 = new PhysicsPoint( JointPoint.PhysicsBody, Vector3.Zero );
		var p2 = new PhysicsPoint( Body.PhysicsBody, Vector3.Zero );

		FixedJoint = PhysicsJoint.CreateFixed( p1, p2 );
		FixedJoint.SpringLinear = new PhysicsSpring( 150, 5 );
		FixedJoint.SpringAngular = new PhysicsSpring( 150, 5 );

		CurrentHandState = HandState.Searching;
	}

	HandState previousHandState;
	protected override void OnUpdate()
	{
		PositionHand();

		AnimatedHand.NoControl = !CurrentHandState.Equals( HandState.Searching );

		Body.MotionEnabled = CurrentHandState.Equals( HandState.Searching );

		switch (CurrentHandState)
		{
			case HandState.Searching:
				Searching();
				break;
			
			case HandState.Holding:
				Holding();
				break;
		}

		previousHandState = CurrentHandState;

		if ( ItemJoint.IsValid() )
		{
			ItemJoint.SpringLinear = new PhysicsSpring( 150 * StrengthModifier, 5 );
			ItemJoint.SpringAngular = new PhysicsSpring( 150 * StrengthModifier, 5 );
		}
	}

	void Searching()
	{
		if ( previousHandState != HandState.Searching )
		{
			Tags.Remove( "activehand" );
		}

		List<GrabPoint> GrabbablePoints = new List<GrabPoint>();
		List<Interactable> InteractablePoints = new List<Interactable>();

		Search( ref GrabbablePoints, ref InteractablePoints );

		var closestDistance = 10000f;
		var closestPoint = GrabbablePoints.Count > 0 ? GrabbablePoints[0] : null;

		foreach ( var gPoint in GrabbablePoints )
		{
			var distance = Vector3.DistanceBetween( WorldPosition, gPoint.WorldPosition );

			if ( distance > closestDistance )
				continue;

			closestDistance = distance;

			closestPoint = gPoint;
		}
		
		if (closestPoint.IsValid())
			GrabPointSelection(closestPoint);
	}

	void GrabPointSelection(GrabPoint closestPoint)
	{
		Gizmo.Draw.SolidSphere( closestPoint.WorldPosition, 0.5f );

		if ( VRController.Grip > 0.5f )
			Grab( closestPoint );
	}


	[Property] GrabPoint HeldPoint { get; set; }
	void Holding()
	{
		if(VRController.Grip < 0.2f)
		{
			Drop();
			return;
		}

		if(HeldPoint.IsValid())
		{
			var HeldPointSkeleton = Hand.Equals( HandEnum.Left ) ? HeldPoint.LeftHand : HeldPoint.RightHand;

			AnimatedHand.Root.WorldPosition = HeldPointSkeleton.WorldPosition;
			AnimatedHand.Root.WorldRotation = HeldPointSkeleton.WorldRotation;

			CopyTransformRecursive( HeldPointSkeleton, AnimatedHand.Root, Vector3.One, new Angles(1,1,1) );

			WorldPosition = HeldPoint.WorldPosition;
			WorldRotation = HeldPoint.WorldRotation;
		}
	}

	public void Grab( GrabPoint point )
	{
		Tags.Add( "activehand" );
		CurrentHandState = HandState.Holding;
		HeldPoint = point;

		HeldPoint.Body.GameObject.SetParent( GameObject.Parent );

		var p1 = new PhysicsPoint( point.Body.PhysicsBody, point.Body.WorldTransform.PointToLocal( point.WorldPosition ), point.Body.WorldTransform.RotationToLocal( point.WorldRotation) );
		var p2 = new PhysicsPoint( JointPoint.PhysicsBody, Vector3.Zero );

		ItemJoint = PhysicsJoint.CreateFixed( p1, p2 );
		ItemJoint.SpringLinear = new PhysicsSpring( 150, 5 );
		ItemJoint.SpringAngular = new PhysicsSpring( 150, 5 );

		HeldPoint.Held = true;
		HeldPoint.Hand = Hand;
	}

	public void Drop()
	{
		CurrentHandState = HandState.Searching;
		HeldPoint.Held = false;
		HeldPoint?.Body.GameObject.SetParent( null );
		HeldPoint = null;
		ItemJoint?.Remove();
		ItemJoint = null;
		
	}

	void Search(ref List<GrabPoint> GrabbablePoints, ref List<Interactable> InteractablePoints)
	{
		for ( int i = 0; i < 2; i++ )
		{
			Vector3 searchPos = WorldPosition;
			if ( i > 0 )
			{
				var ray = Scene.Trace.Ray( AnimatedHand.Root.WorldPosition, AnimatedHand.Root.WorldPosition + AnimatedHand.Root.WorldTransform.Forward * SearchDistance ).Radius( SearchRadius ).Run();
				if ( ray.Hit ) searchPos = ray.HitPosition;
			}
			IEnumerable<GameObject> gameObjects = Scene.FindInPhysics( new Sphere( searchPos, SearchRadius ) );
			GrabbablePoints = new List<GrabPoint>();
			InteractablePoints = new List<Interactable>();

			foreach ( GameObject g in gameObjects )
			{
				if ( i > 0 && g.Tags.Contains( "closepickup" ) ) continue;

				if ( g.Tags.Contains( "interactable" ) )
				{
					var interactablePoint = g.GetComponent<Interactable>();
					if ( !interactablePoint.IsValid )
						break;
					i = 10;
					InteractablePoints.Add( interactablePoint );
				}
				if ( g.Tags.Contains( "grabpoint" ) )
				{
					var grabPoint = g.GetComponent<GrabPoint>();
					if ( !grabPoint.IsValid )
						break;
					if ( grabPoint.Held )
						break;
					i = 10;
					GrabbablePoints.Add( grabPoint ); ;
				}
			}
		}
	}

	void PositionHand()
	{
		JointPoint.WorldRotation = Reference.WorldRotation;

		var direction = Reference.WorldPosition - JointPoint.WorldPosition;
		var trace = Scene.Trace.Ray( JointPoint.WorldPosition, Reference.WorldPosition + direction.Normal * 2 ).IgnoreGameObjectHierarchy( GameObject.Parent ).Run();

		JointPoint.WorldPosition = trace.Hit ?
				trace.HitPosition - direction.Normal * 2 :
				Reference.WorldPosition;
	}

	public static void CopyTransformRecursive( GameObject target, GameObject set, Vector3 posMod, Angles angMod )
	{

		for ( int i = 0; i < target.Children.Count; i++ )
		{
			if ( i >= set.Children.Count )
				continue;
			GameObject targetChild = target.Children[i];
			GameObject setChild = set.Children[i];

			setChild.LocalPosition = targetChild.LocalPosition * posMod;
			Vector3 modifiedAngles = targetChild.LocalRotation.Angles().AsVector3() * angMod.AsVector3();
			setChild.LocalRotation = new Angles( modifiedAngles.x, modifiedAngles.y, modifiedAngles.z );
			CopyTransformRecursive( targetChild, setChild, posMod, angMod );
		}
	}

}
