using Sandbox;

public sealed class RoadObject : Component
{
	public GameObject CreatedObject;

	public void CreateObject( PrefabFile sceneFile )
	{
		CreatedObject?.Destroy();

		CreatedObject = SceneUtility.GetPrefabScene( sceneFile )
				.Clone();
		CreatedObject.SetParent( GameObject );
		CreatedObject.WorldTransform = WorldTransform;
	}

	protected override void OnDestroy()
	{
		CreatedObject?.Destroy();
	}
}
