﻿using RGBSyncStudio.Helper;
using System;
using System.Threading.Tasks;

namespace RGBSyncStudio.UI
{
    public class SimpleModal : IDisposable
    {
        private readonly MainWindowViewModel viewmodel;
        private readonly bool actuallyDispose = true;
        public SimpleModal(MainWindowViewModel vm, string text, bool showPercentage = false, bool showProgressBar = false)
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
                    vm.ShowModalProgressBar = showProgressBar;
                    vm.ModalPercentage = 100;
                    vm.ModalText = text;
                    if (ServiceManager.Instance.ApplicationManager?.MainWindow?.ContainingGrid != null)
                    {
                        Task.Delay(200).Wait();
                        ServiceManager.Instance.ApplicationManager.MainWindow.ContainingGrid.Refresh();
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
