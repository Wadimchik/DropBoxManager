using System;
using System.Globalization;
using System.Windows.Data;

namespace DropBoxManager
{
    internal class FileTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if ((string)value == "Изображение") return @"\Images\Image_16x16.png";
                if ((string)value == "Текстовый документ") return @"\Images\Text_16x16.png";
                if ((string)value == "Видео") return @"\Images\Video_16x16.png";
                if ((string)value == "Аудио") return @"\Images\Audio_16x16.png";
                if ((string)value == "Приложение") return @"\Images\Exe_16x16.png";
                if ((string)value == "Архив") return @"\Images\Archive_16x16.png";
                if ((string)value == "Папка") return @"\Images\Folder_16x16.png";
                if (((string)value).Contains("Файл")) return @"\Images\File_16x16.png";
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
