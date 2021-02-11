using SourceChord.FluentWPF.Utility;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace SourceChord.FluentWPF
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SourceChord.FluentWPF"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SourceChord.FluentWPF;assembly=SourceChord.FluentWPF"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:AcrylicPopup/>
    ///
    /// </summary>
    public class AcrylicPopup : Popup
    {
        static AcrylicPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicPopup), new FrameworkPropertyMetadata(typeof(AcrylicPopup)));
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            HwndSource hwnd = (HwndSource)HwndSource.FromVisual(this.Child);
            AcrylicHelper.EnableBlur(hwnd.Handle, AccentFlagsType.Popup);
        }
    }
}
