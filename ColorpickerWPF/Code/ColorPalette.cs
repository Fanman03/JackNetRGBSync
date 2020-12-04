using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ColorPickerWPF.Code
{
    [Serializable]
    public class ColorPalette
    {
        public List<ColorSwatchItem> BuiltInColors { get; set; }

        public List<ColorSwatchItem> CustomColors { get; set; }

        [XmlIgnore]
        protected const int NumColorsFirstSwatch = 39;
        [XmlIgnore]
        protected const int NumColorsCustomSwatch = 44;
        [XmlIgnore]
        protected const int NumColorsSecondSwatch = 112;

        public ColorPalette()
        {
            BuiltInColors = new List<ColorSwatchItem>();
            CustomColors = new List<ColorSwatchItem>();
        }

        public void InitializeDefaults()
        {
            BuiltInColors.Clear();
            BuiltInColors.AddRange(
                GetColorSwatchItems(
                new List<Color>()
            {
/*                Colors.Black,
                Colors.Red,
                Colors.DarkOrange,
                Colors.Yellow,
                Colors.LawnGreen,
                Colors.Blue,
                Colors.Purple,
                Colors.DeepPink,
                Colors.Aqua,
                Colors.SaddleBrown,
                Colors.Wheat,
                Colors.BurlyWood,
                Colors.Teal,*/


                Color.FromArgb(255,255, 0, 0),
                Color.FromArgb(255,255, 64, 0),
                Color.FromArgb(255,255, 128, 0),
                Color.FromArgb(255,255, 191, 0),
                Color.FromArgb(255,255, 255, 0),
                Color.FromArgb(255,191, 255, 0),
                Color.FromArgb(255,128, 255, 0),
                Color.FromArgb(255,64, 255, 0),
                Color.FromArgb(255,0, 255, 0),
                Color.FromArgb(255,0, 255, 64),
                Color.FromArgb(255,0, 255, 128),
                Color.FromArgb(255,0, 255, 191),
                Color.FromArgb(255,0, 255, 255),

                Color.FromArgb(255,0, 191, 255),
                Color.FromArgb(255,0, 128, 255),
                Color.FromArgb(255,0, 64, 255),
                Color.FromArgb(255,0, 0, 255),
                Color.FromArgb(255,64, 0, 255),
                Color.FromArgb(255,128, 0, 255),
                Color.FromArgb(255,191, 0, 255),
                Color.FromArgb(255,255, 0, 255),
                Color.FromArgb(255,255, 0, 191),
                Color.FromArgb(255,255, 0, 128),
                Color.FromArgb(255,255, 0, 64),
                Colors.White,
                Colors.Black,

                Colors.Tan,
                Color.FromArgb(255,128, 0, 0),
                Color.FromArgb(255,128, 64, 0),
                Color.FromArgb(255,128, 128, 0),
                Color.FromArgb(255,64, 128, 0),
                Color.FromArgb(255,0, 128, 0),
                Color.FromArgb(255,0, 128, 64),
                Color.FromArgb(255,0, 128, 128),
                Color.FromArgb(255,0, 64, 128),
                Color.FromArgb(255,0, 0, 128),
                Color.FromArgb(255,64, 0, 128),
                Color.FromArgb(255,128, 0, 128),
                Color.FromArgb(255,128, 0, 64),


                Colors.Transparent,
                Colors.Transparent,
                Colors.Transparent,

                Color.FromArgb(255, 5, 5, 5),
                Color.FromArgb(255, 15, 15, 15),
                Color.FromArgb(255, 35, 35, 35),
                Color.FromArgb(255, 55, 55, 55),
                Color.FromArgb(255, 75, 75, 75),
                Color.FromArgb(255, 95, 95, 95),
                Color.FromArgb(255, 115, 115, 115),
                Color.FromArgb(255, 135, 135, 135),
                Color.FromArgb(255, 155, 155, 155),
                Color.FromArgb(255, 175, 175, 175),
                Color.FromArgb(255, 195, 195, 195),
                Color.FromArgb(255, 215, 215, 215),
                Color.FromArgb(255, 235, 235, 235),
            }));

            CustomColors.Clear();
            CustomColors.AddRange(Enumerable.Repeat(Colors.White, NumColorsCustomSwatch)
                .Select(x => new ColorSwatchItem() { Color = x, HexString = x.ToHexString() })
                .ToList());
        }


        protected List<ColorSwatchItem> GetColorSwatchItems(List<Color> colors)
        {
            return colors.Select(x => new ColorSwatchItem() { Color = x, HexString = x.ToHexString() }).ToList();
        } 
    }
}
