namespace API.Models;

public class Note : Model
{
    public string? NotepadId { get; set; }
    public required string Header { get; set; }
    public string? Notes { get; set; }
}
