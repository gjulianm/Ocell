using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DanielVaughan.ComponentModel;
using DanielVaughan;
using DanielVaughan.Windows;
using Ocell.Library;
using System.Collections.Generic;

namespace Ocell.Pages.Settings
{
    public class BackgroundsModel : ExtendedViewModelBase
    {
        OcellTheme theme;

        List<string> backgroundNames;
        public IEnumerable<string> BackgroundNames
        {
            get { return backgroundNames; }
        }

        Brush backgroundBrush;
        public Brush BackgroundBrush
        {
            get { return backgroundBrush; }
            set { Assign("BackgroundBrush", ref backgroundBrush, value); }
        }

        int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { Assign("SelectedIndex", ref selectedIndex, value); }
        }

        DelegateCommand saveBackground;
        public ICommand SaveBackground
        {
            get { return saveBackground; }
        }

        public BackgroundsModel()
            : base("Backgrounds")
        {
            theme = new OcellTheme();
            backgroundNames = new List<string> { "Default", "None (transparent)", "Fabric", "Egg", "Tiles", "Tire", "Floral", "Map", "Diamond" };

            

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "SelectedIndex")
                {
                    theme.Background = (BackgroundType)selectedIndex;
                    BackgroundBrush = theme.GetBrush();
                }
            };

            saveBackground = new DelegateCommand((param) =>
            {
                Config.Background = theme;
                MessageService.ShowMessage(Localization.Resources.BackgroundChangeOnRestart);
                GoBack();
            });
        }
    }
}
