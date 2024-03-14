using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using System.Windows.Media;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FilePanelPlaceholderViewModel : CommonViewModelBase, IStoredConnectionViewModel
    {
        public string Name { get; set; }

        public ImageSource Thumbnail { get; private set; }

        public FilePanelPlaceholderViewModel()
        {
            var thumbnail = ResourceManager.GetContentByteArray("/Resources/Connections/HardDrive.png");
            Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(thumbnail);
            Name = Resx.ItemTypeDrive;
        }
    }
}
