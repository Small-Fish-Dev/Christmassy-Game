using Sandbox;
using System;
using System.Text.Json.Serialization;

public sealed class RoadObject : Component
{
	public GameObject CreatedObject;
	[JsonInclude] public Random RandomSeed;
	public bool IsVisible = true;

	public void CreateObject( PrefabFile sceneFile )
	{
		CreatedObject?.Destroy();

		CreatedObject = SceneUtility.GetPrefabScene( sceneFile )
				.Clone();
		CreatedObject.SetParent( GameObject );
		CreatedObject.WorldTransform = WorldTransform;
	}

	TimeUntil _nextCheck;

	protected override void OnFixedUpdate()
	{
		if ( RandomSeed == null ) return;

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
					CreateObject( RandomSeed.FromList( ChristmassyGameLogic.Instance.AllRoadObjects ) );
				}
			}

			_nextCheck = 0.5f + RandomSeed.Float( 0.5f );
		}
	}

	protected override void OnDestroy()
	{
		CreatedObject?.Destroy();
	}
}
