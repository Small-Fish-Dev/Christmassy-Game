using Sandbox;
using System;
using static Sandbox.Component;

public sealed class SantaPlayerSled : Component, ITriggerListener
{
	// TODO: SET wish_x wish_y wish_z

	[Property]
	public SkinnedModelRenderer ModelRenderer { get; set; }


	[Property]
	public float MaxTurnSpeed { get; set; } = 10f;

	public float Velocity = 0f;

	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		MaxTurnSpeed += Time.Delta * 0.2f;

		var roadWidth = ChristmassyGameLogic.Instance.RoadWidth / 1.5f;
		var inputs = Input.AnalogMove;
		Velocity = MathX.Lerp( Velocity, inputs.y * MaxTurnSpeed, Time.Delta * (inputs.y == 0 ? 2f : 1f) );
		Velocity = MathX.Clamp( Velocity, -MaxTurnSpeed, MaxTurnSpeed );
		WorldPosition = WorldPosition.WithY( MathX.Clamp( WorldPosition.y + Velocity, -roadWidth, roadWidth ) );

		if ( WorldPosition.y <= -roadWidth || WorldPosition.y >= roadWidth )
			Velocity *= -0.5f;

		// Animations
		if ( !ModelRenderer.IsValid() ) return;

		ModelRenderer.Set( "wish_y", inputs.y * -MaxTurnSpeed * 30f );
	}

	private ModelPhysics _ragdoll;

	[Button]
	public void Ragdoll()
	{
		if ( _ragdoll.IsValid() ) return;
		if ( !ModelRenderer.IsValid() ) return;

		foreach ( var child in ModelRenderer.GameObject.Children )
			child.Flags &= ~GameObjectFlags.ProceduralBone;

		_ragdoll = ModelRenderer.GameObject.AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;
	}

	[Button]
	public void Unragdoll()
	{
		if ( !_ragdoll.IsValid() ) return;
		if ( !ModelRenderer.IsValid() ) return;

		_ragdoll.Destroy();
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject == GameObject )
			return;

		ResetGift( other.GameObject );
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
