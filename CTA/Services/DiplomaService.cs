using System.Drawing;
using System.Drawing.Imaging;
using CTA.Models;

namespace CTA.Services;

public class DiplomaService
{
    private readonly Dictionary<string, TextElementSettings> _settings;
    private readonly string _templatePath;

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

        using var brush = new SolidBrush(Color.Black);

        // Шрифты
        using var fontPlace = new Font(
            "Times New Roman",
            60);

        using var fontAward = new Font(
            "Times New Roman",
            30);

        using var fontContest = new Font(
            "Times New Roman",
            45);

        using var fontName = new Font(
            "Times New Roman",
            40);

        using var fontTeacher = new Font(
            "Times New Roman",
            35);

        using var fontSmall = new Font(
            "Times New Roman",
            20);

        // Место
        var place = student.Place;

        if (place == "1")
        {
            graphics.DrawString(
                "I",
                fontPlace,
                brush,
                680,
                315);

            DrawCenteredText(
                graphics,
                "ЖЕҢІМПАЗЫ",
                fontAward,
                620,
                image.Width,
                brush);
        }
        else if (place == "2")
        {
            graphics.DrawString(
                "II",
                fontPlace,
                brush,
                680,
                315);

            DrawCenteredText(
                graphics,
                "ЖҮЛДЕГЕРІ",
                fontAward,
                620,
                image.Width,
                brush);
        }
        else
        {
            graphics.DrawString(
                "III",
                fontPlace,
                brush,
                680,
                315);

            DrawCenteredText(
                graphics,
                "ЖҮЛДЕГЕРІ",
                fontAward,
                620,
                image.Width,
                brush);
        }

        // Название конкурса
        if (!string.IsNullOrWhiteSpace(student.Contest))
        {
            var contestSettings =
                _settings["Название конкурса"];

            using var contestFont =
                new Font(
                    "Times New Roman",
                    (float)contestSettings.FontSize);

            DrawCenteredScaledText(
                graphics,
                student.Contest,
                contestFont,
                (float)contestSettings.Y,
                image.Width,
                (float)contestSettings.WidthScale,
                (float)contestSettings.HeightScale,
                brush);
        }

        // ФИО ученика
        var nameSettings =
            _settings["ФИО ученика"];

        using var nameFont =
            new Font(
                "Times New Roman",
                (float)nameSettings.FontSize);

        DrawCenteredScaledText(
            graphics,
            student.FullName,
            nameFont,
            (float)nameSettings.Y,
            image.Width,
            (float)nameSettings.WidthScale,
            (float)nameSettings.HeightScale,
            brush);

        // Учитель
        var teacherSettings =
            _settings["ФИО преподавателя"];

        using var teacherFont =
            new Font(
                "Times New Roman",
                (float)teacherSettings.FontSize);

        DrawScaledText(
            graphics,
            student.Teacher,
            teacherFont,
            (float)teacherSettings.X,
            (float)teacherSettings.Y,
            (float)teacherSettings.WidthScale,
            (float)teacherSettings.HeightScale,
            brush);
        
        // Регистрационный номер
        var registrationNumber =
            student.RegistrationNumber.PadLeft(7, '0');

        var registrationSettings =
            _settings["Регистрационный номер"];

        using var registrationFont =
            new Font(
                "Times New Roman",
                (float)registrationSettings.FontSize);

        DrawScaledText(
            graphics,
            $"№-{registrationNumber}",
            registrationFont,
            (float)registrationSettings.X,
            (float)registrationSettings.Y,
            (float)registrationSettings.WidthScale,
            (float)registrationSettings.HeightScale,
            brush);

        // Учебный год
        var yearSettings =
            _settings["Учебный год"];

        using var yearFont =
            new Font(
                "Times New Roman",
                (float)yearSettings.FontSize);

        DrawScaledText(
            graphics,
            "2026-2027 оқу жылы",
            yearFont,
            (float)yearSettings.X,
            (float)yearSettings.Y,
            (float)yearSettings.WidthScale,
            (float)yearSettings.HeightScale,
            brush);
        return image;
    }

    private static void DrawCenteredText(
        Graphics graphics,
        string text,
        Font font,
        float y,
        int imageWidth,
        Brush brush)
    {
        var size = graphics.MeasureString(text, font);

        var x = (imageWidth - size.Width) / 2;

        graphics.DrawString(
            text,
            font,
            brush,
            x,
            y);
    }

    public static void SavePdf(
        Bitmap image,
        string path)
    {
        image.Save(
            path,
            ImageFormat.Png);
    }
    private static void DrawScaledText(
        Graphics graphics,
        string text,
        Font font,
        float x,
        float y,
        float widthScale,
        float heightScale,
        Brush brush)
    {
        var state = graphics.Save();

        graphics.TranslateTransform(x, y);

        graphics.ScaleTransform(
            widthScale,
            heightScale);

        graphics.DrawString(
            text,
            font,
            brush,
            0,
            0);

        graphics.Restore(state);
    }
    private static void DrawCenteredScaledText(
        Graphics graphics,
        string text,
        Font font,
        float y,
        int imageWidth,
        float widthScale,
        float heightScale,
        Brush brush)
    {
        var size = graphics.MeasureString(text, font);

        var scaledWidth =
            size.Width * widthScale;

        var x =
            (imageWidth - scaledWidth) / 2;

        DrawScaledText(
            graphics,
            text,
            font,
            x,
            y,
            widthScale,
            heightScale,
            brush);
    }
}