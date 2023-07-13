using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;

namespace DropBoxManager
{
    public class AboutWindowVM : INotifyPropertyChanged
    {
        #region Commands

        private RelayCommand<RequestNavigateEventArgs> requestNavigateCommand;
        public RelayCommand<RequestNavigateEventArgs> RequestNavigateCommand
        {
            get
            {
                return requestNavigateCommand ?? (requestNavigateCommand = new RelayCommand<RequestNavigateEventArgs>(OnRequestNavigate, (o) => { return true; }));
            }
        }

        #endregion

        #region Event handlers

        private void OnRequestNavigate(RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
