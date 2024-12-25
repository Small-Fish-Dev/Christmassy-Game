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

	[Property]
	[Category( "Map" )]
	public int RoadObjects { get; set; } = 10;

	[Property]
	[Category( "Map" )]
	public Collider SlipperySlope { get; set; }

	/// <summary>
	/// How many degrees it rotates per second
	/// </summary>
	[Property]
	[Category( "Gameplay" )]
	[Range( 0f, 360f, 1f )]
	public float RotationSpeed { get; set; } = 15f;

	public GameObject MapClone;
	public TimeSince StartTimer;
	public int Points = 0;

	public static ChristmassyGameLogic Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;

		StartGame();
	}

	protected override void OnUpdate()
	{
		if ( !Map.IsValid() ) return;

		Map.WorldRotation *= Rotation.FromPitch( -RotationSpeed * Time.Delta );
		MapClone.WorldRotation *= Rotation.FromPitch( -RotationSpeed * Time.Delta );
	}

	private TimeUntil _updateVelocity;

	protected override void OnFixedUpdate()
	{
		RotationSpeed += Time.Delta * 0.3f;

		if ( !SlipperySlope.IsValid() ) return;

		if ( _updateVelocity )
		{
			SlipperySlope.SurfaceVelocity = Vector3.Backward * RotationSpeed * 10000f;

			_updateVelocity = 1f;
		}
	}

	private List<GameObject> _cottages = new List<GameObject>();

	[Button( "Generate Cottages Debug" )]
	public void GenerateCottages()
	{
		if ( !Map.IsValid() ) return;

		ClearCottages();

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
			var randomDistance = Game.Random.Float( 150f, 400f );
			var sidePosition = (RoadWidth / 2f + randomDistance) * (side == 0 ? Vector3.Left : Vector3.Right);
			randomCottage.WorldPosition += sidePosition;
			randomCottage.SetParent( Map );

			_cottages.Add( randomCottage );
		}
	}

	private List<GameObject> _roadObjects = new List<GameObject>();

	[Button( "Generate Road" )]
	public void GenerateRoad()
	{
		if ( !Map.IsValid() ) return;

		ClearRoad();

		var allPresents = ResourceLibrary.GetAll<PrefabFile>()
			.Where( x => x.GetMetadata( "Tags" )?.Contains( "gift" ) ?? false );

		var allObstacles = ResourceLibrary.GetAll<PrefabFile>()
			.Where( x => x.GetMetadata( "Tags" )?.Contains( "obstacle" ) ?? false );

		if ( !allPresents.Any() )
		{
			Log.Error( "No presents found" );
			return;
		}

		if ( !allObstacles.Any() )
		{
			Log.Error( "No obstacle found" );
			return;
		}

		var allObjects = allPresents.Concat( allObstacles ).ToList();

		var angleSlice = 360f / RoadObjects;

		for ( int i = 1; i < RoadObjects; i++ )
		{
			var side = Game.Random.Int( -1, 1 );
			var randomObject = SceneUtility.GetPrefabScene( Game.Random.FromList( allObjects ) )
				.Clone();

			randomObject.WorldRotation = Rotation.FromPitch( angleSlice * i );

			randomObject.WorldPosition = Map.WorldPosition + randomObject.WorldRotation.Up * MapRadius;
			var sidePosition = Vector3.Left * (RoadWidth - 80f) * side;
			randomObject.WorldPosition += sidePosition;
			randomObject.SetParent( Map );

			_roadObjects.Add( randomObject );
		}
	}

	public void GenerateMap()
	{
		GenerateCottages();
		GenerateRoad();

		MapClone = Map.Clone();
		MapClone.WorldPosition += Vector3.Forward * 2500f + Vector3.Down * 000f;
		MapClone.WorldRotation *= Rotation.FromPitch( -90f );
		MapClone.WorldScale *= 0.7f;
	}

	public void ClearMap()
	{
		ClearCottages();
		ClearRoad();
		MapClone?.Destroy();
	}

	public void ClearCottages()
	{
		if ( _cottages != null )
		{
			foreach ( var cottage in _cottages.ToList() )
				cottage.Destroy();

			_cottages.Clear();
		}
	}

	public void ClearRoad()
	{
		if ( _roadObjects != null )
		{
			foreach ( var present in _roadObjects.ToList() )
				present.Destroy();

			_roadObjects.Clear();
		}
	}

	public void StartGame()
	{
		ClearMap();

		Points = 0;
		StartTimer = 0f;

		GenerateMap();

		var player = Scene.Components.GetAll<SantaPlayerSled>().FirstOrDefault();
		player.Unragdoll();
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
