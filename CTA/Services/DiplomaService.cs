using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using CTA.Models;

namespace CTA.Services;

public class DiplomaService
{
    private readonly string _templatePath;
    private readonly Dictionary<string, TextElementSettings> _settings;

    public DiplomaService(
        string templatePath,
        Dictionary<string, TextElementSettings> settings)
    {
        _templatePath = templatePath;
        _settings = settings;
    }

    public Bitmap Generate(StudentRecord student)
    {
        var image = new Bitmap(_templatePath);

        using var graphics = Graphics.FromImage(image);

        graphics.TextRenderingHint =
            System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.Black);

        // =========================
        // МЕСТО
        // =========================

        var placeSettings = _settings["Место"];

        using var placeFont = new Font(
            "Times New Roman",
            (float)placeSettings.FontSize);

        var placeText = student.Place switch
        {
            "1" => "I",
            "2" => "II",
            _ => "III"
        };

        DrawText(
            graphics,
            placeText,
            placeFont,
            placeSettings,
            brush);


        // =========================
        // НАГРАДА
        // =========================

        var awardSettings = _settings["Награда"];

        using var awardFont = new Font(
            "Times New Roman",
            (float)awardSettings.FontSize);

        var awardText =
            student.Place == "1"
                ? "ЖЕҢІМПАЗЫ"
                : "ЖҮЛДЕГЕРІ";

        DrawCenteredText(
            graphics,
            awardText,
            awardFont,
            awardSettings,
            image.Width,
            brush);


        // =========================
        // НАЗВАНИЕ КОНКУРСА
        // =========================

        if (!string.IsNullOrWhiteSpace(student.Contest))
        {
            var contestSettings =
                _settings["Название конкурса"];

            using var contestFont = new Font(
                "Times New Roman",
                (float)contestSettings.FontSize);

            DrawText(
                graphics,
                student.Contest,
                contestFont,
                contestSettings,
                brush);
        }


        // =========================
        // ФИО УЧЕНИКА
        // =========================

        var nameSettings =
            _settings["ФИО ученика"];

        using var nameFont = new Font(
            "Times New Roman",
            (float)nameSettings.FontSize);

        DrawText(
            graphics,
            student.FullName,
            nameFont,
            nameSettings,
            brush);


        // =========================
        // ФИО ПРЕПОДАВАТЕЛЯ
        // =========================

        var teacherSettings =
            _settings["ФИО преподавателя"];

        using var teacherFont = new Font(
            "Times New Roman",
            (float)teacherSettings.FontSize);

        DrawText(
            graphics,
            student.Teacher,
            teacherFont,
            teacherSettings,
            brush);


        // =========================
        // РЕГИСТРАЦИОННЫЙ НОМЕР
        // =========================

        var registrationSettings =
            _settings["Регистрационный номер"];

        using var registrationFont = new Font(
            "Times New Roman",
            (float)registrationSettings.FontSize);

        var registrationNumber =
            student.RegistrationNumber.PadLeft(7, '0');

        DrawText(
            graphics,
            $"№-{registrationNumber}",
            registrationFont,
            registrationSettings,
            brush);


        // =========================
        // УЧЕБНЫЙ ГОД
        // =========================

        var yearSettings =
            _settings["Учебный год"];

        using var yearFont = new Font(
            "Times New Roman",
            (float)yearSettings.FontSize);

        DrawText(
            graphics,
            "2026-2027 оқу жылы",
            yearFont,
            yearSettings,
            brush);


        return image;
    }


    // =========================================================
    // ОБЫЧНЫЙ ТЕКСТ
    // =========================================================

    private static void DrawText(
        Graphics graphics,
        string text,
        Font font,
        TextElementSettings settings,
        Brush brush)
    {
        var state = graphics.Save();

        graphics.TranslateTransform(
            (float)settings.X,
            (float)settings.Y);

        graphics.ScaleTransform(
            (float)settings.WidthScale,
            (float)settings.HeightScale);

        graphics.DrawString(
            text,
            font,
            brush,
            0,
            0);

        graphics.Restore(state);
    }


    // =========================================================
    // ЦЕНТРИРОВАННЫЙ ТЕКСТ
    // =========================================================

    private static void DrawCenteredText(
        Graphics graphics,
        string text,
        Font font,
        TextElementSettings settings,
        int imageWidth,
        Brush brush)
    {
        var size = graphics.MeasureString(text, font);

        var scaledWidth =
            size.Width * settings.WidthScale;

        var x =
            settings.X;

        // Если X = 800, считаем его центром.
        // Это позволяет сохранить привычное положение
        // награды в центре диплома.
        if (x == 800)
        {
            x =
                (imageWidth - scaledWidth) / 2;
        }

        var state = graphics.Save();

        graphics.TranslateTransform(
            (float)x,
            (float)settings.Y);

        graphics.ScaleTransform(
            (float)settings.WidthScale,
            (float)settings.HeightScale);

        graphics.DrawString(
            text,
            font,
            brush,
            0,
            0);

        graphics.Restore(state);
    }


    // =========================================================
    // ПОЛУЧИТЬ РАЗМЕР ТЕКСТА
    // =========================================================

    public static SizeF MeasureText(
        string text,
        float fontSize,
        float widthScale,
        float heightScale)
    {
        using var bitmap =
            new Bitmap(1, 1);

        using var graphics =
            Graphics.FromImage(bitmap);

        using var font =
            new Font(
                "Times New Roman",
                fontSize);

        var size =
            graphics.MeasureString(text, font);

        return new SizeF(
            size.Width * widthScale,
            size.Height * heightScale);
    }


    // =========================================================
    // PDF
    // =========================================================

    public static void SavePdf(
        Bitmap image,
        string path)
    {
        // Пока оставляем временное сохранение.
        // Настоящий PDF подключим следующим этапом.
        image.Save(path, ImageFormat.Png);
    }
}