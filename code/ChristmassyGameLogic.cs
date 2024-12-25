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
	public int PresentsSpawned { get; set; } = 10;

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

	public static ChristmassyGameLogic Instance { get; private set; }

	protected override void OnStart()
	{
		GenerateMap();

		MapClone = Map.Clone();
		MapClone.WorldPosition += Vector3.Forward * 2500f + Vector3.Down * 000f;
		MapClone.WorldRotation *= Rotation.FromPitch( -90f );
		MapClone.WorldScale *= 0.7f;

		Instance = this;
		StartTimer = 0f;
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

	private List<GameObject> _presents = new List<GameObject>();

	[Button( "Generate Presents Debug" )]
	public void GeneratePresents()
	{
		if ( !Map.IsValid() ) return;

		ClearPresents();

		var allPresents = ResourceLibrary.GetAll<PrefabFile>()
			.Where( x => x.GetMetadata( "Tags" )?.Contains( "gift" ) ?? false )
			.ToList();

		if ( !allPresents.Any() )
		{
			Log.Error( "No presents found" );
			return;
		}

		var angleSlice = 360f / PresentsSpawned;

		for ( int i = 0; i < PresentsSpawned; i++ )
		{
			var side = Game.Random.Int( -1, 1 );
			var randomPresent = SceneUtility.GetPrefabScene( Game.Random.FromList( allPresents ) )
				.Clone();

			randomPresent.WorldRotation = Rotation.FromPitch( angleSlice * i );

			randomPresent.WorldPosition = Map.WorldPosition + randomPresent.WorldRotation.Up * MapRadius;
			var sidePosition = Vector3.Left * (RoadWidth - 80f) * side;
			randomPresent.WorldPosition += sidePosition;
			randomPresent.SetParent( Map );

			_presents.Add( randomPresent );
		}
	}

	public void GenerateMap()
	{
		GenerateCottages();
		GeneratePresents();
	}

	public void ClearMap()
	{
		ClearCottages();
		ClearPresents();
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

	public void ClearPresents()
	{
		if ( _presents != null )
		{
			foreach ( var present in _presents.ToList() )
				present.Destroy();

			_presents.Clear();
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
