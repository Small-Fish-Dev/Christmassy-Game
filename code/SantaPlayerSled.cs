using Sandbox;
using System;
using System.Text.Json.Nodes;
using static Sandbox.Component;

public sealed class SantaPlayerSled : Component, ITriggerListener
{

	[Property]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	[Property]
	public ModelRenderer Sleigh { get; set; }

	[Property]
	public GameObject Pivot { get; set; }

	[Property]
	public BoxCollider Collider { get; set; }


	[Property]
	public float MaxTurnSpeed { get; set; } = 400f;

	public float Velocity = 0f;
	public float Height = 0f;
	public bool CanJump => Pivot.LocalPosition.z <= 0f;

	private float _targetPitch = 0f;
	private float _originalTurnSpeed;

	protected override void OnStart()
	{
		_originalTurnSpeed = MaxTurnSpeed;
	}

	protected override void OnFixedUpdate()
	{
		MaxTurnSpeed += Time.Delta * 5;
		var roadWidth = ChristmassyGameLogic.Instance.RoadWidth / (Pivot.LocalPosition.z > 0f ? 0.5f : 1.5f);
		var inputs = Input.AnalogMove;
		Velocity = MathX.Lerp( Velocity, inputs.y * MaxTurnSpeed, Time.Delta * (inputs.y == 0 ? 2f : 1f) );
		Velocity = MathX.Clamp( Velocity, -MaxTurnSpeed, MaxTurnSpeed );
		WorldPosition = WorldPosition.WithY( MathX.Clamp( WorldPosition.y + Velocity * Time.Delta, -roadWidth, roadWidth ) );

		if ( CanJump && (WorldPosition.y <= -roadWidth || WorldPosition.y >= roadWidth) )
			Velocity *= -0.25f;

		if ( Input.Pressed( "jump" ) && CanJump && Pivot.IsValid() )
		{
			Height += 350f;
			_targetPitch = -30f;
		}

		if ( Pivot.LocalPosition.z > 0f || Height > 0f )
		{
			Pivot.LocalPosition += Vector3.Up * Time.Delta * Height;
			Pivot.LocalPosition = Pivot.LocalPosition.WithZ( MathF.Max( Pivot.LocalPosition.z, 0f ) );

			Collider.Center = Pivot.LocalPosition + Vector3.Up * 35f;
			Height -= Time.Delta * 1000f;
		}

		if ( CanJump && Height < 0f )
		{
			Collider.Center = Pivot.LocalPosition + Vector3.Up * 35f;
			Height = 0f;
			_targetPitch = 0f;

			if ( WorldPosition.y <= -roadWidth / 2.5f || WorldPosition.y >= roadWidth / 2.5f )
				Ragdoll();
		}

		var targetRotation = Rotation.FromPitch( _targetPitch ) * Rotation.FromYaw( Velocity / 10f );

		Pivot.LocalRotation = Rotation.Lerp( Pivot.LocalRotation, targetRotation, Time.Delta * 20f );

		// Animations
		if ( !ModelRenderer.IsValid() ) return;

		ModelRenderer.Set( "wish_y", inputs.y * -MaxTurnSpeed );
	}

	private GameObject _oldSanta;
	private GameObject _oldSleigh;

	[Button]
	public void Ragdoll()
	{
		if ( _oldSanta.IsValid() ) return;
		if ( !ModelRenderer.IsValid() ) return;

		Sound.Play( new SoundEvent( "sounds/impact.sound" ), WorldPosition );


		var santa = ModelRenderer.GameObject;
		_oldSanta = santa.Clone( santa.WorldPosition, santa.WorldRotation, santa.WorldScale );
		_oldSanta.Tags.Remove( "player" );
		var ragdollRenderer = _oldSanta.GetComponent<SkinnedModelRenderer>();

		foreach ( var child in _oldSanta.Children )
			child.Flags &= ~GameObjectFlags.ProceduralBone;

		var santaPhysics = _oldSanta.AddComponent<ModelPhysics>();
		santaPhysics.Renderer = ragdollRenderer;
		santaPhysics.Model = ragdollRenderer.Model;

		var sleigh = Sleigh.GameObject;
		_oldSleigh = sleigh.Clone( sleigh.WorldPosition, sleigh.WorldRotation, sleigh.WorldScale );
		_oldSleigh.Tags.Remove( "player" );
		var sleighRigidBody = _oldSleigh.AddComponent<Rigidbody>();
		sleighRigidBody.Velocity += Vector3.Up * 1000f + Vector3.Backward * 500f + Vector3.Left * Velocity;

		santa.Enabled = false;
		sleigh.Enabled = false;
		Collider.Enabled = false;

		ChristmassyGameLogic.Instance.EndGame();
	}

	[Button]
	public void Unragdoll()
	{
		if ( !_oldSanta.IsValid() ) return;
		if ( !ModelRenderer.IsValid() ) return;

		_oldSanta.Destroy();
		_oldSleigh.Destroy();

		ModelRenderer.GameObject.Enabled = true;
		Sleigh.GameObject.Enabled = true;
		Collider.Enabled = true;
		MaxTurnSpeed = _originalTurnSpeed;
		Height = 0f;
		Velocity = 0f;
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( other.Tags.Has( "gift" ) )
			UnwrapGift( other.GameObject );

		if ( other.Tags.Has( "obstacle" ) )
			Ragdoll();
	}

	private async void UnwrapGift( GameObject gift )
	{
		ChristmassyGameLogic.Instance.Points += 10;

		var currentSpeed = ChristmassyGameLogic.Instance.RotationSpeed;
		var resetTime = 180f / currentSpeed; // Reset when it's on the other side

		gift.Enabled = false;
		await Task.DelaySeconds( resetTime );
		gift.Enabled = true;
	}
}
