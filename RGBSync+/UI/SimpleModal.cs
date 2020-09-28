using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus.UI
{
    public class SimpleModal : IDisposable
    {
        private MainWindowViewModel viewmodel;
        private bool actuallyDispose = true;
        public SimpleModal(MainWindowViewModel vm, string text)
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
