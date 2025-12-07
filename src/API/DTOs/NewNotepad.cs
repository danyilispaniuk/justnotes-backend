using API.Models;

namespace API.DTOs
{
    public class NewNotepadDTO
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        //public NoteDTO[]? notes { get; set; }
    }
}