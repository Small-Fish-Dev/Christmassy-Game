using Sandbox;

public sealed class ChristmassyGameLogic : Component
{
	/// <summary>
	/// The map we're going to rotate
	/// </summary>
	[Property]
	[Category( "Map" )]
	public GameObject Map { get; set; }

	[Property]
	[Category( "Map" )]
	public float MapRadius { get; set; } = 2040f;

	[Property]
	[Category( "Map" )]
	public float RoadWidth { get; set; } = 200f;

	[Property]
	[Category( "Map" )]
	public int CottagesSpawned { get; set; } = 20;

	/// <summary>
	/// How many degrees it rotates per second
	/// </summary>
	[Property]
	[Category( "Gameplay" )]
	[Range( 0f, 360f, 1f )]
	public float RotationSpeed { get; set; } = 15f;

	public GameObject MapClone;

	protected override void OnStart()
	{
		MapClone = Map.Clone();
		MapClone.WorldPosition += Vector3.Forward * 3000f + Vector3.Down * 1500f;
		MapClone.WorldRotation *= Rotation.FromPitch( -90f );
		MapClone.WorldScale *= 1.3f;
	}

	protected override void OnUpdate()
	{
		if ( !Map.IsValid() ) return;

		Map.WorldRotation *= Rotation.FromPitch( -RotationSpeed * Time.Delta );
		MapClone.WorldRotation *= Rotation.FromPitch( -RotationSpeed * Time.Delta );
	}

	private List<GameObject> _cottages = new List<GameObject>();

	[Button( "Generate Map Debug" )]
	public void GenerateMap()
	{
		if ( !Map.IsValid() ) return;

		ClearMap();

		var allCottages = ResourceLibrary.GetAll<PrefabFile>()
			.Where( x => x.GetMetadata( "Tags" )?.Contains( "cottage" ) ?? false )
			.ToList();

		if ( !allCottages.Any() )
		{
			Log.Error( "No cottages found" );
			return;
		}

		var angleSlice = 360f / CottagesSpawned;

		for ( int i = 0; i < CottagesSpawned; i++ )
		{
			var side = i % 2;
			var randomCottage = SceneUtility.GetPrefabScene( Game.Random.FromList( allCottages ) )
				.Clone();

			var randomPitch = Game.Random.Float( -angleSlice / 2f, angleSlice / 2f );
			randomCottage.WorldRotation = Rotation.FromPitch( angleSlice * i + randomPitch );
			var sideRotation = Rotation.FromYaw( -90f + 180f * side );
			randomCottage.WorldRotation *= sideRotation;

			randomCottage.WorldPosition = Map.WorldPosition + randomCottage.WorldRotation.Up * MapRadius;
			var randomDistance = Game.Random.Float( 100f, 300f );
			var sidePosition = (RoadWidth / 2f + randomDistance) * (side == 0 ? Vector3.Left : Vector3.Right);
			randomCottage.WorldPosition += sidePosition;
			randomCottage.SetParent( Map );

			_cottages.Add( randomCottage );
		}
	}

	public void ClearMap()
	{
		if ( _cottages != null )
		{
			foreach ( var cottage in _cottages.ToList() )
				cottage.Destroy();

			_cottages.Clear();
		}
	}

	protected override void DrawGizmos()
	{
		using ( Gizmo.Scope( "MapGizmo", new global::Transform( -WorldPosition, WorldRotation.Inverse ) ) )
		{
			var draw = Gizmo.Draw;

			if ( !Map.IsValid() ) return;

			draw.Color = Color.Blue.WithAlpha( 0.3f );
			draw.SolidCylinder( Map.WorldPosition + Map.WorldRotation.Right * 2500f, Map.WorldPosition + Map.WorldRotation.Left * 2500f, MapRadius, 64 );
		}
	}
}
