using Sandbox;

public sealed class ProcedualBoneAnimation : Component
{
	SkinnedModelRenderer SkinnedModelRenderer;
	List<(GameObject bone, bool notAnimated)> BoneObjects;
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
			BoneObjects.Add( (boneObject, boneObject.Flags.HasFlag(GameObjectFlags.ProceduralBone)) );

			boneObject.Flags = GameObjectFlags.ProceduralBone;
		}
	}

	protected override void OnUpdate()
	{
		for (int i = 0; i < boneCount; i++ )
		{
			SkinnedModelRenderer.TryGetBoneTransformAnimation( SkinnedModelRenderer.Model.Bones.GetBone(SkinnedModelRenderer.Model.GetBoneName(i)), out var transform );

			if ( BoneObjects[i].notAnimated )
				continue;
			BoneObjects[i].bone.WorldTransform = transform;
		}
	}

}
