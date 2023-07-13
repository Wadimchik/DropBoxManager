using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace DropBoxManager
{
    public class FileInfo : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }

        private double size;
        public double Size
        {
            get { return size; }
            set
            {
                size = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                NotifyPropertyChanged();
            }
        }

        private string type;
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged();
            }
        }

        public FileInfo(string name)
        {
            Name = name;
            Type = "Папка";
        }

        public FileInfo(string name, double size, DateTime date)
        {
            Name = name;
            Size = size;
            Date = date;

            string extension = Path.GetExtension(name);
            if (extension == ".png" || extension == ".jpg" || extension == ".bmp" || extension == ".tiff" || extension == ".psd" || extension == ".raw" || extension == ".gif" || extension == ".svg" || extension == ".pdf") Type = "Изображение";
            else if (extension == ".txt" || extension == ".doc" || extension == ".docx" || extension == ".rtf" || extension == ".odt") Type = "Текстовый документ";
            else if (extension == ".mp4" || extension == ".mov" || extension == ".avi" || extension == ".wmv" || extension == ".mpeg") Type = "Видео";
            else if (extension == ".mp3" || extension == ".ogg" || extension == ".wav" || extension == ".aiff" || extension == ".wma") Type = "Аудио";
            else if (extension == ".rar" || extension == ".zip" || extension == ".7z" || extension == ".iso" || extension == ".tar") Type = "Архив";
            else if (extension == ".exe") Type = "Приложение";
            else Type = "Файл " + extension.Substring(1).ToUpper();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
