using Sandbox;

public sealed class ProcedualBoneAnimation : Component
{
	SkinnedModelRenderer SkinnedModelRenderer;
	List<(GameObject boneObject, BoneCollection.Bone bone, bool notAnimated)> BoneObjects;
	int boneCount;
	protected override void OnStart()
	{
		SkinnedModelRenderer = GetComponent<SkinnedModelRenderer>();
		BoneObjects = new();
		boneCount = SkinnedModelRenderer.GetBoneTransforms( true ).Count();
		for (int i = 0; i < boneCount; i++ )
		{
			var boneObject = SkinnedModelRenderer.GetBoneObject( i );
			if ( !boneObject.IsValid() )
				continue;
			BoneObjects.Add( (boneObject, SkinnedModelRenderer.Model.Bones.GetBone( SkinnedModelRenderer.Model.GetBoneName( i ) ),  boneObject.Flags.HasFlag(GameObjectFlags.ProceduralBone)) );

			boneObject.Flags = GameObjectFlags.ProceduralBone;
		}
	}

	protected override void OnPreRender()
	{
		for (int i = 0; i < boneCount; i++ )
		{
			SkinnedModelRenderer.TryGetBoneTransformAnimation( BoneObjects[i].bone , out var transform );

			if ( BoneObjects[i].notAnimated )
				continue;
			BoneObjects[i].boneObject.WorldTransform = transform;
		}
	}

}
