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
    public class ColorBlob : Panel
    {
        public static int ActiveSlotIndex { get; set; }

        public int Index { get; set; }
        public string SlotNumber => ((Index + 1) % 10).ToString();

        public Color Color => Player.BrushColors[Index];

        public string RedStyle => $"opacity: {Color.r:F2};";
        public string GreenStyle => $"opacity: {Color.g:F2};";
        public string BlueStyle => $"opacity: {Color.b:F2};";

        public override void Tick()
        {
            base.Tick();

            SetClass( "selected", ActiveSlotIndex == Index );
        }
    }
}
