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
	public int LampsSpawned { get; set; } = 40;

	[Property]
	[Category( "Map" )]
	public int RoadObjects { get; set; } = 10;

	[Property]
	[Category( "Map" )]
	public Collider SlipperySlope { get; set; }

	[Property]
	[Category( "Map" )]
	public GameObject Lamp { get; set; }

	[Property]
	[Category( "Components" )]
	public SantaUI UI { get; set; }

	[Property]
	[Category( "Components" )]
	public SoundPointComponent WindSound { get; set; }

	[Property]
	[Category( "Components" )]
	public SoundPointComponent MusicSound { get; set; }


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
	public bool IsPlaying = true;

	public static ChristmassyGameLogic Instance { get; private set; }

	private float _originalSpeed;

	protected override void OnStart()
	{
		Instance = this;
		_originalSpeed = RotationSpeed;

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
		if ( !IsPlaying )
		{
			MusicSound.Volume = MathX.Lerp( MusicSound.Volume, 0f, Time.Delta * 3f );
			WindSound.Volume = 2f;
			return;
		}

		MusicSound.Volume = MathX.Lerp( MusicSound.Volume, 0.7f, Time.Delta * 3f );

		RotationSpeed += Time.Delta * 0.3f;
		WindSound.SoundOverride = true;
		WindSound.Volume = MathX.Remap( RotationSpeed, 20f, 100f, 2f, 1f );
		WindSound.Pitch = MathX.Remap( RotationSpeed, 20f, 100f, 0.1f, 1f );

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

			var randomPitch = Game.Random.Float( -angleSlice / 4f, angleSlice / 4f );
			randomCottage.WorldRotation = Rotation.FromPitch( angleSlice * i + randomPitch );
			var sideRotation = Rotation.FromYaw( -90f + 180f * side );
			randomCottage.WorldRotation *= sideRotation;

			randomCottage.WorldPosition = Map.WorldPosition + randomCottage.WorldRotation.Up * MapRadius;
			var randomDistance = Game.Random.Float( 200f, 400f );
			var sidePosition = (RoadWidth / 2f + randomDistance) * (side == 0 ? Vector3.Left : Vector3.Right);
			randomCottage.WorldPosition += sidePosition;
			randomCottage.SetParent( Map );

			_cottages.Add( randomCottage );
		}
	}

	private List<GameObject> _lamps = new List<GameObject>();

	[Button( "Generate Lamps Debug" )]
	public void GenerateLamps()
	{
		if ( !Map.IsValid() ) return;

		ClearLamps();

		if ( !Lamp.IsValid() )
		{
			Log.Error( "No lamp found" );
			return;
		}

		var angleSlice = 360f / LampsSpawned;

		for ( int i = 0; i < LampsSpawned; i++ )
		{
			var side = i % 2;
			var newLamp = Lamp.Clone();

			var randomPitch = Game.Random.Float( -angleSlice / 6f, angleSlice / 6f );
			newLamp.WorldRotation = Rotation.FromPitch( angleSlice * i + angleSlice / 2f + randomPitch );
			var sideRotation = Rotation.FromYaw( -90f + 180f * side );
			newLamp.WorldRotation *= sideRotation;

			newLamp.WorldPosition = Map.WorldPosition + newLamp.WorldRotation.Up * MapRadius;
			var randomDistance = Game.Random.Float( 100f, 150f );
			var sidePosition = (RoadWidth / 2f + randomDistance) * (side == 0 ? Vector3.Left : Vector3.Right);
			newLamp.WorldPosition += sidePosition;
			newLamp.SetParent( Map );

			_lamps.Add( newLamp );
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

	private List<GameObject> _trees = new List<GameObject>();

	[Button( "Generate Trees" )]
	public void GenerateTrees()
	{
		if ( !Map.IsValid() ) return;

		ClearTrees();

		var allTrees = ResourceLibrary.GetAll<PrefabFile>()
			.Where( x => x.GetMetadata( "Tags" )?.Contains( "tree" ) ?? false )
			.ToList();

		if ( !allTrees.Any() )
		{
			Log.Error( "No trees found" );
			return;
		}

		var angleSlice = 1f;

		for ( int i = 1; i < 360; i++ )
		{
			var side = Game.Random.Int( 0, 1 );
			var randomTree = SceneUtility.GetPrefabScene( Game.Random.FromList( allTrees ) )
				.Clone();

			randomTree.WorldRotation = Rotation.FromPitch( angleSlice * i );

			randomTree.WorldPosition = Map.WorldPosition + randomTree.WorldRotation.Up * MapRadius;
			var randomDistance = Game.Random.Float( 800f, 2400f );
			var sidePosition = (RoadWidth / 2f + randomDistance) * (side == 0 ? Vector3.Left : Vector3.Right);
			randomTree.WorldPosition += sidePosition;
			randomTree.SetParent( Map );

			_trees.Add( randomTree );
		}
	}

	public void GenerateMap()
	{
		GenerateCottages();
		GenerateRoad();
		GenerateLamps();

		MapClone = Map.Clone();
		MapClone.WorldPosition += Vector3.Forward * 2500f + Vector3.Down * 000f;
		MapClone.WorldRotation *= Rotation.FromPitch( -83f );
		MapClone.WorldScale *= 0.7f;
	}

	public void ClearMap()
	{
		ClearCottages();
		ClearRoad();
		ClearLamps();
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
	public void ClearTrees()
	{
		if ( _trees != null )
		{
			foreach ( var tree in _trees.ToList() )
				tree.Destroy();

			_trees.Clear();
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

	public void ClearLamps()
	{
		if ( _lamps != null )
		{
			foreach ( var lamp in _lamps.ToList() )
				lamp.Destroy();

			_lamps.Clear();
		}
	}

	public void StartGame()
	{
		ClearMap();

		Points = 0;
		StartTimer = 0f;
		RotationSpeed = _originalSpeed;

		GenerateMap();

		var player = Scene.Components.GetAll<SantaPlayerSled>().FirstOrDefault();
		player.Unragdoll();

		IsPlaying = true;
		MusicSound.StartSound();
	}

	public void EndGame()
	{
		IsPlaying = false;
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
