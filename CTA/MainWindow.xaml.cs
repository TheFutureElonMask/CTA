using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CTA.Models;
using CTA.Services;
using Microsoft.Win32;

namespace CTA;

public partial class MainWindow : Window
{
    // =========================================================
    // ДАННЫЕ
    // =========================================================

    private List<StudentRecord> _students = new();

    private readonly Dictionary<string, TextElementSettings> _settings =
        new()
        {
            ["ФИО ученика"] = new TextElementSettings
            {
                Name = "ФИО ученика",
                X = 600,
                Y = 655,
                FontSize = 40,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["Название конкурса"] = new TextElementSettings
            {
                Name = "Название конкурса",
                X = 500,
                Y = 516,
                FontSize = 45,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["ФИО преподавателя"] = new TextElementSettings
            {
                Name = "ФИО преподавателя",
                X = 600,
                Y = 735,
                FontSize = 35,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["Регистрационный номер"] = new TextElementSettings
            {
                Name = "Регистрационный номер",
                X = 150,
                Y = 990,
                FontSize = 20,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["Учебный год"] = new TextElementSettings
            {
                Name = "Учебный год",
                X = 1100,
                Y = 990,
                FontSize = 20,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["Место"] = new TextElementSettings
            {
                Name = "Место",
                X = 680,
                Y = 315,
                FontSize = 60,
                WidthScale = 1.0,
                HeightScale = 1.0
            },

            ["Награда"] = new TextElementSettings
            {
                Name = "Награда",
                X = 700,
                Y = 620,
                FontSize = 30,
                WidthScale = 1.0,
                HeightScale = 1.0
            }
        };


    // =========================================================
    // ВЫБРАННЫЙ ЭЛЕМЕНТ
    // =========================================================

    private string? _selectedElement;


    // =========================================================
    // ПЕРЕТАСКИВАНИЕ
    // =========================================================

    private bool _isDragging;

    private Point _lastMousePosition;


    // =========================================================
    // КОНСТРУКТОР
    // =========================================================

    public MainWindow()
    {
        InitializeComponent();

        ElementComboBox.ItemsSource =
            _settings.Keys;

        ElementComboBox.SelectedIndex = 0;
    }
    
    private void GenerateAll_Click(
    object sender,
    RoutedEventArgs e)
{
    if (_students.Count == 0)
    {
        MessageBox.Show(
            "Сначала загрузите Excel и нажмите «Показать предпросмотр».");

        return;
    }

    var templatePath =
        TemplatePathTextBox.Text;

    if (!File.Exists(templatePath))
    {
        MessageBox.Show(
            "Выберите шаблон.");

        return;
    }

    try
    {
        var baseFolder =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Готовые_дипломы");

        Directory.CreateDirectory(
            baseFolder);

        var diplomaService =
            new DiplomaService(
                templatePath,
                _settings);

        var generatedCount = 0;

        foreach (var student in _students)
        {
            var schoolName =
                string.IsNullOrWhiteSpace(
                    student.SchoolName)
                    ? "Без_школы"
                    : SanitizeFileName(
                        student.SchoolName);

            var schoolFolder =
                Path.Combine(
                    baseFolder,
                    schoolName);

            Directory.CreateDirectory(
                schoolFolder);

            using var diploma =
                diplomaService.Generate(
                    student);

            var studentName =
                SanitizeFileName(
                    student.FullName);

            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName =
                    $"Ученик_{generatedCount + 1}";
            }

            var filePath =
                Path.Combine(
                    schoolFolder,
                    $"{studentName}.png");

            diploma.Save(
                filePath,
                System.Drawing.Imaging.ImageFormat.Png);

            generatedCount++;
        }

        MessageBox.Show(
            $"Готово!\n\n" +
            $"Создано дипломов: {generatedCount}\n\n" +
            $"Папка:\n{baseFolder}");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Ошибка при генерации:\n\n{ex.Message}");
    }
}


    // =========================================================
    // EXCEL
    // =========================================================

    private void SelectExcel_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            ExcelPathTextBox.Text =
                dialog.FileName;
        }
    }
    
    private static string SanitizeFileName(
        string name)
    {
        foreach (var invalidChar
                 in Path.GetInvalidFileNameChars())
        {
            name =
                name.Replace(
                    invalidChar.ToString(),
                    "_");
        }

        return name
            .Trim()
            .Replace(" ", "_");
    }


    // =========================================================
    // ШАБЛОН
    // =========================================================

    private void SelectTemplate_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter =
                "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };

        if (dialog.ShowDialog() == true)
        {
            TemplatePathTextBox.Text =
                dialog.FileName;
        }
    }


    // =========================================================
    // СОЗДАНИЕ ПРЕДПРОСМОТРА
    // =========================================================

    private void Generate_Click(
        object sender,
        RoutedEventArgs e)
    {
        var excelPath =
            ExcelPathTextBox.Text;

        var templatePath =
            TemplatePathTextBox.Text;


        if (!File.Exists(excelPath))
        {
            MessageBox.Show(
                "Выберите Excel-файл.");

            return;
        }


        if (!File.Exists(templatePath))
        {
            MessageBox.Show(
                "Выберите шаблон.");

            return;
        }


        try
        {
            var excelService =
                new ExcelService();

            _students =
                excelService.Read(excelPath);


            if (_students.Count == 0)
            {
                MessageBox.Show(
                    "В Excel нет данных.");

                return;
            }


            UpdatePreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка:\n\n{ex.Message}");
        }
    }


    // =========================================================
    // ОБНОВЛЕНИЕ ПРЕДПРОСМОТРА
    // =========================================================

    private void UpdatePreview()
    {
        if (_students.Count == 0)
            return;


        var templatePath =
            TemplatePathTextBox.Text;


        if (!File.Exists(templatePath))
            return;


        var diplomaService =
            new DiplomaService(
                templatePath,
                _settings);


        using var diploma =
            diplomaService.Generate(
                _students[0]);


        using var stream =
            new MemoryStream();


        diploma.Save(
            stream,
            System.Drawing.Imaging.ImageFormat.Png);


        stream.Position = 0;


        var bitmapImage =
            new BitmapImage();


        bitmapImage.BeginInit();

        bitmapImage.CacheOption =
            BitmapCacheOption.OnLoad;

        bitmapImage.StreamSource =
            stream;

        bitmapImage.EndInit();

        bitmapImage.Freeze();


        PreviewImage.Source =
            bitmapImage;


        UpdateSelectionBorder();
    }


    // =========================================================
    // ВЫБОР ЧЕРЕЗ COMBOBOX
    // =========================================================

    private void ElementComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (ElementComboBox.SelectedItem
            is not string name)
        {
            return;
        }


        SelectElement(name);
    }


