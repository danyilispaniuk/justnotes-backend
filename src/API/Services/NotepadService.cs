using MongoDB.Driver;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;

namespace API.Services;
public class NotepadService
{
    private readonly IMongoCollection<Notepad> collection;

    public NotepadService(IOptions<NotepadDatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Notepad>(settings.Value.CollectionName);
    }

    public async Task CreateAsync(NewNotepadDTO n)
    {
        var notepad = new Notepad
        {
            Name = n.name,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
        };

        await collection.InsertOneAsync(notepad);
    }

    public async Task<List<NotepadDTO>>GetNotepadsAsync()
    {
        var notepads = await collection.Find(_ => true).ToListAsync();
        List<NotepadDTO> res = new List<NotepadDTO>();

        foreach (var np in notepads)
        {
            var notes = np.Notes?.ToList() ?? new List<Note>();

            var noteDTOs = notes.Select(n => new NoteDTO
            {
                id = n.Id,
                notepadId = n.NotepadId,
                header = n.Header,
                notes = n.Notes,
                created = n.Created.ToString(),
                updated = n.Updated.ToString()
            }).ToArray();

            res.Add(new NotepadDTO
            {
                id = np.Id,
                name = np.Name,
                created = np.Created.ToString(),
                updated = np.Updated.ToString(),
                // notes = noteDTOs
            });
        }

        return res;
    }

    public async Task<NotepadDTO?>GetAsync(string id)
    {
        var notepad = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (notepad == null)
        {
            return null;
        }
        
        var res = new NotepadDTO
        {
            id = notepad.Id,
            name = notepad.Name,
            created = notepad.Created.ToString(),
            updated = notepad.Updated.ToString()
        };

        var notes = notepad.Notes?.ToList() ?? new List<Note>();

        var noteDTOs = notes.Select(n => new NoteDTO
        {
            id = n.Id,
            notepadId = n.NotepadId,
            header = n.Header,
            notes = n.Notes,
            created = n.Created.ToString(),
            updated = n.Updated.ToString()
        }).ToArray();

        //res.notes = noteDTOs;

        return res;
    }

    public async Task UpdateAsync(string id, NotepadDTO notepad)
    {
        
        if (!string.IsNullOrWhiteSpace(notepad.name))
        {
            var filter = Builders<Notepad>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notepad>.Update.
                Set(n => n.Name, notepad.name).
                Set(n => n.Updated, DateTime.UtcNow);

            var updateResult = await collection.UpdateOneAsync(filter, update);
            Console.WriteLine($"Notepad {id} update: {updateResult.ModifiedCount} fields");
        }
    }

    public async Task RemoveAsync(string id) =>
        await collection.DeleteOneAsync(x => x.Id == id);
    
    public async Task<List<NotepadDTO>> SearchAsync(string searchWord)
    {
        var notepads = await collection.Find(x => x.Name.Contains(searchWord)).ToListAsync();
        List<NotepadDTO> res = new List<NotepadDTO>();

        foreach (var np in notepads)
        {
            var notes = np.Notes?.ToList() ?? new List<Note>();

            var noteDTOs = notes.Select(n => new NoteDTO
            {
                id = n.Id,
                notepadId = n.NotepadId,
                header = n.Header,
                notes = n.Notes,
                created = n.Created.ToString(),
                updated = n.Updated.ToString()
            }).ToArray();

            res.Add(new NotepadDTO
            {
                id = np.Id,
                name = np.Name,
                created = np.Created.ToString(),
                updated = np.Updated.ToString(),
                //notes = noteDTOs
            });
        }

        return res;
    }
}
