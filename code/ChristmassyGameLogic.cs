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
	public float MapRadius { get; set; } = 500f;

	[Property]
	[Category( "Map" )]
	public int CottagesSpawned { get; set; } = 20;

	/// <summary>
	/// How many degrees it rotates per second
	/// </summary>
	[Property]
	[Category( "Gameplay" )]
	[Range( 1f, 100f, 1f )]
	public float RotationSpeed { get; set; } = 5f;


	protected override void OnUpdate()
	{
		if ( !Map.IsValid() ) return;

		Map.WorldRotation *= Rotation.FromPitch( RotationSpeed * Time.Delta );
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
			var randomCottage = SceneUtility.GetPrefabScene( Game.Random.FromList( allCottages ) )
				.Clone();

			randomCottage.WorldRotation = Map.WorldRotation + Rotation.FromPitch( angleSlice * i );
			randomCottage.WorldPosition = Map.WorldPosition + randomCottage.WorldRotation.Up * MapRadius;
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
}
