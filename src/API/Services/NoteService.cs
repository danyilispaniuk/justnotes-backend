using MongoDB.Driver;
using MongoDB.Driver.Search;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;
using API.Services;

namespace API.Services;
public class NoteService
{
    private readonly IMongoCollection<Note> collection;
    private readonly NotepadService notepadService;

    public NoteService(IOptions<NoteDatabaseSettings> settings, NotepadService notepadService)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Note>(settings.Value.CollectionName);

        this.notepadService = notepadService;
    }

    public async Task CreateNoteAsync(NewNoteDTO n)
    {
        n.header ??= n.notes;

        var note = new Note
        {
            NotepadId = null,
            Header = n.header,
            Notes = n.notes,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(n.notepadId))
        {
            var notepad = await notepadService.GetAsync(n.notepadId);
            if (notepad != null)
                note.NotepadId = n.notepadId;
        }

        await collection.InsertOneAsync(note);
    }

    public async Task<List<NoteDTO>> GetNotesAsync()
    {
        var notes = await collection.Find(_ => true).ToListAsync();

        return notes.Select(n => new NoteDTO
        {
            id = n.Id,
            notepadId = n.NotepadId,
            header = n.Header,
            notes = n.Notes,
            created = n.Created.ToString(),
            updated = n.Updated.ToString()
        }).ToList();
    }

    public async Task<NoteDTO?> GetAsync(string id)
    {
        var note = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (note == null)
            return null;

        return new NoteDTO
        {
            id = note.Id,
            notepadId = note.NotepadId,
            header = note.Header,
            notes = note.Notes,
            created = note.Created.ToString(),
            updated = note.Updated.ToString()
        };
    }

    public async Task RemoveAsync(string id) =>
        await collection.DeleteOneAsync(x => x.Id == id);

    // public async Task<List<Note>> SearchNoteAsync(string searchWord)
    // {
    //     var result = await collection.Aggregate()
    //         .Search(x => x
    //             .Autocomplete(n => n.Header, searchWord)
    //             .Index("notesSearch")
    //         )
    //         .ToListAsync();

    //     return result;
    // }

    public async Task<List<NoteDTO>?> SearchNote(string searchWord)
    {
        var notes = await collection.Find(x => x.Header.Contains(searchWord)).ToListAsync();

        return notes.Select(n => new NoteDTO
        {
            id = n.Id,
            notepadId = n.NotepadId,
            header = n.Header,
            notes = n.Notes,
            created = n.Created.ToString(),
            updated = n.Updated.ToString()
        }).ToList();
    }
}
