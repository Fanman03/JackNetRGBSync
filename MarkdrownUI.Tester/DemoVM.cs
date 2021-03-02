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
        private string textBoxText = "1337";
        public string TextBoxText
        {
            get => textBoxText;
            set => Set(ref textBoxText, value);
        }

        private string sliderValue;
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

        public List<TestObject> TestList { get; set; } = new List<TestObject>
        {
            new TestObject{Name="one",Number="1"},
            new TestObject{Name="two",Number="2"},
            new TestObject{Name="three",Number="3"},
        };

        public class TestObject : MarkDownViewModel
        {
            private string name;
            public string Name
            {
                get => name;
                set => Set(ref name, value);
            }

            private string number;
            public string Number
            {
                get => number;
                set => Set(ref number, value);
            }
        }

    }
}
