using Sandbox;
using System;
using System.Runtime.InteropServices;

namespace Voxels
{
	public readonly struct Voxel
    {
        private static readonly float[] _sValueLookup = new float[256];

        private const float ColorScale = 1f / 255f;

		public static Voxel operator +( Voxel a, Voxel b )
        {
            var alpha = ColorScale * b.RawValue;

			return new Voxel( Math.Max( a.RawValue, b.RawValue ),
                (byte)MathF.Round( a.R * (1f - alpha) + b.R * alpha ),
                (byte)MathF.Round( a.G * (1f - alpha) + b.G * alpha ),
                (byte)MathF.Round( a.B * (1f - alpha) + b.B * alpha ) );
        }

		public static Voxel operator -( Voxel a, Voxel b )
		{
            return new Voxel( (byte) Math.Max( a.RawValue - b.RawValue, 0 ), a.R, a.G, a.B );
        }

		static Voxel()
		{
			for ( var i = 1; i < 255; ++i )
			{
				_sValueLookup[i] = (i - 127.5f) / 127.5f;
            }

			_sValueLookup[0] = -1f;
			_sValueLookup[255] = 1f;
        }

		public readonly byte RawValue;
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

		public float Value => _sValueLookup[RawValue];
        public Color Color => new Color( R * ColorScale, G * ColorScale, B * ColorScale );

		public Voxel( float value, byte r, byte g, byte b )
		{
			RawValue = (byte)Math.Clamp( (int)MathF.Round( value * 127.5f + 127.5f ), 0, 255 );
            R = r;
            G = g;
            B = b;
        }

		public Voxel( byte rawValue, byte r, byte g, byte b )
		{
			RawValue = rawValue;
            R = r;
            G = g;
            B = b;
        }

		public override string ToString()
		{
			return $"({Value:F2}, #{R:x2}{G:x2}{B:x2})";
		}
	}

	[StructLayout( LayoutKind.Sequential )]
	public readonly struct VoxelVertex
	{
		public static VertexAttribute[] Layout { get; } =
		{
			new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Tangent, VertexAttributeFormat.Float32),
            new VertexAttribute(VertexAttributeType.Color, VertexAttributeFormat.Float32)
		};

		public readonly Vector3 Position;
		public readonly Vector3 Normal;
		public readonly Vector3 Tangent;
        public readonly Vector3 Color;

		public VoxelVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 color)
		{
			Position = position;
			Normal = normal;
			Tangent = tangent;
            Color = color;
        }
	}
}
