using Sandbox;

public sealed class ChristmassyGameLogic : Component
{
	/// <summary>
	/// The map we're going to rotate
	/// </summary>
	[Property]
	public GameObject Map { get; set; }

	/// <summary>
	/// How many degrees it rotates per second
	/// </summary>
	[Property]
	[Range( 1f, 100f, 1f )]
	public float RotationSpeed { get; set; } = 5f;


	protected override void OnUpdate()
	{
		if ( !Map.IsValid() ) return;

		Map.WorldRotation *= Rotation.FromPitch( RotationSpeed * Time.Delta );
	}
}
