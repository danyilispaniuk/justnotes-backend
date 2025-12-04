namespace API.DTOs
{
    public class NoteDTO
    {
        public string? id { get; set; }
        public string? notepadId { get; set; }
        public string? header { get; set; }
        public string? notes { get; set; }
        public string? created { get; set; }
        public string? updated { get; set; }
    }
}