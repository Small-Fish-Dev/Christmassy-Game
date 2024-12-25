using Sandbox;
using System;
using static Sandbox.Component;

public sealed class SantaPlayerSled : Component, ITriggerListener
{
	// TODO: SET wish_x wish_y wish_z

	[Property]
	public SkinnedModelRenderer ModelRenderer { get; set; }


	[Property]
	public float MaxTurnSpeed { get; set; } = 20f;

	public float Velocity = 0f;

	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		var roadWidth = ChristmassyGameLogic.Instance.RoadWidth / 1.5f;
		var inputs = Input.AnalogMove;
		Velocity = MathX.Lerp( Velocity, inputs.y * MaxTurnSpeed, Time.Delta * (inputs.y == 0 ? 2f : 1f) );
		Velocity = MathX.Clamp( Velocity, -MaxTurnSpeed, MaxTurnSpeed );
		WorldPosition = WorldPosition.WithY( MathX.Clamp( WorldPosition.y + Velocity, -roadWidth, roadWidth ) );

		if ( WorldPosition.y <= -roadWidth || WorldPosition.y >= roadWidth )
			Velocity *= -0.5f;

		// Animations
		if ( !ModelRenderer.IsValid() ) return;

		ModelRenderer.Set( "wish_y", inputs.y * -5000f );
	}
	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject == GameObject )
			return;

		other.GameObject.Destroy();
	}
}
