namespace API.DTOs
{
    public class NewNoteDTO
    {
        public string? notepadId { get; set; }
        public required string header { get; set; }
        public required string notes { get; set; }
    }
}