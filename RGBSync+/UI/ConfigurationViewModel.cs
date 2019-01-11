using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using RGB.NET.Core;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using RGBSyncPlus.Properties;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MessageBox = System.Windows.MessageBox;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using RGBSyncPlus.Helper;
using Settings = RGBSyncPlus.Configuration.Settings;
using Application = System.Windows.Application;

namespace RGBSyncPlus.UI
{
    public sealed class ConfigurationViewModel : AbstractBindable, IDropTarget
    {
        #region Properties & Fields

        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public double UpdateRate
        {
            get => 1.0 / ApplicationManager.Instance.UpdateTrigger.UpdateFrequency;
            set
            {
                double val = MathHelper.Clamp(value, 1, 100);
                ApplicationManager.Instance.Settings.UpdateRate = val;
                ApplicationManager.Instance.UpdateTrigger.UpdateFrequency = 1.0 / val;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SyncGroup> _syncGroups;
        public ObservableCollection<SyncGroup> SyncGroups
        {
            get => _syncGroups;
            set => SetProperty(ref _syncGroups, value);
        }

        private SyncGroup _selectedSyncGroup;
        public SyncGroup SelectedSyncGroup
        {
            get => _selectedSyncGroup;
            set
            {
                if (SetProperty(ref _selectedSyncGroup, value))
                    UpdateLedLists();
            }
        }

        private ListCollectionView _availableSyncLeds;
        public ListCollectionView AvailableSyncLeds
        {
            get => _availableSyncLeds;
            set => SetProperty(ref _availableSyncLeds, value);
        }

        private ListCollectionView _availableLeds;
        public ListCollectionView AvailableLeds
        {
            get => _availableLeds;
            set => SetProperty(ref _availableLeds, value);
        }

        private ListCollectionView _synchronizedLeds;
        public ListCollectionView SynchronizedLeds
        {
            get => _synchronizedLeds;
            set => SetProperty(ref _synchronizedLeds, value);
        }

        #endregion

        #region Commands

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        private ActionCommand _exportCommand;
        public ActionCommand ExportCommand => _exportCommand ?? (_exportCommand = new ActionCommand(Export));

        private ActionCommand _importCommand;
        public ActionCommand ImportCommand => _importCommand ?? (_importCommand = new ActionCommand(Import));

        private ActionCommand _toggleAura;
        public ActionCommand ToggleAuraCommand => _toggleAura ?? (_toggleAura = new ActionCommand(ToggleAura));

        private ActionCommand _discord;
        public ActionCommand DiscordCommand => _discord ?? (_discord = new ActionCommand(Discord));

        private ActionCommand _addSyncGroupCommand;
        public ActionCommand AddSyncGroupCommand => _addSyncGroupCommand ?? (_addSyncGroupCommand = new ActionCommand(AddSyncGroup));

        private ActionCommand<SyncGroup> _removeSyncGroupCommand;
        public ActionCommand<SyncGroup> RemoveSyncGroupCommand => _removeSyncGroupCommand ?? (_removeSyncGroupCommand = new ActionCommand<SyncGroup>(RemoveSyncGroup));

        #endregion

        #region Constructors

        public ConfigurationViewModel()
        {
            SyncGroups = new ObservableCollection<SyncGroup>(ApplicationManager.Instance.Settings.SyncGroups);

            AvailableSyncLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(x => x.Device.DeviceInfo.SupportsSyncBack));
            OnPropertyChanged(nameof(AvailableSyncLeds));
        }

        #endregion

        #region Methods

        private ListCollectionView GetGroupedLedList(IEnumerable<Led> leds) => GetGroupedLedList(leds.Select(led => new SyncLed(led)).ToList());

        private ListCollectionView GetGroupedLedList(IList syncLeds)
        {
            ListCollectionView collectionView = new ListCollectionView(syncLeds);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SyncLed.Device)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.Device), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.LedId), ListSortDirection.Ascending));
            collectionView.Refresh();
            return collectionView;
        }

        private void UpdateLedLists()
        {
            SynchronizedLeds = GetGroupedLedList(SelectedSyncGroup.Leds);
            OnPropertyChanged(nameof(SynchronizedLeds));

            AvailableLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(led => !SelectedSyncGroup.Leds.Any(sc => (sc.LedId == led.Id) && (sc.Device == led.Device.GetDeviceName()))));
            OnPropertyChanged(nameof(AvailableLeds));
        }

        private void OpenHomepage() => Process.Start("https://www.fanman03.ml/");

        private void Discord() => Process.Start("https://discordapp.com/invite/pRyBKPr");

        private void Export()
        {
            string settingsPath = "Settings.json";
            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            string data = System.IO.File.ReadAllText(settingsPath);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Settings Files|*.json";
            saveFileDialog1.Title = "Export Settings";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
              System.IO.File.WriteAllText(saveFileDialog1.FileName, data);
            }
            else
            {
                MessageBox.Show("No file selected", "Error");
            }
        }

        private void Import()
        {
            string settingsPath = "Settings.json";
            string path = Environment.CurrentDirectory;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Browse for a settings file";
            openFileDialog1.Filter = "Settings files (*.json)|*.json";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                //Save the file
                string filename = openFileDialog1.FileName;
                string data = System.IO.File.ReadAllText(@filename);
                string settingsFile = path + "\\Settings.json";
                System.IO.File.WriteAllText("Settings.json", data);

                //Apply the changes
                Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath), new ColorSerializer());
                ConfigurationUpdates.PerformOn(settings);
                ApplicationManager.Instance.Settings = settings;
                ApplicationManager.Instance.Initialize();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("No file selected", "Error");
            }
        }

        private void AddSyncGroup()
        {
            SyncGroup syncGroup = new SyncGroup();
            SyncGroups.Add(syncGroup);
            ApplicationManager.Instance.AddSyncGroup(syncGroup);
        }

        private void ToggleAura()
        {
            if (File.Exists("DeviceProvider/RGB.NET.Devices.Asus1.dll") && File.Exists("DeviceProvider/RGB.NET.Devices.Asus2.dll.disabled"))
            {
                File.Move("DeviceProvider/RGB.NET.Devices.Asus1.dll", "DeviceProvider/RGB.NET.Devices.Asus1.dll.disabled");
                File.Move("DeviceProvider/RGB.NET.Devices.Asus2.dll.disabled", "DeviceProvider/RGB.NET.Devices.Asus2.dll");

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            } 
            else if (File.Exists("DeviceProvider/RGB.NET.Devices.Asus1.dll.disabled") && File.Exists("DeviceProvider/RGB.NET.Devices.Asus2.dll"))
            {
                File.Move("DeviceProvider/RGB.NET.Devices.Asus1.dll.disabled", "DeviceProvider/RGB.NET.Devices.Asus1.dll");
                File.Move("DeviceProvider/RGB.NET.Devices.Asus2.dll", "DeviceProvider/RGB.NET.Devices.Asus2.dll.disabled");

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            } else
            {
                MessageBox.Show("Asus device providers are missing, please reinstall the software", "Error");
            }
            
        }

        private void RemoveSyncGroup(SyncGroup syncGroup)
        {
            if (syncGroup == null) return;

            if (MessageBox.Show($"Are you sure that you want to delete the group '{syncGroup.DisplayName}'", "Remove LED Group?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            SyncGroups.Remove(syncGroup);
            ApplicationManager.Instance.RemoveSyncGroup(syncGroup);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is SyncLed || dropInfo.Data is IEnumerable<SyncLed>) && (dropInfo.TargetCollection is ListCollectionView))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetCollection is ListCollectionView targetList)) return;

            //HACK DarthAffe 04.06.2018: Super ugly hack - I've no idea how to do this correctly ...
            ListCollectionView sourceList = targetList == AvailableLeds ? SynchronizedLeds : AvailableLeds;

            if (dropInfo.Data is SyncLed syncLed)
            {
                targetList.AddNewItem(syncLed);
                sourceList.Remove(syncLed);

                targetList.CommitNew();
                sourceList.CommitEdit();
            }
            else if (dropInfo.Data is IEnumerable<SyncLed> syncLeds)
            {
                foreach (SyncLed led in syncLeds)
                {
                    targetList.AddNewItem(led);
                    sourceList.Remove(led);
                }
                targetList.CommitNew();
                sourceList.CommitEdit();
            }
        }

        #endregion
    }
}
