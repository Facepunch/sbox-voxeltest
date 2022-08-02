using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace VoxelTest
{
    public partial class Cursor : ModelEntity
    {
        public override void Spawn()
        {
            base.Spawn();

            Predictable = true;

            SetModel( "models/cursor.mdl" );
        }
    }
}
