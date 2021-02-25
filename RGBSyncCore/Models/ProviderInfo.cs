using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SyncStudio.Core.Models
{
    public class ProviderInfo
    {
        public Guid InstanceId { get; set; }
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public ReleaseNumber Version { get; set; }
        public bool IsPublicRelease { get; set; }
        public string Blurb { get; set; }
        public BitmapImage Image { get; set; }
        public Guid AuthorId { get; set; }
    }
}
