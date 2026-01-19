using API.Models;

namespace API.DTOs
{
    public class NotepadDTO
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        // public NoteDTO[]? notes { get; set; }
        
        public string? Created { get; set; }
        public string? Updated { get; set; }
    }
}