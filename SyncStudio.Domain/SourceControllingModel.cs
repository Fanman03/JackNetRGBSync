using SimpleLed;

namespace SyncStudio.Domain
{
    public class SourceControllingModel 
    {
        public string Name { get; set; }
        public string ProviderName { get; set; }

        public string ConnectedTo { get; set; }

        public bool IsCurrent { get; set; }
    }
}