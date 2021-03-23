using MarkdownUI.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdrownUI.Tester
{
    public class DemoVM : MarkDownViewModel
    {
        public DemoVM()
        {
            MadLedViewDevices = new List<MadLedViewDevice>();
            MadLedViewDevices.Add(new MadLedViewDevice()
            {
                Name = "test 1",
                LedCount = "8",
                Serial = "1234"
            });

            MadLedViewDevices.Add(new MadLedViewDevice()
            {
                Name = "test 2",
                LedCount = "16",
                Serial = "5678"
            });
        }
        private string textBoxText = "example Text";
        public string TextBoxText
        {
            get => textBoxText;
            set => Set(ref textBoxText, value);
        }

        private string sliderValue = "17";
        public string SliderValue
        {
            get => sliderValue;
            set => Set(ref sliderValue, value);
        }

        public void TestClick()
        {

            var ca = TextBoxText.ToCharArray();
            Array.Reverse(ca);
            TextBoxText = new string(ca);
        }


        public List<MadLedViewDevice> MadLedViewDevices { get; set; }

        public class MadLedViewDevice : MarkDownViewModel
        {
            private string serialNumber;
            public string Serial
            {
                get => serialNumber;
                set => Set(ref serialNumber, value);
            }

            private string name;
            public string Name
            {
                get => name;
                set => Set(ref name, value);
            }

            private string ledCount = "0";
            public string LedCount
            {
                get => ledCount;
                set => Set(ref ledCount, value);
            }
        }
    }
}
