using SyncStudio.WPF.UI;
using SyncStudio.WPF.UI.Tabs;

namespace SyncStudio.WPF.Services
{
    public class ModalService
    {
        public void ShowModal(ModalModel modalModel)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ServiceManager.Instance.ApplicationManager.MainWindow.DataContext);

            vm.ModalText = modalModel.ModalText;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }

        public void ShowSimpleModal(string text)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ServiceManager.Instance.ApplicationManager.MainWindow.DataContext);

            vm.ModalText = text;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }


    }
}