    // =========================================================
    // ВЫБРАТЬ ЭЛЕМЕНТ
    // =========================================================

    private void SelectElement(string name)
    {
        if (!_settings.TryGetValue(
                name,
                out var settings))
        {
            return;
        }


        _selectedElement = name;


        // Синхронизируем ComboBox
        if (ElementComboBox.SelectedItem as string != name)
        {
            ElementComboBox.SelectedItem = name;
        }


        // Заполняем правую панель
        XTextBox.Text =
            settings.X.ToString("0.##");

        YTextBox.Text =
            settings.Y.ToString("0.##");

        FontSizeTextBox.Text =
            settings.FontSize.ToString("0.##");

        WidthScaleTextBox.Text =
            settings.WidthScale.ToString("0.##");

        HeightScaleTextBox.Text =
            settings.HeightScale.ToString("0.##");


        UpdateSelectionBorder();
    }


    // =========================================================
    // ПРИМЕНЕНИЕ НАСТРОЕК
    // =========================================================

    private void ApplySettings_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_selectedElement == null)
            return;


        if (!_settings.TryGetValue(
                _selectedElement,
                out var settings))
        {
            return;
        }


        if (double.TryParse(
                XTextBox.Text,
                out var x))
        {
            settings.X = x;
        }


        if (double.TryParse(
                YTextBox.Text,
                out var y))
        {
            settings.Y = y;
        }


        if (double.TryParse(
                FontSizeTextBox.Text,
                out var fontSize))
        {
            settings.FontSize = fontSize;
        }


        if (double.TryParse(
                WidthScaleTextBox.Text,
                out var widthScale))
        {
            settings.WidthScale =
                widthScale;
        }


        if (double.TryParse(
                HeightScaleTextBox.Text,
                out var heightScale))
        {
            settings.HeightScale =
                heightScale;
        }


        UpdatePreview();
    }


    // =========================================================
    // КЛИК ПО ПРЕДПРОСМОТРУ
    // =========================================================

    private void PreviewImage_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_students.Count == 0)
            return;


        var clickPosition =
            e.GetPosition(
                PreviewImage);


        var imagePoint =
            DisplayToImageCoordinates(
                clickPosition);


        // Ищем элемент, по которому кликнули
        var elementName =
            FindElementAtPoint(
                imagePoint);


        if (elementName == null)
        {
            // Кликнули по пустому месту
            _selectedElement = null;

            SelectionBorder.Visibility =
                Visibility.Collapsed;

            e.Handled = true;

            return;
        }


        // Автоматически выбираем элемент
        SelectElement(elementName);


        // Начинаем перемещение
        _isDragging = true;


        _lastMousePosition =
            clickPosition;


        PreviewImage.CaptureMouse();


        e.Handled = true;
    }


    // =========================================================
    // ДВИЖЕНИЕ МЫШИ
    // =========================================================

    private void PreviewImage_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!_isDragging)
            return;


        if (_selectedElement == null)
            return;


        if (!_settings.TryGetValue(
                _selectedElement,
                out var settings))
        {
            return;
        }


        var currentPosition =
            e.GetPosition(
                PreviewImage);


        var deltaX =
            currentPosition.X -
            _lastMousePosition.X;


        var deltaY =
            currentPosition.Y -
            _lastMousePosition.Y;


        var scale =
            GetDisplayScale();


        if (scale <= 0)
            return;


        // Переводим движение мыши
        // из экранных пикселей
        // в координаты изображения
        settings.X +=
            deltaX / scale;


        settings.Y +=
            deltaY / scale;


        _lastMousePosition =
            currentPosition;


        // Обновляем поля
        XTextBox.Text =
            settings.X.ToString("0.##");

        YTextBox.Text =
            settings.Y.ToString("0.##");


        UpdatePreview();
    }


    // =========================================================
    // ОТПУСКАНИЕ МЫШИ
    // =========================================================

    private void PreviewImage_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        _isDragging = false;

        PreviewImage.ReleaseMouseCapture();

        e.Handled = true;
    }


    // =========================================================
    // ПОИСК ЭЛЕМЕНТА ПОД КУРСОРОМ
    // =========================================================

    private string? FindElementAtPoint(
        Point imagePoint)
    {
        if (_students.Count == 0)
            return null;


        var student =
            _students[0];


        // Проверяем элементы в обратном порядке,
        // чтобы последний элемент имел приоритет,
        // если элементы пересекаются.
        var elementNames =
            _settings.Keys.Reverse();


        foreach (var name in elementNames)
        {
            if (!_settings.TryGetValue(
                    name,
                    out var settings))
            {
                continue;
            }


            var text =
                GetElementText(
                    name,
                    student);


            if (string.IsNullOrWhiteSpace(text))
                continue;


            var size =
                DiplomaService.MeasureText(
                    text,
                    (float)settings.FontSize,
                    (float)settings.WidthScale,
                    (float)settings.HeightScale);


            var bounds =
                GetElementBounds(
                    name,
                    settings,
                    size);


            if (bounds.Contains(imagePoint))
            {
                return name;
            }
        }


        return null;
    }


    // =========================================================
    // ГРАНИЦЫ ЭЛЕМЕНТА
    // =========================================================

    private Rect GetElementBounds(
        string name,
        TextElementSettings settings,
        System.Drawing.SizeF size)
    {
        var x = settings.X;


        // Награда пока остаётся
        // центрированной относительно X = 800.
        if (name == "Награда" &&
            Math.Abs(settings.X - 800) < 0.01)
        {
            x =
                (1600 - size.Width) / 2;
        }


        return new Rect(
            x,
            settings.Y,
            size.Width,
            size.Height);
    }


    // =========================================================
    // ПОЛУЧИТЬ ТЕКСТ
    // =========================================================

    private string GetElementText(
        string elementName,
        StudentRecord student)
    {
        return elementName switch
        {
            "ФИО ученика" =>
                student.FullName,

            "Название конкурса" =>
                student.Contest,

            "ФИО преподавателя" =>
                student.Teacher,

            "Регистрационный номер" =>
                $"№-{student.RegistrationNumber.PadLeft(7, '0')}",

            "Учебный год" =>
                "2026-2027 оқу жылы",

            "Место" =>
                student.Place switch
                {
                    "1" => "I",
                    "2" => "II",
                    _ => "III"
                },

            "Награда" =>
                student.Place == "1"
                    ? "ЖЕҢІМПАЗЫ"
                    : "ЖҮЛДЕГЕРІ",

            _ => ""
        };
    }


    // =========================================================
    // РАМКА ВЫДЕЛЕНИЯ
    // =========================================================

    private void UpdateSelectionBorder()
    {
        if (_selectedElement == null)
        {
            SelectionBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        if (_students.Count == 0)
        {
            SelectionBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        if (!_settings.TryGetValue(
                _selectedElement,
                out var settings))
        {
            SelectionBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        var text =
            GetElementText(
                _selectedElement,
                _students[0]);

        if (string.IsNullOrWhiteSpace(text))
        {
            SelectionBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        var size =
            DiplomaService.MeasureText(
                text,
                (float)settings.FontSize,
                (float)settings.WidthScale,
                (float)settings.HeightScale);

        var bounds =
            GetElementBounds(
                _selectedElement,
                settings,
                size);

        var scale =
            GetDisplayScale();

        if (scale <= 0)
        {
            SelectionBorder.Visibility =
                Visibility.Collapsed;

            return;
        }

        Canvas.SetLeft(
            SelectionBorder,
            bounds.X * scale);

        Canvas.SetTop(
            SelectionBorder,
            bounds.Y * scale);

        SelectionBorder.Width =
            bounds.Width * scale;

        SelectionBorder.Height =
            bounds.Height * scale;

        SelectionBorder.Visibility =
            Visibility.Visible;
    }


    // =========================================================
    // МАСШТАБ ИЗОБРАЖЕНИЯ НА ЭКРАНЕ
    // =========================================================

    private double GetDisplayScale()
    {
        if (PreviewContainer.ActualWidth <= 0 ||
            PreviewContainer.ActualHeight <= 0)
        {
            return 0;
        }

        return PreviewContainer.ActualWidth / 1600.0;
    }


    // =========================================================
    // ОТСТУП ИЗ-ЗА STRETCH="UNIFORM"
    // =========================================================
    
    
    // =========================================================
    // ЭКРАННЫЕ КООРДИНАТЫ → КООРДИНАТЫ ДИПЛОМА
    // =========================================================

    private Point DisplayToImageCoordinates(
        Point displayPoint)
    {
        var scale = GetDisplayScale();

        if (scale <= 0)
            return new Point();

        return new Point(
            displayPoint.X / scale,
            displayPoint.Y / scale);
    }
}