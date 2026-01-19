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
            Header = n.header.ToString(),
            Notes = n.notes.ToString(),
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
            Id = note.Id,
            NotepadId = note.NotepadId,
            Header = note.Header,
            Notes = note.Notes,
            Created = note.Created.ToString("O"),
            Updated = note.Updated.ToString("O"),
        };
    }

    public async Task<List<NoteDTO>> GetNotesAsync()
    {
        var notes = await collection.Find(_ => true).ToListAsync();

        var res = notes.OrderByDescending(n => n.Updated).Select(n => new NoteDTO
        {
            Id = n.Id,
            NotepadId = n.NotepadId,
            Header = n.Header,
            Notes = n.Notes,
            Created = n.Created.ToString("O"),
            Updated = n.Updated.ToString("O"),
        }).ToList();

        return res;
    }

    public async Task<NoteDTO?> GetAsync(string id)
    {
        var note = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (note == null)
            return null;

        return new NoteDTO
        {
            Id = note.Id,
            NotepadId = note.NotepadId,
            Header = note.Header,
            Notes = note.Notes,
            Created = note.Created.ToString("O"),
            Updated = note.Updated.ToString("O"),
        };
    }

    public async Task<NoteDTO[]?> GetByNotepadAsync(string id)
    {
        var notepad = await notepadCollection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (notepad == null)
            return null;

        var notes = await collection
            .Find(x => x.NotepadId == id)
            .ToListAsync();

        if (notes == null || notes.Count == 0)
            return Array.Empty<NoteDTO>();

        var res = notes
            .OrderByDescending(n => n.Updated)
            .Select(note => new NoteDTO
            {
                Id = note.Id,
                NotepadId = note.NotepadId,
                Header = note.Header,
                Notes = note.Notes,
                Created = note.Created.ToString("O"),
                Updated = note.Updated.ToString("O"),
            })
            .ToArray();

        return res;
    }


    public async Task UpdateAsync(string id, NoteDTO dto)
    {
        var note = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (note == null) return;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            note.Notes = dto.Notes;

        if (string.IsNullOrWhiteSpace(dto.Header) && !string.IsNullOrWhiteSpace(note.Notes))
        {
            note.Header = FirstFiveWords(note.Notes);
        }
        else
        {
            note.Header = dto.Header??FirstFiveWords(note.Notes);
        }

        note.Updated = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.NotepadId))
        {
            var notepad = await notepadCollection.Find(x => x.Id == dto.NotepadId).FirstOrDefaultAsync();

            if (notepad != null)
                note.NotepadId = dto.NotepadId;
            else
                note.NotepadId = null;
        }
        else
        {
            note.NotepadId = null;
        }

        await collection.ReplaceOneAsync(x => x.Id == id, note);
        Console.WriteLine($"Note updated: {note.Id}");
    }

    public async Task RemoveAsync(string id)
    {
        await collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<List<NoteDTO>> SearchAsync(string searchWord)
    {
        var notes = await collection.Find(x => x.Header.Contains(searchWord)).ToListAsync();

        return notes.Select(n => new NoteDTO
        {
            Id = n.Id,
            NotepadId = n.NotepadId,
            Header = n.Header,
            Notes = n.Notes,
            Created = n.Created.ToString("O"),
            Updated = n.Updated.ToString("O"),
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
