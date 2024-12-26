using Sandbox;
using System;
using System.Text.Json.Serialization;

public sealed class RoadObject : Component
{
	public GameObject CreatedObject;
	public GameObject Clone;
	public bool IsVisible = true;

	protected override void OnStart()
	{
		CreateObject();
	}

	public void CreateObject()
	{
		CreatedObject?.Destroy();
		Clone?.Destroy();

		var randomObject = Game.Random.FromList( ChristmassyGameLogic.Instance.AllRoadObjects );
		CreatedObject = SceneUtility.GetPrefabScene( randomObject ).Clone();
		CreatedObject.SetParent( GameObject );
		CreatedObject.WorldTransform = WorldTransform;

		Clone = CreatedObject.Clone();
		Clone.SetParent( ChristmassyGameLogic.Instance.MapClone );
		Clone.LocalTransform = LocalTransform;
		Clone.GetComponent<Collider>()?.Destroy();
	}

	TimeUntil _nextCheck;

	protected override void OnFixedUpdate()
	{
		if ( _nextCheck )
		{
			if ( Vector3.Dot( WorldRotation.Up, Vector3.Backward ) >= 0.5f )
			{
				if ( IsVisible )
				{
					IsVisible = false;
					CreatedObject?.Destroy();
				}
			}

			if ( Vector3.Dot( WorldRotation.Up, Vector3.Down ) <= 1f )
			{
				if ( !IsVisible )
				{
					IsVisible = true;
					CreateObject();
				}
			}

			_nextCheck = 0.5f + Game.Random.Float( 0.5f );
		}
	}

	protected override void OnDestroy()
	{
		CreatedObject?.Destroy();
		Clone?.Destroy();
	}
}
