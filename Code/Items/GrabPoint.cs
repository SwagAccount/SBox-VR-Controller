using Sandbox;

public sealed class GrabPoint : Component
{
	[Property] public bool Main { get; set; } = true;
	[Property] public GrabPoint SecondaryPoint { get; set; }
	[Property] public GrabPoint MainPoint { get; set; }
	[Property] public bool RifleHold { get; set; }
	[Property] public bool Held { get; set; }
	[Property] public VrhandInteraction.HandEnum Hand { get; set; }
	[Property] public Rigidbody Body { get; set; }
	[Property] public float StrengthMult { get; set; } = 1f;

	[Property] public GameObject LeftHand { get; set; }
	[Property] public GameObject RightHand { get; set; }
	[Property] public VrhandInteraction GrabbedHand { get; set; }

	[Property] public bool Left { get; set; } = true;
	[Property] public bool Right { get; set; } = true;

	[Property]
	public List<GrabPoint> Friends { get; set; }

	public bool DoUnparent()
	{
		for (int i = 0; i < Friends.Count; i++)
		{
			if ( Friends[i].Held )
				return false;
		}

		return !Held;
	}

	GameObject _relativePoint = null;
	[Property] public GameObject RelativePoint
	{
		get 
		{ 
			return _relativePoint; 
		} 
		set 
		{ 
			_relativePoint = value;
			UpdatePoint();
		}
	}

	Vector3 _pointOffset = Vector3.Zero;
	[Property]
	public Vector3 PointOffset
	{
		get 
		{ 
			return _pointOffset;
		}
		set 
		{ 
			_pointOffset = value;
			UpdatePoint();
		}
	}

	public GameObject RelativeObject => RelativePoint.IsValid() ? RelativePoint : GameObject;

	public Vector3 VisualPoint => RelativeObject.WorldTransform.PointToWorld( PointOffset );

	[Button]
	public void UpdatePoint()
	{
		var collider = GetComponent<SphereCollider>();

		collider.Center = GameObject.WorldTransform.PointToLocal( RelativeObject.WorldTransform.PointToWorld( PointOffset ) );
	}



	[Button]
	void CopyRightToLeft()
	{
		VrhandInteraction.CopyTransformRecursive( RightHand, LeftHand, new Vector3(1,1,-1), new Angles( -1, 1, -1 ) );
	}

	[Button]
	void CopyLeftToRight()
	{
		VrhandInteraction.CopyTransformRecursive( LeftHand, RightHand, new Vector3( 1, 1, -1 ), new Angles( -1, 1, -1 ) );
	}

	protected override void OnStart()
	{

	}

	protected override void DrawGizmos()
	{
		if ( !Gizmo.IsSelected )
			return;

		Gizmo.Draw.IgnoreDepth = true;
		Gizmo.Draw.SolidSphere( GameObject.WorldTransform.PointToLocal( RelativeObject.WorldTransform.PointToWorld( PointOffset ) ), 0.5f );
	}
}
