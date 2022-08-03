using Sandbox;
using System.Diagnostics;
using Voxels;
using VoxelTest.UI;

namespace VoxelTest
{
	public partial class Game : Sandbox.Game
	{
		public new static Game Current => Sandbox.Game.Current as Game;

		public Hud Hud { get; }

        public Game()
        {
            if ( IsClient )
            {
                Hud = new Hud();
            }
        }

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new Player();
			client.Pawn = player;

			player.Respawn();
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

		}

		[Net] public VoxelVolume Voxels { get; private set; }

		[ConCmd.Server( "clear_voxels" )]
		public static void ClearVoxels()
		{
			Current.Voxels?.Delete();
			Current.Voxels = null;
		}

		public VoxelVolume GetOrCreateVoxelVolume()
		{
			if ( !IsServer )
			{
				throw new System.Exception( $"{nameof( GetOrCreateVoxelVolume )} can only be called on the server." );
			}

			if ( Voxels != null ) return Voxels;

			Voxels = new VoxelVolume( new Vector3( 32_768f, 32_768f, 32_768f ), 256f, 4, NormalStyle.Smooth );

			return Voxels;
		}

		[ConCmd.Server( "spawn_spheres" )]
		public static void SpawnSphere( int count = 1 )
		{
			var caller = ConsoleSystem.Caller;
			var voxels = Current.GetOrCreateVoxelVolume();

			var timer = new Stopwatch();
			timer.Start();

			var bounds = new BBox( new Vector3( -512f, -512f, 0f ), new Vector3( 512f, 512f, 512f ))
                         + caller.Pawn.Position + caller.Pawn.EyeRotation.Forward.WithZ(0).Normal * 512f;
			var smoothing = 16f;

			for ( var i = 0; i < count; ++i )
			{
				var radius = Rand.Float( 32f, 64f );
				var centerRange = new BBox( bounds.Mins + radius + smoothing, bounds.Maxs - radius - smoothing );
                var color = Color.Random;

                voxels.Add( new SphereSdf( centerRange.RandomPointInside, radius, smoothing ), Matrix.Identity, color );
            }

			Log.Info( $"Spawned {count} spheres in {timer.Elapsed.TotalMilliseconds:F2}ms" );
		}

		[ConCmd.Server( "spawn_boxes" )]
		public static void SpawnBoxes( int count = 1 )
		{
			var caller = ConsoleSystem.Caller;
			var voxels = Current.GetOrCreateVoxelVolume();

			var timer = new Stopwatch();
			timer.Start();

			var sizeRange = new BBox( 64f, 256f );

			var bounds = new BBox( new Vector3( -512f, -512f, 0f ), new Vector3( 512f, 512f, 512f ) )
                + caller.Pawn.Position + caller.Pawn.EyeRotation.Forward.WithZ( 0 ).Normal * 512f;
			var smoothing = 64f;

			for ( var i = 0; i < count; ++i )
			{
				var size = sizeRange.RandomPointInside;
				var centerRange = new BBox( bounds.Mins + size + smoothing, bounds.Maxs - size - smoothing );
				var center = centerRange.RandomPointInside;
                var color = Color.Random;

                var rotation = Rotation.Random;

				voxels.Add( new BBoxSdf( - size * 0.5f, size * 0.5f, smoothing ),
                     Matrix.CreateRotation( rotation ) * Matrix.CreateTranslation( center ), color );
			}

			Log.Info( $"Spawned {count} boxes in {timer.Elapsed.TotalMilliseconds:F2}ms" );
		}
	}
}
