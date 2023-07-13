using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DropBoxManager
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        #region Properties

        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                NotifyPropertyChanged();
            }
        }

        private string directoryPath;
        public string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                directoryPath = value;
                NotifyPropertyChanged();
            }
        }

        private FileInfo selectedFile;
        public FileInfo SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                NotifyPropertyChanged();
            }
        }

        private string accessToken = "";

        public ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo>();

        #endregion

        #region Commands

        private RelayCommand<object> createFolderButtonCommand;
        public RelayCommand<object> CreateFolderButtonCommand
        {
            get
            {
                return createFolderButtonCommand ?? (createFolderButtonCommand = new RelayCommand<object>(OnCreateFolderButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<object> downloadButtonCommand;
        public RelayCommand<object> DownloadButtonCommand
        {
            get
            {
                return downloadButtonCommand ?? (downloadButtonCommand = new RelayCommand<object>(OnDownloadButtonClick, (o) => { return SelectedFile != null; }));
            }
        }

        private RelayCommand<object> uploadButtonCommand;
        public RelayCommand<object> UploadButtonCommand
        {
            get
            {
                return uploadButtonCommand ?? (uploadButtonCommand = new RelayCommand<object>(OnUploadButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<object> renameButtonCommand;
        public RelayCommand<object> RenameButtonCommand
        {
            get
            {
                return renameButtonCommand ?? (renameButtonCommand = new RelayCommand<object>(OnRenameButtonClick, (o) => { return SelectedFile != null; }));
            }
        }

        private RelayCommand<object> removeButtonCommand;
        public RelayCommand<object> RemoveButtonCommand
        {
            get
            {
                return removeButtonCommand ?? (removeButtonCommand = new RelayCommand<object>(OnRemoveButtonClick, (o) => { return SelectedFile != null; }));
            }
        }

        private RelayCommand<object> refreshButtonCommand;
        public RelayCommand<object> RefreshButtonCommand
        {
            get
            {
                return refreshButtonCommand ?? (refreshButtonCommand = new RelayCommand<object>(OnRefreshButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<object> backButtonCommand;
        public RelayCommand<object> BackButtonCommand
        {
            get
            {
                return backButtonCommand ?? (backButtonCommand = new RelayCommand<object>(OnBackButtonClick, (o) => { return !String.IsNullOrEmpty(DirectoryPath); }));
            }
        }

        private RelayCommand<object> exitButtonCommand;
        public RelayCommand<object> ExitButtonCommand
        {
            get
            {
                return exitButtonCommand ?? (exitButtonCommand = new RelayCommand<object>(OnExitButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<object> loginButtonCommand;
        public RelayCommand<object> LoginButtonCommand
        {
            get
            {
                return loginButtonCommand ?? (loginButtonCommand = new RelayCommand<object>(OnLoginButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<object> openAboutWindowButtonCommand;
        public RelayCommand<object> OpenAboutWindowButtonCommand
        {
            get
            {
                return openAboutWindowButtonCommand ?? (openAboutWindowButtonCommand = new RelayCommand<object>(OnOpenAboutWindowButtonClick, (o) => { return true; }));
            }
        }

        private RelayCommand<RowDoubleClickEventArgs> rowDoubleClickCommand;
        public RelayCommand<RowDoubleClickEventArgs> RowDoubleClickCommand
        {
            get
            {
                return rowDoubleClickCommand ?? (rowDoubleClickCommand = new RelayCommand<RowDoubleClickEventArgs>(OnRowDoubleClick, (o) => { return true; }));
            }
        }

        #endregion

        public MainWindowVM()
        {
            DirectoryPath = string.Empty;
            if (!String.IsNullOrEmpty(Properties.Settings.Default.AccessToken)) accessToken = Properties.Settings.Default.AccessToken;
            if (!String.IsNullOrEmpty(accessToken)) Login();
            else LoginButtonCommand.Execute(this);
        }

        public async void GetFiles()
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    var list = await client.Files.ListFolderAsync(DirectoryPath);

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Files.Clear();
                        foreach (var item in list.Entries.Where(i => i.IsFolder)) Files.Add(new FileInfo(item.Name));
                        foreach (var item in list.Entries.Where(i => i.IsFile)) Files.Add(new FileInfo(item.Name, Math.Round((double)item.AsFile.Size / (1024 * 1024), 4), item.AsFile.ClientModified));
                    }));
                }
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось обновить список файлов. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void CreateFolder(string name)
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    var response = await client.Files.CreateFolderV2Async(DirectoryPath + "/" + name);
                    GetFiles();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не создать новую папку. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void DownloadFile(string serverFilePath, string filePath)
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    if (SelectedFile.Type == "Папка")
                    {
                        using (var response = await client.Files.DownloadZipAsync(serverFilePath))
                        {
                            byte[] content = await response.GetContentAsByteArrayAsync();
                            File.WriteAllBytes(filePath, content);
                        }
                    }
                    else
                    {
                        using (var response = await client.Files.DownloadAsync(serverFilePath))
                        {
                            byte[] content = await response.GetContentAsByteArrayAsync();
                            File.WriteAllBytes(filePath, content);
                        }
                    }
                    ThemedMessageBox.Show("Скачивание", "Файл \"" + Path.GetFileName(filePath) + "\" успешно сохранен в директории " + Path.GetDirectoryName(filePath) + ".", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не удалось скачать файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void UploadFile(byte[] content, string fileName)
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    using (var memoryStream = new MemoryStream(content))
                    {
                        string filePath = DirectoryPath.Replace("/", "\\");
                        filePath = Path.Combine(filePath, fileName);
                        filePath = filePath.Replace("\\", "/");
                        if (!filePath.Contains('/')) filePath = "/" + filePath;

                        var updated = await client.Files.UploadAsync(filePath, WriteMode.Overwrite.Instance, body: memoryStream);
                        GetFiles();
                        ThemedMessageBox.Show("Загрузка", "Файл \"" + fileName + "\" успешно загружен в папку " + Path.GetDirectoryName(filePath) + ".", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не удалось загрузить файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void RenameFile(string oldPath, string newPath)
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    var response = await client.Files.MoveV2Async(oldPath, newPath);
                    GetFiles();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не удалось переименовать файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void RemoveFile(string serverFilePath)
        {
            try
            {
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    var response = await client.Files.DeleteV2Async(serverFilePath);
                    GetFiles();

                    if (response.Metadata.IsFile) ThemedMessageBox.Show("Удаление", "Файл \"" + Path.GetFileName(serverFilePath) + "\" успешно удален.", MessageBoxButton.OK, MessageBoxImage.Information);
                    else if (response.Metadata.IsFolder) ThemedMessageBox.Show("Удаление", "Папка \"" + Path.GetFileName(serverFilePath) + "\" успешно удалена.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не удалось удалить файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void Login()
        {
            try
            {
                Properties.Settings.Default.AccessToken = accessToken;
                Properties.Settings.Default.Save();
                using (DropboxClient client = new DropboxClient(accessToken))
                {
                    var response = await client.Users.GetCurrentAccountAsync();
                    UserName = response.Name.DisplayName;
                }
            }
            catch(Exception ex)
            {
                if (ex.Message.Contains("Invalid authorization"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Введите токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else if (ex.Message.Contains("expired_access_token"))
                {
                    MessageBoxResult result = ThemedMessageBox.Show("Информация", "Срок действия Вашего токена доступа истек. Вы хотите ввести новый токен доступа?", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes) LoginButtonCommand.Execute(this);
                }
                else if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event handlers

        private void OnCreateFolderButtonClick(object var)
        {
            try
            {
                InputDialogWindow dialog = new InputDialogWindow();
                InputDialogWindowVM viewModel = dialog.DataContext as InputDialogWindowVM;
                viewModel.Title = "Новая папка";
                viewModel.Text = "Введите имя новой папки:";
                viewModel.IsFile = false;

                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true) CreateFolder(viewModel.Input);
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось создать новую папку. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDownloadButtonClick(object var)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.InitialDirectory = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
                if (SelectedFile.Type == "Папка")
                {
                    dialog.FileName = SelectedFile.Name + ".zip";
                    dialog.DefaultExt = ".zip";
                    dialog.Filter = "Архивы (*.zip)|*.zip";
                }
                else
                {
                    dialog.FileName = SelectedFile.Name;
                    string extension = Path.GetExtension(SelectedFile.Name);
                    dialog.DefaultExt = extension;
                    dialog.Filter = "Файлы " + extension.Substring(1).ToUpper() + " (*" + extension + ")|*" + extension;
                }

                Nullable<bool> result = dialog.ShowDialog();
                if (result == true) DownloadFile(DirectoryPath + "/" + SelectedFile.Name, dialog.FileName);
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось скачать файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnUploadButtonClick(object var)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                Nullable<bool> result = dialog.ShowDialog();
                if (result == true) UploadFile(File.ReadAllBytes(dialog.FileName), Path.GetFileName(dialog.FileName));
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось загрузить файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRenameButtonClick(object var)
        {
            try
            {
                InputDialogWindow dialog = new InputDialogWindow();
                InputDialogWindowVM viewModel = dialog.DataContext as InputDialogWindowVM;
                viewModel.Title = "Переименовать";
                viewModel.Input = SelectedFile.Name;
                if (SelectedFile.Type == "Папка")
                {
                    viewModel.Text = "Введите новое имя папки:";
                    viewModel.IsFile = false;
                }
                else
                {
                    viewModel.Text = "Введите новое имя файла:";
                    viewModel.IsFile = true;
                }

                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true) RenameFile(DirectoryPath + "/" + SelectedFile.Name, DirectoryPath + "/" + viewModel.Input);
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось переименовать файл. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRemoveButtonClick(object var)
        {
            MessageBoxResult result = SelectedFile.Type == "Папка" ? ThemedMessageBox.Show("Удаление папки", "Вы уверены, что хотите удалить папку \"" + SelectedFile.Name + "\"?", MessageBoxButton.YesNo, MessageBoxImage.Question) : ThemedMessageBox.Show("Удаление файла", "Вы уверены, что хотите удалить файл \"" + SelectedFile.Name + "\"?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) RemoveFile(DirectoryPath + "/" + SelectedFile.Name);
        }

        private void OnRefreshButtonClick(object var)
        {
            GetFiles();
        }

        private void OnBackButtonClick(object var)
        {
            try
            {
                DirectoryPath = DirectoryPath.Replace("/", "\\");
                DirectoryPath = Path.GetDirectoryName(DirectoryPath);
                DirectoryPath = DirectoryPath.Replace("\\", "/");
                if (DirectoryPath.Count(x => x == '/') == 1 && DirectoryPath.Length == 1) DirectoryPath = string.Empty;
                GetFiles();
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", "Не удалось перейти в предыдущий каталог. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoginButtonClick(object var)
        {
            try
            {
                InputDialogWindow dialog = new InputDialogWindow();
                InputDialogWindowVM viewModel = dialog.DataContext as InputDialogWindowVM;
                viewModel.Title = "Авторизация";
                viewModel.Input = accessToken;
                viewModel.Text = "Введите Ваш сгенерированный токен доступа из консоли приложений.";
                viewModel.NoValidation = true;

                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    accessToken = viewModel.Input;
                    Login();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("malformed"))
                {
                    ThemedMessageBox.Show("Ошибка", "Не удалось авторизоваться. Неверный токен доступа.", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoginButtonCommand.Execute(this);
                }
                else ThemedMessageBox.Show("Ошибка", "Не удалось войти в аккаунт по указанному токену доступа. " + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenAboutWindowButtonClick(object var)
        {
            AboutWindow window = new AboutWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void OnExitButtonClick(object var)
        {
            Application.Current.Shutdown();
        }

        private void OnRowDoubleClick(RowDoubleClickEventArgs e)
        {
            try
            {
                if (SelectedFile != null && SelectedFile.Type == "Папка")
                {
                    DirectoryPath = DirectoryPath.Replace("/", "\\");
                    DirectoryPath = Path.Combine(DirectoryPath, SelectedFile.Name);
                    DirectoryPath = DirectoryPath.Replace("\\", "/");
                    if (!DirectoryPath.Contains("/")) DirectoryPath = "/" + DirectoryPath;
                    GetFiles();
                }
            }
            catch (Exception ex)
            {
                ThemedMessageBox.Show("Ошибка", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
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
