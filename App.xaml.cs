using DevExpress.Xpf.Core;
using System.Windows;

namespace DropBoxManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisDarkName;
            base.OnStartup(e);
        }
    }
}
