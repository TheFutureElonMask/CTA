using PdfSharp.Pdf;

namespace CTA.Models;

public class StudentRecord
{
    public string Place { get; set; } = "";
    public string Contest { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Teacher { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string SchoolName { get; set; } = "";

    public string FullName => $"{FirstName} {LastName}".Trim();
}