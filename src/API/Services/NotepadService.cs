using MongoDB.Driver;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;
using MongoDB.Bson;

namespace API.Services;
public class NotepadService
{
    private readonly IMongoCollection<Notepad> collection;
    private readonly IMongoCollection<Note> noteCollection;

    public NotepadService(IOptions<NotepadDatabaseSettings> settings, IOptions<NoteDatabaseSettings> noteSettings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Notepad>(settings.Value.CollectionName);
        noteCollection = db.GetCollection<Note>(noteSettings.Value.CollectionName);
    }

    public async Task<NotepadDTO> CreateAsync(NewNotepadDTO n)
    {
            var notepad = new Notepad
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = n.Name.ToString(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
            };
            Console.WriteLine(notepad.Name);

            await collection.InsertOneAsync(notepad);

            return new NotepadDTO
            {
                Id = notepad.Id,
                Name = notepad.Name,
                Created = notepad.Created.ToString("O"),
                Updated = notepad.Updated.ToString("O")
            };
    }

    public async Task<List<NotepadDTO>> GetNotepadsAsync()
    {
        var notepads = await collection.Find(_ => true).ToListAsync();

        var res = notepads
            .OrderByDescending(n => n.Updated)
            .Select(np => new NotepadDTO
            {
                Id = np.Id,
                Name = np.Name,
                Created = np.Created.ToString("O"),
                Updated = np.Updated.ToString("O"),
            })
            .ToList();

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
            Id = notepad.Id,
            Name = notepad.Name,
            Created = notepad.Created.ToString("O"),
            Updated = notepad.Updated.ToString("O")
        };

        return res;
    }

    public async Task UpdateAsync(string id, NotepadDTO notepad)
    {
        
        if (!string.IsNullOrWhiteSpace(notepad.Name))
        {
            var filter = Builders<Notepad>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notepad>.Update.
                Set(n => n.Name, notepad.Name).
                Set(n => n.Updated, DateTime.UtcNow);

            var updateResult = await collection.UpdateOneAsync(filter, update);
            Console.WriteLine($"Notepad {id} update: {updateResult.ModifiedCount} fields");
        }
    }

    public async Task RemoveAsync(string id)
    {
        var result = await collection.DeleteOneAsync(x => x.Id == id);
        if (result.DeletedCount == 1)
        {
            var notes = await noteCollection.Find(x => x.Id == id).ToListAsync();

            if (notes != null)
            {
                var filter = Builders<Note>.Filter.Eq(n => n.NotepadId, id);
                noteCollection.DeleteMany(filter);
            }
        }
    }
    
    public async Task<List<NotepadDTO>> SearchAsync(string searchWord)
    {
        var notepads = await collection.Find(x => x.Name.Contains(searchWord)).ToListAsync();
        List<NotepadDTO> res = new List<NotepadDTO>();

        foreach (var np in notepads)
        {
            var notes = np.Notes?.ToList() ?? new List<Note>();

            var noteDTOs = notes.Select(n => new NoteDTO
            {
                Id = n.Id,
                NotepadId = n.NotepadId,
                Header = n.Header,
                Notes = n.Notes,
                Created = n.Created.ToString("O"),
                Updated = n.Updated.ToString("O")
            }).ToArray();

            res.Add(new NotepadDTO
            {
                Id = np.Id,
                Name = np.Name,
                Created = np.Created.ToString("O"),
                Updated = np.Updated.ToString("O")
            });
        }

        return res;
    }
}
