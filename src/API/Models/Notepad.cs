namespace API.Models;

public class Notepad : Model
{
    public required string Name { get; set; }
    public Note[]? Notes { get; set; }

}
