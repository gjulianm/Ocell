using AncoraMVVM.Base;
using Ocell.Library;
using PropertyChanged;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Ocell.Pages.Settings
{
    [ImplementPropertyChanged]
    public class BackgroundsModel : ExtendedViewModelBase
    {
        OcellTheme theme;

        List<string> backgroundNames;
        public IEnumerable<string> BackgroundNames
        {
            get { return backgroundNames; }
        }

        public Brush BackgroundBrush { get; set; }

        public int SelectedIndex { get; set; }

        DelegateCommand saveBackground;
        public ICommand SaveBackground
        {
            get { return saveBackground; }
        }

        public BackgroundsModel()
        {
            theme = new OcellTheme();
            backgroundNames = new List<string> { "Default", "None (transparent)", "Fabric", "Egg", "Tiles", "Tire", "Floral", "Map", "Diamond" };



            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "SelectedIndex")
                {
                    theme.Background = (BackgroundType)SelectedIndex;
                    BackgroundBrush = theme.GetBrush();
                }
            };

            saveBackground = new DelegateCommand((param) =>
            {
                Config.Background = theme;
                Notificator.ShowMessage(Localization.Resources.BackgroundChangeOnRestart);
                GoBack();
            });
        }
    }
}
