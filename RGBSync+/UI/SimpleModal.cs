using RGBSyncPlus.Helper;
using System;
using System.Threading.Tasks;

namespace RGBSyncPlus.UI
{
    public class SimpleModal : IDisposable
    {
        private readonly MainWindowViewModel viewmodel;
        private readonly bool actuallyDispose = true;
        public SimpleModal(MainWindowViewModel vm, string text, bool showPercentage = false)
        {
            if (vm != null)
            {
                if (vm.ShowModal)
                {
                    actuallyDispose = false;
                    Task.Delay(200).Wait();
                }
                else
                {
                    viewmodel = vm;
                    vm.ShowModal = true;
                    vm.ModalShowPercentage = showPercentage;
                    vm.ModalPercentage = 100;
                    vm.ModalText = text;
                    if (ApplicationManager.Instance?.ConfigurationWindow?.ContainingGrid != null)
                    {
                        Task.Delay(200).Wait();
                        ApplicationManager.Instance.ConfigurationWindow.ContainingGrid.Refresh();
                        Task.Delay(200).Wait();
                    }

                    Task.Delay(200).Wait();
                }
            }
        }

        public void UpdateModalPercentage(MainWindowViewModel vm, int perc)
        {
            if (vm != null)
            {
                vm.ModalPercentage = 100 - perc;
            }
        }
        public void Dispose()
        {
            if (viewmodel != null)
            {
                if (actuallyDispose)
                {
                    viewmodel.ShowModal = false;
                }
            }
        }
    }
}
