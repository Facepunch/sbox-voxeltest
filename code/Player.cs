using System;
using Sandbox;
using Voxels;

namespace VoxelTest
{
	partial class Player : Sandbox.Player
    {
        private const float BaseInnerRadius = 8f;
        private const float BaseMaxDistance = 32f;

        private const int BrushScaleSteps = 4;

        private const float MinBrushScale = 0.5f;
		private const float MaxBrushScale = 4f;

		[Net]
		public Cursor Cursor { get; set; }

        [Net] public float BrushScale { get; set; } = 1f;

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new ThirdPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			
            Cursor ??= new Cursor();
            Cursor.Owner = this;

			base.Respawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;
		}

		public TimeSince LastEdit { get; private set; }
        public TimeSince LastJump { get; private set; }

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

            if ( Input.Pressed( InputButton.Jump ) )
            {
                if (LastJump < 0.25f )
                {
                    if ( Controller is NoclipController )
					{
						Controller = new WalkController();
					}
                    else
					{
						Controller = new NoclipController();
					}
                }

                LastJump = 0f;
			}

            if ( Input.MouseWheel != 0 )
            {
                BrushScale *= MathF.Pow( 2f, (float) Input.MouseWheel / BrushScaleSteps );
            }

            BrushScale = Math.Clamp( BrushScale, MinBrushScale, MaxBrushScale );

			var pos = EyePosition + EyeRotation.Forward * (128f + BrushScale * 64f);

            Cursor.Position = pos;
            Cursor.Scale = 2f * BrushScale;

			if ( !IsServer )
				return;

			if ( LastEdit > 1f / 60f && (Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack )) )
			{
				var voxels = Game.Current.GetOrCreateVoxelVolume();
				var transform = Matrix.CreateTranslation( pos );

				if ( Input.Down( InputButton.PrimaryAttack ) )
				{
					var shape = new SphereSdf( Vector3.Zero, 8f * BrushScale, 32f * BrushScale );
					voxels.Add( shape, transform, 0 );
				}
				else
				{
					var shape = new SphereSdf( Vector3.Zero, 8f * BrushScale, 32f * BrushScale );
					voxels.Subtract( shape, transform, 0 );
				}
			}

			LastEdit = 0f;

			if ( Input.Pressed( InputButton.Flashlight ) )
			{
				var r = Input.Rotation;
				var ent = new Prop
				{
					Position = EyePosition + r.Forward * 50,
					Rotation = r
				};

				ent.SetModel( "models/citizen_props/crate01.vmdl" );
				ent.Velocity = r.Forward * 1000;
			}
		}
	}
}
