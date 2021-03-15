using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.WPF.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a basic bindable class which notifies when a property value changes.
    /// </summary>
    public abstract class BaseViewModel : IBindable
    {

        protected BaseViewModel()
        {

            InternalSolids.themeWatcher.OnThemeChanged += Watcher_OnThemeChanged;

            var test = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            AccentColor = test.Color;
            AccentSolidColorBrush = new SolidColorBrush(AccentColor);
            AccentSolidColorBrush.Freeze();

            RaiseChanges();
        }

        private void Watcher_OnThemeChanged(object sender, ThemeWatcher.ThemeChangeEventArgs e)
        {
            CurrentTheme = e.CurrentTheme;

            PrimaryColor = CurrentTheme == ThemeWatcher.WindowsTheme.Dark ? Colors.White : Colors.Black;
            PrimarySolidColorBrush = new SolidColorBrush(PrimaryColor);

            SecondaryColor = CurrentTheme == ThemeWatcher.WindowsTheme.Dark ? Colors.Black : Colors.White;
            SecondarySolidColorBrush = new SolidColorBrush(SecondaryColor);

            AccentSolidColorBrush = new SolidColorBrush(e.AccentColor);
            AccentSolidColorBrush.Freeze();

            AccentColor = e.AccentColor;

            Debug.WriteLine("ThemeChange: ***********");
            Debug.WriteLine("Primary: " + PrimaryColor);
            Debug.WriteLine("Secondary: " + SecondaryColor);
            Debug.WriteLine("Accent: " + AccentColor);

            RaiseChanges();
        }


        private SolidColorBrush primarySolidColorBrush = new SolidColorBrush(Colors.White);

        [JsonIgnore]
        public SolidColorBrush PrimarySolidColorBrush
        {
            get => primarySolidColorBrush;
            set
            {
                value.Freeze();
                SetProperty(ref primarySolidColorBrush, value);
            }
        }

        private SolidColorBrush secondarySolidColorBrush = new SolidColorBrush(Colors.Black);
        [JsonIgnore]
        public SolidColorBrush SecondarySolidColorBrush
        {
            get => secondarySolidColorBrush;
            set
            {
                value.Freeze();
                SetProperty(ref secondarySolidColorBrush, value);
            }
        }

        private SolidColorBrush accentSolidColorBrush = new SolidColorBrush(Colors.CornflowerBlue);
        [JsonIgnore]
        public SolidColorBrush AccentSolidColorBrush
        {
            get => accentSolidColorBrush;
            set
            {
                value.Freeze();
                SetProperty(ref accentSolidColorBrush, value);
            }
        }

        private Color primaryColor = Colors.White;
        private Color secondaryColor = Colors.Black;
        private Color accentColor = Colors.CornflowerBlue;
        [JsonIgnore]
        public Color PrimaryColor
        {
            get => primaryColor;
            set { SetProperty(ref primaryColor, value); }
        }

        [JsonIgnore]
        public Color SecondaryColor
        {
            get => secondaryColor;
            set => SetProperty(ref secondaryColor, value);
        }

        [JsonIgnore]
        public Color AccentColor
        {
            get => accentColor;
            set => SetProperty(ref accentColor, value);
        }

        [JsonIgnore] public Color PrimaryLow => new Color { A = 0x33, R = PrimaryColor.R, G = PrimaryColor.G, B = PrimaryColor.B };
        [JsonIgnore] public Color PrimaryMediumLow => new Color { A = 0x66, R = PrimaryColor.R, G = PrimaryColor.G, B = PrimaryColor.B };
        [JsonIgnore] public Color PrimaryMedium => new Color { A = 0x99, R = PrimaryColor.R, G = PrimaryColor.G, B = PrimaryColor.B };
        [JsonIgnore] public Color PrimaryMediumHigh => new Color { A = 0xCC, R = PrimaryColor.R, G = PrimaryColor.G, B = PrimaryColor.B };


        [JsonIgnore] public Color SecondaryLow => new Color { A = 0x33, R = SecondaryColor.R, G = SecondaryColor.G, B = SecondaryColor.B };
        [JsonIgnore] public Color SecondaryMediumLow => new Color { A = 0x66, R = SecondaryColor.R, G = SecondaryColor.G, B = SecondaryColor.B };
        [JsonIgnore] public Color SecondaryMedium => new Color { A = 0x99, R = SecondaryColor.R, G = SecondaryColor.G, B = SecondaryColor.B };
        [JsonIgnore] public Color SecondaryMediumHigh => new Color { A = 0xCC, R = SecondaryColor.R, G = SecondaryColor.G, B = SecondaryColor.B };

        [JsonIgnore] public SolidColorBrush PrimaryLowSolidColorBrush => new SolidColorBrush(PrimaryLow);
        [JsonIgnore] public SolidColorBrush PrimaryMediumLowSolidColorBrush => new SolidColorBrush(PrimaryMediumLow);
        [JsonIgnore] public SolidColorBrush PrimaryMediumSolidColorBrush => new SolidColorBrush(PrimaryMedium);
        [JsonIgnore] public SolidColorBrush PrimaryMediumHighSolidColorBrush => new SolidColorBrush(PrimaryMediumHigh);


        [JsonIgnore] public SolidColorBrush SecondaryLowSolidColorBrush => new SolidColorBrush(SecondaryLow);
        [JsonIgnore] public SolidColorBrush SecondaryMediumLowSolidColorBrush => new SolidColorBrush(SecondaryMediumLow);
        [JsonIgnore] public SolidColorBrush SecondaryMediumSolidColorBrush => new SolidColorBrush(SecondaryMedium);
        [JsonIgnore] public SolidColorBrush SecondaryMediumHighSolidColorBrush => new SolidColorBrush(SecondaryMediumHigh);


        private void RaiseChanges()
        {
            this.OnPropertyChanged("PrimaryLow");
            this.OnPropertyChanged("PrimaryMediumLow");
            this.OnPropertyChanged("PrimaryMedium");
            this.OnPropertyChanged("PrimaryMediumHigh");

            this.OnPropertyChanged("SecondaryLow");
            this.OnPropertyChanged("SecondaryMediumLow");
            this.OnPropertyChanged("SecondaryMedium");
            this.OnPropertyChanged("SecondaryMediumHigh");

            this.OnPropertyChanged("PrimaryLowSolidColorBrush");
            this.OnPropertyChanged("PrimaryMediumLowSolidColorBrush");
            this.OnPropertyChanged("PrimaryMediumSolidColorBrush");
            this.OnPropertyChanged("PrimaryMediumHighSolidColorBrush");

            this.OnPropertyChanged("SecondaryLowSolidColorBrush");
            this.OnPropertyChanged("SecondaryMediumLowSolidColorBrush");
            this.OnPropertyChanged("SecondaryMediumSolidColorBrush");
            this.OnPropertyChanged("SecondaryMediumHighSolidColorBrush");
        }
        #region Events

        private ThemeWatcher.WindowsTheme currentTheme;

        [JsonIgnore]
        public ThemeWatcher.WindowsTheme CurrentTheme
        {
            get => currentTheme;
            set => SetProperty(ref currentTheme, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the property already matches the desirec value or needs to be updated.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to the backing-filed.</param>
        /// <param name="value">Value to apply.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool RequiresUpdate<T>(ref T storage, T value)
        {
            return !Equals(storage, value);
        }

        /// <summary>
        /// Checks if the property already matches the desired value and updates it if not.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to the backing-filed.</param>
        /// <param name="value">Value to apply.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This value is optional 
        /// and can be provided automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute"/>.</param>
        /// <returns><c>true</c> if the value was changed, <c>false</c> if the existing value matched the desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!this.RequiresUpdate(ref storage, value)) return false;

            storage = value;
            // ReSharper disable once ExplicitCallerInfoArgument
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/>-event when a a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This value is optional 
        /// and can be provided automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Represents a basic bindable class which notifies when a property value changes.
    /// </summary>
    public interface IBindable : INotifyPropertyChanged
    {
    }
}
