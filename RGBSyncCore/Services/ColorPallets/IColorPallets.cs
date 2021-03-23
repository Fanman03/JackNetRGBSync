using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace SyncStudio.Core.Services.ColorPallets
{
    public interface IColorPallets
    {
        void SaveActiveColorPallet();
        void DeleteColorPallet(string name);
        void SetActiveColorPallet(ColorProfile colorProfile);
        ColorProfile GetActiveColorPallet();
        List<ColorProfile> GetAllColorPallets();
    }
}
