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
	public float MaxTurnSpeed { get; set; } = 400f;

	public float Velocity = 0f;
	public float Height = 0f;

	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		MaxTurnSpeed += Time.Delta * 5;

		var roadWidth = ChristmassyGameLogic.Instance.RoadWidth / 1.5f;
		var inputs = Input.AnalogMove;
		Velocity = MathX.Lerp( Velocity, inputs.y * MaxTurnSpeed, Time.Delta * (inputs.y == 0 ? 2f : 1f) );
		Velocity = MathX.Clamp( Velocity, -MaxTurnSpeed, MaxTurnSpeed );
		WorldPosition = WorldPosition.WithY( MathX.Clamp( WorldPosition.y + Velocity * Time.Delta, -roadWidth, roadWidth ) );

		if ( WorldPosition.y <= -roadWidth || WorldPosition.y >= roadWidth )
			Velocity *= -0.5f;

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
		sleighRigidBody.Velocity += Vector3.Up * 1000f + Vector3.Backward * 500f;

		santa.Enabled = false;
		sleigh.Enabled = false;
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
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( other.Tags.Has( "gift" ) )
			ResetGift( other.GameObject );

		if ( other.Tags.Has( "obstacle" ) )
			Ragdoll();
	}

	private async void ResetGift( GameObject gift )
	{
		var currentSpeed = ChristmassyGameLogic.Instance.RotationSpeed;
		var resetTime = 180f / currentSpeed; // Reset when it's on the other side

		gift.Enabled = false;
		await Task.DelaySeconds( resetTime );
		gift.Enabled = true;
	}
}
