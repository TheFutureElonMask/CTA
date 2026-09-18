using System.IO;
using System.Windows;
using CTA.Models;
using CTA.Services;
using Microsoft.Win32;

namespace CTA;

public partial class MainWindow : Window
{
    private List<StudentRecord> _students = new();

    private Dictionary<string, TextElementSettings> _settings = new()
    {
        ["ФИО ученика"] = new TextElementSettings()
        {
            Name = "ФИО ученика",
            X = 800,
            Y = 655,
            FontSize = 40,
            WidthScale = 1.0,
            HeightScale = 1.0
        },
        ["Название конкурса"] = new TextElementSettings()
        {
            Name = "Название конкурса",
            X = 800,
            Y = 516,
            FontSize = 45,
            WidthScale = 1.0,
            HeightScale = 1.0
        },
        ["ФИО преподавателя"] = new TextElementSettings()
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
            X = 800,
            Y = 620,
            FontSize = 30,
            WidthScale = 1.0,
            HeightScale = 1.0
        }
    };
    
    private void ElementComboBox_SelectionChanged(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ElementComboBox.SelectedItem is not string name)
            return;

        if (!_settings.TryGetValue(name, out var settings))
            return;

        XTextBox.Text = settings.X.ToString();
        YTextBox.Text = settings.Y.ToString();
        FontSizeTextBox.Text = settings.FontSize.ToString();
        WidthScaleTextBox.Text = settings.WidthScale.ToString();
        HeightScaleTextBox.Text = settings.HeightScale.ToString();
    }
    

    public MainWindow()
    {
        InitializeComponent();
        
        ElementComboBox.ItemsSource = _settings.Keys;
        ElementComboBox.SelectedIndex = 0;
    }

    private void SelectExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            ExcelPathTextBox.Text = dialog.FileName;
        }
    }

    private void SelectTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };

        if (dialog.ShowDialog() == true)
        {
            TemplatePathTextBox.Text = dialog.FileName;
        }
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        var excelPath = ExcelPathTextBox.Text;
        var templatePath = TemplatePathTextBox.Text;

        if (!File.Exists(excelPath))
        {
            MessageBox.Show("Выберите Excel-файл.");
            return;
        }

        if (!File.Exists(templatePath))
        {
            MessageBox.Show("Выберите шаблон.");
            return;
        }

        try
        {
            var excelService = new ExcelService();

            _students = excelService.Read(excelPath);

            if (_students.Count == 0)
            {
                MessageBox.Show("В Excel нет данных.");
                return;
            }

            var firstStudent = _students[0];
            var diplomaService = new DiplomaService(templatePath, _settings);

            using var diploma = diplomaService.Generate(firstStudent);

            using var stream = new MemoryStream();

            diploma.Save(
                stream,
                System.Drawing.Imaging.ImageFormat.Png);

            stream.Position = 0;

            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption =
                System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            PreviewImage.Source = bitmapImage;
        }

        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка:\n\n{ex.Message}");
        }
    }

    private void ApplySettings_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ElementComboBox.SelectedItem is not string name)
            return;

        if (!_settings.TryGetValue(name, out var settings))
            return;

        if (double.TryParse(XTextBox.Text, out var x))
            settings.X = x;

        if (double.TryParse(YTextBox.Text, out var y))
            settings.Y = y;

        if (double.TryParse(
                FontSizeTextBox.Text,
                out var fontSize))
            settings.FontSize = fontSize;

        if (double.TryParse(
                WidthScaleTextBox.Text,
                out var widthScale))
            settings.WidthScale = widthScale;

        if (double.TryParse(
                HeightScaleTextBox.Text,
                out var heightScale))
            settings.HeightScale = heightScale;

        UpdatePreview();
    }
    private void UpdatePreview()
    {
        if (_students.Count == 0)
            return;

        var templatePath = TemplatePathTextBox.Text;

        if (!File.Exists(templatePath))
            return;

        var diplomaService =
            new DiplomaService(
                templatePath,
                _settings);

        using var diploma =
            diplomaService.Generate(_students[0]);

        using var stream =
            new MemoryStream();

        diploma.Save(
            stream,
            System.Drawing.Imaging.ImageFormat.Png);

        stream.Position = 0;

        var bitmapImage =
            new System.Windows.Media.Imaging.BitmapImage();

        bitmapImage.BeginInit();

        bitmapImage.CacheOption =
            System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;

        bitmapImage.StreamSource = stream;

        bitmapImage.EndInit();

        bitmapImage.Freeze();

        PreviewImage.Source = bitmapImage;
    }
}