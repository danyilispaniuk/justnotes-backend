namespace API.DTOs
{
    public class NoteDTO
    {
        public string? Id { get; set; }
        public string? NotepadId { get; set; }
        public string? Header { get; set; }
        public required string Notes { get; set; }
        public string? Created { get; set; }
        public string? Updated { get; set; }
    }
}