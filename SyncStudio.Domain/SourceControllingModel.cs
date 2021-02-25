using SimpleLed;

namespace SyncStudio.Domain
{
    public class SourceControllingModel : BaseViewModel
    {
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }


        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set => SetProperty(ref providerName, value);
        }

        private string connectedTo;

        public string ConnectedTo
        {
            get => connectedTo;
            set => SetProperty(ref connectedTo, value);
        }

        private bool isCurrent;

        public bool IsCurrent
        {
            get => isCurrent;
            set => SetProperty(ref isCurrent, value);
        }
    }
}