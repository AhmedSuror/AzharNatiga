#nullable disable warnings
namespace Natiga.Models;

public class StudentResultVM
{
    public string SeatNo { get; set; }
    public string Name { get; set; }

    // Subject-wise marks (dynamic key-value)
    public Dictionary<string, string> Marks { get; set; } = new();
}