using MongoDB.Driver;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;
using API.Services;

namespace API.Services;
public class NoteService
{
    private readonly IMongoCollection<Note> collection;

    private NotepadService notepadService;

    public NoteService(IOptions<NoteDatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Note>(settings.Value.CollectionName);
    }

        public async Task CreateNoteAsync(NewNoteDTO n)
    {
        n.header ??= n.notes;;
        
        var note = new Note
        {
            NotepadId = null,
            Header = n.header,
            Notes = n.notes,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        if (n.notepadId != null)
        {
            var notepad = await notepadService.GetAsync(n.notepadId);
            if (notepad != null) note.NotepadId = n.notepadId;
        };

        await collection.InsertOneAsync(note);
    }

    public async Task<List<NoteDTO>> GetNotesAsync()
    {
        var notes = await collection.Find(_ => true).ToListAsync();
        List<NoteDTO> res = new List<NoteDTO>();

        if(notes.Count > 0)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                NoteDTO noteToDTO = new NoteDTO
                {
                    id = notes[i].Id,
                    header = notes[i].Header,
                    notes = notes[i].Notes,
                    created = notes[i].Created.ToString(),
                    updated = notes[i].Updated.ToString()
                };

                res.Add(noteToDTO);
            }
        }

        return res;
    }
    
    // public async Task<List<Note>> GetNotesAsync() =>
    //     await collection.Find(_ => true).ToListAsync();

    public async Task<NoteDTO> GetAsync(string id)
    {
        var note = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (note == null)
        {
            return new NoteDTO
            {
                notepadId = null,
                header = null,
                notes = null,
                created = null,
                updated = null
            };
        }

        return new NoteDTO
        {
            notepadId = note.NotepadId,
            header = note.Header,
            notes = note.Notes,
            created = note.Created.ToString(),
            updated = note.Updated.ToString()
        };
    }

    public async Task RemoveAsync(string id) =>
        await collection.DeleteOneAsync(x => x.Id == id);
}
