using RGBSyncStudio.UI;
using RGBSyncStudio.UI.Tabs;

namespace RGBSyncStudio.Services
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
