using Sandbox;
using System;
using System.Text.Json.Serialization;

public sealed class RoadObject : Component
{
	public GameObject CreatedObject;
	public bool IsVisible = true;

	protected override void OnStart()
	{
		CreateObject();
	}

	public void CreateObject()
	{
		CreatedObject?.Destroy();

		var randomObject = Game.Random.FromList( ChristmassyGameLogic.Instance.AllRoadObjects );
		CreatedObject = SceneUtility.GetPrefabScene( randomObject )
				.Clone();
		CreatedObject.SetParent( GameObject );
		CreatedObject.WorldTransform = WorldTransform;
	}

	TimeUntil _nextCheck;

	protected override void OnFixedUpdate()
	{
		if ( _nextCheck )
		{
			if ( Vector3.Dot( WorldRotation.Up, Vector3.Down ) > 0f ) // We're upside down!
			{
				if ( IsVisible )
				{
					IsVisible = false;
					CreatedObject?.Destroy();
				}
			}

			if ( Vector3.Dot( WorldRotation.Up, Vector3.Down ) <= 0f )
			{
				if ( !IsVisible )
				{
					IsVisible = true;
					CreateObject();
				}
			}

			_nextCheck = 1f + Game.Random.Float( 0.5f );
		}
	}

	protected override void OnDestroy()
	{
		CreatedObject?.Destroy();
	}
}
