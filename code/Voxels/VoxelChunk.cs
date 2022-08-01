using Sandbox;
using System.Linq;

namespace Voxels
{
	public partial class VoxelChunk : ModelEntity
	{
		[Net] public ArrayVoxelData Data { get; private set; }
		[Net] public float Size { get; private set; }

		private Mesh _mesh;
		private Model _model;

		private bool _meshInvalid;
		private int _lastNetReadCount;

		public VoxelChunk()
		{

		}

		public VoxelChunk( ArrayVoxelData data, float size )
		{
			Data = data;
			Size = size;

            UsePhysicsCollision = true;

            EnableAllCollisions = true;

			CollisionBounds = new BBox( 0f, size );
		}

		public void InvalidateMesh()
		{
			_meshInvalid = true;
		}

		[Event.Tick.Client]
		public void ClientTick()
		{
			if ( _lastNetReadCount != Data.NetReadCount )
			{
				_lastNetReadCount = Data.NetReadCount;

				InvalidateMesh();
			}

			if ( _meshInvalid )
			{
				_meshInvalid = false;

				UpdateMesh( true, true );
			}
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			if ( _meshInvalid )
			{
				_meshInvalid = false;

				UpdateMesh( false, true );

				Data.WriteNetworkData();
			}
		}

		public void UpdateMesh( bool render, bool collision )
		{
			var writer = MarchingCubesMeshWriter.Rent();

			writer.Scale = Size;

			try
			{
				if ( render )
				{
					Data.UpdateMesh( writer, 0, true, false );

					if ( writer.Vertices.Count == 0 )
					{
						EnableDrawing = false;
						EnableShadowCasting = false;
						
						SetModel( "" );
					}
					else
					{
						if ( _mesh == null )
						{
							var material = Material.Load( "materials/voxeltest.vmat" );

							_mesh = new Mesh( material )
							{
								Bounds = new BBox( 0f, Size )
							};
						}

						if ( _mesh.HasVertexBuffer )
						{
							_mesh.SetVertexBufferSize( writer.Vertices.Count );
							_mesh.SetVertexBufferData( writer.Vertices );
						}
						else
						{
							_mesh.CreateVertexBuffer( writer.Vertices.Count, VoxelVertex.Layout, writer.Vertices );
						}

						_mesh.SetVertexRange( 0, writer.Vertices.Count );

						if ( _model == null )
						{
							var modelBuilder = new ModelBuilder();

							modelBuilder.AddMesh( _mesh );

							_model = modelBuilder.Create();
						}

						EnableDrawing = true;
						EnableShadowCasting = true;

                        Model = _model;
                    }
				}

				if ( collision )
				{
					Data.UpdateMesh( writer, 1, false, true );

					if ( writer.CollisionVertices.Count == 0 )
					{
						if ( PhysicsBody != null && PhysicsBody.IsValid() )
						{
							PhysicsBody.ClearShapes();
						}
					}
					else
					{
						if ( PhysicsBody == null || !PhysicsBody.IsValid() )
						{
							// Just to initialize PhysicsBody
							SetupPhysicsFromAABB( PhysicsMotionType.Static, 0f, Size );
						}

                        PhysicsBody.ClearShapes();
						PhysicsBody.AddMeshShape( writer.CollisionVertices.ToArray(), writer.CollisionIndices.ToArray() );
					}
				}
			}
			finally
			{
				writer.Return();
			}
		}

		protected override void OnDestroy()
		{

		}
	}
}
