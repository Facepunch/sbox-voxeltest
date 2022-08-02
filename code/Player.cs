using Sandbox;
using Voxels;

namespace VoxelTest
{
	partial class Player : Sandbox.Player
    {
		[Net]
		public Cursor Cursor { get; set; }

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

			var pos = EyePosition + EyeRotation.Forward * 160f;

            Cursor.Position = pos;
            Cursor.Scale = 2f;

			if ( !IsServer )
				return;

			if ( LastEdit > 1f / 60f && (Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack )) )
			{
				var voxels = Game.Current.GetOrCreateVoxelVolume();
				var transform = Matrix.CreateTranslation( pos );

				if ( Input.Down( InputButton.PrimaryAttack ) )
				{
					var shape = new SphereSdf( Vector3.Zero, 8f, 32f );
					voxels.Add( shape, transform, 0 );
				}
				else
				{
					var shape = new SphereSdf( Vector3.Zero, 8f, 64f );
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
