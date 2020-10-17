using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class UpgradingViewModel : BaseViewModel
    {
        private string message;
        public string Message
        {
            get => message;
            set => Set(ref message,value);
        }

        private int percentage = 100;
        public int Percentage
        {
            get => percentage;
            set => Set(ref percentage, value);
        }
    }
}
