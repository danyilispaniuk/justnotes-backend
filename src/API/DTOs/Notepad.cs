using API.Models;

namespace API.DTOs
{
    public class NotepadDTO
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public Note[]? notes { get; set; }
        
        public string? created { get; set; }
        public string? updated { get; set; }
    }
}