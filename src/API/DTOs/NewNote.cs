namespace API.DTOs
{
    public class NewNoteDTO
    {
        public string? Id { get; set; }
        public string? notepadId { get; set; }
        public required string header { get; set; }
        public required string notes { get; set; }
    }
}