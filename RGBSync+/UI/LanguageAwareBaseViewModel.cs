using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace RGBSyncPlus.UI
{
  public   class LanguageAwareBaseViewModel : BaseViewModel
  {
      private bool hasSetEvent;

      public LanguageAwareBaseViewModel()
      {
          try
          {
              ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
              {
                  Language = ApplicationManager.Instance.NGSettings.Lang;
              };
          }
          catch
          {
          }
      }
      private string language;

      public string Language
      {
          get => language;
          set => SetProperty(ref language, value);
      }

    }
}
