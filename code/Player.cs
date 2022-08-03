using System;
using System.Linq;
using Sandbox;
using Voxels;

namespace VoxelTest
{
	partial class Player : Sandbox.Player
    {
        private const float BaseInnerRadius = 8f;
        private const float BaseMaxDistance = 32f;

        private const int BrushScaleSteps = 4;

        private const float MinBrushScale = 1f;
		private const float MaxBrushScale = 4f;

        public static Color[] BrushColors { get; } = new[] { Color.White }
            .Concat( Enumerable.Range( 0, 8 )
                .Select( x => new ColorHsv( x * 360f / 8f, 0.75f, 1f ).ToColor() ) )
            .Concat( new[] { Color.Black } ).ToArray();

		[Net]
		public Cursor Cursor { get; set; }

        [Net] public float BrushScale { get; set; } = 1f;
		[Net] public int MaterialIndex { get; set; }

        private Vector3 _lastPaintPosition;

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
		
        public TimeSince LastJump { get; private set; }

        private readonly InputButton[] _slots = new InputButton[]
        {
            InputButton.Slot1,
            InputButton.Slot2,
            InputButton.Slot3,
            InputButton.Slot4,
            InputButton.Slot5,
            InputButton.Slot6,
            InputButton.Slot7,
            InputButton.Slot8,
            InputButton.Slot9,
            InputButton.Slot0
        };

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

            foreach ( var slot in _slots )
            {
                if ( Input.Pressed( slot ) )
                {
                    MaterialIndex = Array.IndexOf( _slots, slot );
				}
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

			if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack ) )
            {
                var dist = (pos - _lastPaintPosition).Length;

                if ( dist >= 8f * BrushScale )
                {
                    _lastPaintPosition = pos;

                    var voxels = Game.Current.GetOrCreateVoxelVolume();
                    var transform = Matrix.CreateTranslation(pos);

                    if (Input.Down(InputButton.PrimaryAttack))
                    {
                        var shape = new SphereSdf(Vector3.Zero, 4f * BrushScale, 32f * BrushScale);
                        voxels.Add(shape, transform, BrushColors[MaterialIndex]);
                    }
                    else
                    {
                        var shape = new SphereSdf(Vector3.Zero, 4f * BrushScale, 32f * BrushScale);
                        voxels.Subtract(shape, transform, BrushColors[MaterialIndex]);
                    }
				}
			}
            else
            {
                _lastPaintPosition = new Vector3( 0f, 0f, -float.MaxValue );
            }

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
