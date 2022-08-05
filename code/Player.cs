using System;
using System.Linq;
using Sandbox;
using Voxels;
using VoxelTest.UI;

namespace VoxelTest
{
	partial class Player : Sandbox.Player
    {
        private const float BaseInnerRadius = 8f;
        private const float BaseMaxDistance = 32f;

        private const int BrushScaleSteps = 4;

        private const float MinBrushSize = 32f;
		private const float MaxBrushSize = 128f;

        public static Color[] BrushColors { get; } = new[] { Color.White }
            .Concat( Enumerable.Range( 0, 8 )
                .Select( x => new ColorHsv( x * 360f / 8f, 0.75f, 1f ).ToColor() ) )
            .Concat( new[] { Color.Black } ).ToArray();

		[Net]
		public Cursor Cursor { get; set; }

        private int _materialIndex;

        [Net, Predicted] public float BrushSize { get; set; } = 32f;

        [Net, Predicted] public Plane? SnapPlane { get; set; }

        [Net, Predicted]
        public int MaterialIndex
        {
            get => _materialIndex;
            set
            {
                _materialIndex = value;

                if ( IsClient )
                {
                    ColorBlob.ActiveSlotIndex = value;
                }
            }
        }

        public ClothingContainer Clothing { get; } = new();

        private Vector3 _lastPaintPosition;

        public Player()
        {

        }

        public Player( Client cl )
        {
            Clothing.LoadFromClient( cl );
        }

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new ThirdPersonCamera();

            Clothing.DressEntity( this );

            EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

            if ( IsServer )
            {
                Cursor ??= new Cursor();
                Cursor.Owner = this;
            }

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
                if ( Input.Down( InputButton.Run ) )
                {
                    MaterialIndex = (MaterialIndex + Math.Sign( Input.MouseWheel ) + BrushColors.Length) % BrushColors.Length;
                }
                else
                {
                    BrushSize *= MathF.Pow(2f, (float)Input.MouseWheel / BrushScaleSteps);
                }
            }

            BrushSize = Math.Clamp(BrushSize, MinBrushSize, MaxBrushSize);

            var pos = EyePosition + EyeRotation.Forward * (128f + BrushSize * 2f);

            if ( Input.Pressed( InputButton.Walk ) )
            {
                SnapPlane = new Plane( pos, -EyeRotation.Forward );
            }
            
            if ( Input.Released( InputButton.Walk ) )
            {
                SnapPlane = null;
            }

            if ( SnapPlane.HasValue )
            {
                pos = SnapPlane.Value.Trace( new Ray( EyePosition, EyeRotation.Forward ) ) ?? pos;
            }

            Cursor.Position = pos;
            Cursor.Scale = BrushSize / 16f;

            if ( !IsServer )
				return;

			if ( Input.Down( InputButton.PrimaryAttack ) || Input.Down( InputButton.SecondaryAttack ) )
            {
                var dist = (pos - _lastPaintPosition).Length;

                if ( dist >= BrushSize / 8f )
                {
                    _lastPaintPosition = pos;

                    var voxels = Game.Current.GetOrCreateVoxelVolume();
                    var transform = Matrix.CreateTranslation(pos);
                    var gradientWidth = 2f * voxels.ChunkSize / (1 << voxels.ChunkSubdivisions);

                    if (Input.Down(InputButton.PrimaryAttack))
                    {
                        var shape = new SphereSdf( Vector3.Zero, BrushSize, gradientWidth );
                        voxels.Add(shape, transform, BrushColors[MaterialIndex]);
                    }
                    else
                    {
                        var shape = new SphereSdf( Vector3.Zero, BrushSize - gradientWidth, gradientWidth );
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
