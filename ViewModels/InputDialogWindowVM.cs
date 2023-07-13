using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DevExpress.Xpf.Editors;

namespace DropBoxManager
{
    public class InputDialogWindowVM : INotifyPropertyChanged
    {
        #region Properties

        public bool NoValidation { get; set; }
        public bool IsFile { get; set; }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                NotifyPropertyChanged();
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                NotifyPropertyChanged();
            }
        }

        private string input;
        public string Input
        {
            get { return input; }
            set
            {
                input = value;
                NotifyPropertyChanged();
            }
        }

        private bool inputCorrect;
        public bool InputCorrect
        {
            get { return inputCorrect; }
            set
            {
                inputCorrect = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        private RelayCommand<Window> okButtonCommand;
        public RelayCommand<Window> OkButtonCommand
        {
            get
            {
                return okButtonCommand ?? (okButtonCommand = new RelayCommand<Window>(OkButtonClicked, (o) => { return InputCorrect; }));
            }
        }

        private RelayCommand<Window> cancelButtonCommand;
        public RelayCommand<Window> CancelButtonCommand
        {
            get
            {
                return cancelButtonCommand ?? (cancelButtonCommand = new RelayCommand<Window>(CancelButtonClicked, (o) => { return true; }));
            }
        }

        private RelayCommand<ValidationEventArgs> validateInputCommand;
        public RelayCommand<ValidationEventArgs> ValidateInputCommand
        {
            get
            {
                return validateInputCommand ?? (validateInputCommand = new RelayCommand<ValidationEventArgs>(OnValidateInput, (o) => { return true; }));
            }
        }

        #endregion

        public InputDialogWindowVM()
        {
            //if (NoValidation) InputCorrect = true;
        }

        #region Event handlers

        private void OkButtonClicked(Window window)
        {
            if (window != null)
            {
                (window.DataContext as InputDialogWindowVM).DialogResult = true;
                window.Close();
            }
        }

        private void CancelButtonClicked(Window window)
        {
            if (window != null)
            {
                (window.DataContext as InputDialogWindowVM).DialogResult = false;
                window.Close();
            }
        }

        private void OnValidateInput(ValidationEventArgs e)
        {
            if (NoValidation)
            {
                InputCorrect = true;
                return;
            }
            if (!IsFile)
            {
                if (e.Value == null)
                {
                    InputCorrect = false;
                    return;
                }
                else if (string.IsNullOrWhiteSpace(e.Value.ToString()))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя не должно состоять из одних пробелов.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().First() == ' ')
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Первый символ не должен быть пробелом.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().Last() == ' ')
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Последний символ не должен быть пробелом.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().Contains(".") || e.Value.ToString().Contains("/") || e.Value.ToString().Contains("\\"))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя папки не должно содержать запрещенных символов: (\".\", \"/\", \"\\\").";
                    InputCorrect = false;
                }
                else InputCorrect = true;
            }
            else
            {
                if (e.Value == null)
                {
                    InputCorrect = false;
                    return;
                }
                else if (string.IsNullOrWhiteSpace(e.Value.ToString()))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя не должно состоять из одних пробелов.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().First() == ' ')
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Первый символ не должен быть пробелом.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().Last() == ' ')
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Последний символ не должен быть пробелом.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().First() == '.' && e.Value.ToString().Length > 1)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя файла должно иметь имя перед расширением.";
                    InputCorrect = false;
                }
                else if (!e.Value.ToString().Contains(".") || e.Value.ToString().Count(x => x == '.') > 1 || e.Value.ToString().Last() == '.' || (e.Value.ToString().First() != '.' && e.Value.ToString()[e.Value.ToString().IndexOf('.') - 1] == ' ' || e.Value.ToString()[e.Value.ToString().IndexOf('.') + 1] == ' '))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя файла должно иметь расширение.";
                    InputCorrect = false;
                }
                else if (e.Value.ToString().Contains("/") || e.Value.ToString().Contains("\\"))
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Default;
                    e.ErrorContent = "Имя файла не должно содержать запрещенных символов: (\"/\", \"\\\").";
                    InputCorrect = false;
                }
                else InputCorrect = true;
            }
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
