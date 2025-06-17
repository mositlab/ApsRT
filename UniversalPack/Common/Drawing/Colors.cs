using System.Drawing;

namespace UniversalPack.Common.Drawing
{
    public static class Colors
    {
        public static Color MOSITLAB_BLUE { get; } = Color.FromArgb(47, 184, 196);
        public static Color MOSITLAB_BLUE_DARK { get; } = Color.FromArgb(5, 95, 103);
        public static Color MOSITLAB_RED { get; } = Color.FromArgb(234, 84, 84);
        public static Color MOSITLAB_RED_DARK { get; } = Color.FromArgb(180, 60, 61);

        public static Color[] StaticColors = new Color[]
        {
            Color.Red,
            Color.DarkRed,
            Color.Orange,
            Color.DarkOrange,
            Color.Yellow,
            Color.YellowGreen,
            Color.Lime,
            Color.Green,
            Color.DarkGreen,
            //Color.AliceBlue,
            Color.LightBlue,
            Color.RoyalBlue,
            Color.Blue,
            Color.BlueViolet,
            Color.Violet,
            Color.DarkViolet,
            Color.Brown,
            Color.Gray,
            Color.DarkGray,
            Color.Black
        };

        public static Color NewRandomColor()
        {
            var r = Random.Shared;
            return Color.FromArgb(r.Next(100, 256), r.Next(100, 256), r.Next(100, 256));
        }
    }
}
