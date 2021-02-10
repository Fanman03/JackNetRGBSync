using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.UI;
using RGBSyncPlus.UI.Tabs;

namespace RGBSyncPlus.Services
{
    public class ModalService
    {
        
        public void ShowModal(ModalModel modalModel)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ServiceManager.Instance.ApplicationManager.ConfigurationWindow.DataContext);

            vm.ModalText = modalModel.ModalText;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }

        public void ShowSimpleModal(string text)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ServiceManager.Instance.ApplicationManager.ConfigurationWindow.DataContext);

            vm.ModalText = text;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }


    }
}
