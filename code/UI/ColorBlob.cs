using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace VoxelTest.UI
{
    [UseTemplate]
    public partial class ColorBlob : Panel
    {
        public static int ActiveSlotIndex { get; set; }

        public int Index { get; set; }
        public string SlotNumber => ((Index + 1) % 10).ToString();

        public Color Color => Player.BrushColors[Index];

        private Panel RedBlob { get; set; }
        private Panel GreenBlob { get; set; }
        private Panel BlueBlob { get; set; }

        public override void Tick()
        {
            base.Tick();

            RedBlob.Style.Opacity = Color.r;
            GreenBlob.Style.Opacity = Color.g;
            BlueBlob.Style.Opacity = Color.b;

            SetClass( "selected", ActiveSlotIndex == Index );
        }
    }
}
