using MongoDB.Driver;
using MongoDB.Driver.Search;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;
using API.Services;
using MongoDB.Bson;

namespace API.Services;
public class NoteService
{
    private readonly IMongoCollection<Note> collection;
    private readonly IMongoCollection<Notepad> notepadCollection;
    private readonly NotepadService notepadService;

    public NoteService(IOptions<NoteDatabaseSettings> settings, IOptions<NotepadDatabaseSettings> notepadSettings, NotepadService notepadService)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Note>(settings.Value.CollectionName);
        notepadCollection = db.GetCollection<Notepad>(notepadSettings.Value.CollectionName);

        this.notepadService = notepadService;
    }
    public async Task<NoteDTO> CreateAsync(NewNoteDTO n)
    {
        n.header ??= FirstFiveWords(n.notes);

        var note = new Note
        {
            Id = ObjectId.GenerateNewId().ToString(),
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

    // public async Task CreateAsync(NewNoteDTO n)
    // {
    //     n.header ??= FirstFiveWords(n.notes);

    //     var note = new Note
    //     {
    //         Id = null,
    //         NotepadId = null,
    //         Header = n.header,
    //         Notes = n.notes,
    //         Created = DateTime.UtcNow,
    //         Updated = DateTime.UtcNow
    //     };

    //     if (!string.IsNullOrEmpty(n.notepadId))
    //     {
    //         var notepad = await notepadService.GetAsync(n.notepadId);
    //         if (notepad != null)
    //             note.NotepadId = n.notepadId;
    //     }

    //     await collection.InsertOneAsync(note);
    // }

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

    public async Task<NoteDTO[]?> GetByNotepadAsync(string id)
    {
        var notepad = await notepadCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (notepad == null) return null;
        
        var notes = await collection.Find(x => x.NotepadId == id).ToListAsync();
        if (notes == null) return null;
        
        List<NoteDTO> res = new List<NoteDTO>();
        foreach (var note in notes)
        {
            res.Add(new NoteDTO
            {
                id = note.Id,
                header = note.Header,
                notes = note.Notes,
                notepadId = note.NotepadId,
                created = note.Created.ToString(),
                updated = note.Updated.ToString()
            });
        }

        return res.ToArray();
    }

    public async Task UpdateAsync(string id, NoteDTO dto)
    {
        var note = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (note == null) return;

        if (!string.IsNullOrWhiteSpace(dto.notes))
            note.Notes = dto.notes;

        if (string.IsNullOrWhiteSpace(dto.header) && !string.IsNullOrWhiteSpace(note.Notes))
        {
            note.Header = FirstFiveWords(note.Notes);
        }
        else
        {
            note.Header = dto.header??FirstFiveWords(note.Notes);
        }

        note.Updated = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.notepadId))
        {
            var notepad = await notepadCollection.Find(x => x.Id == dto.notepadId).FirstOrDefaultAsync();

            if (notepad != null)
                note.NotepadId = dto.notepadId;
            else
                note.NotepadId = null;
        }
        else
        {
            note.NotepadId = null;
        }

        await collection.ReplaceOneAsync(x => x.Id == id, note);
        Console.WriteLine($"Note update: {note.Id}");
    }

    public async Task RemoveAsync(string id) =>
        await collection.DeleteOneAsync(x => x.Id == id);

    public async Task<List<NoteDTO>> SearchAsync(string searchWord)
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

    public static string FirstFiveWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var words = text
            .Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', words.Take(5));
    }
}
