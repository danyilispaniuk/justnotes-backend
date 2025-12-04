using MongoDB.Driver;
using Microsoft.Extensions.Options;

using API.DTOs;
using API.Models;
using Microsoft.VisualBasic;

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

    public async Task CreateNotepadAsync(NewNotepadDTO n)
    {
        var notepad = new Notepad
        {
            Name = n.name,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
        };

        //Notes = n.notes,

        await collection.InsertOneAsync(notepad);
    }

    public async Task<List<NotepadDTO>> GetNotepadsAsync()
    {
        var notepads = await collection.Find(_ => true).ToListAsync();
        List<NotepadDTO> res = new List<NotepadDTO>();

        if(notepads.Count > 0)
        {
            for (int i = 0; i < notepads.Count; i++)
            {
                NotepadDTO notepadToDTO = new NotepadDTO
                {
                    id = notepads[i].Id,
                    name = notepads[i].Name,
                    notes = notepads[i].Notes,
                    created = notepads[i].Created.ToString(),
                    updated = notepads[i].Updated.ToString()
                };

                res.Add(notepadToDTO);
            }
        }

        return res;
    }

    public async Task<NotepadDTO> GetAsync(string id)
    {
        var notepad = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (notepad == null)
        {
            return new NotepadDTO
            {
                notes = null,
                name = null,
                created = null,
                updated = null
            };
        }

        return new NotepadDTO
        {
            name = notepad.Name,
            notes = notepad.Notes,
            created = notepad.Created.ToString(),
            updated = notepad.Updated.ToString()
        };
    }

    public async Task RemoveAsync(string id) =>
        await collection.DeleteOneAsync(x => x.Id == id);
}
