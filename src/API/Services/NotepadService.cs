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
                notes = noteDTOs
            });
        }

        return res;
    }

    public async Task<NotepadDTO>GetAsync(string id)
    {
        var notepad = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (notepad == null)
        {
            return new NotepadDTO
            {
                id = null,
                notes = null,
                name = null,
                created = null,
                updated = null
            };
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

        res.notes = noteDTOs;

        return res;
    }

    public async Task<NotepadDTO>UpdateAsync(string id, NotepadDTO notepad)
    {
        
        if (notepad.name != null | notepad.name != "")
        {
            var filter = Builders<Notepad>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notepad>.Update.
                Set(n => n.Name, notepad.name).
                Set(n => n.Updated, DateTime.UtcNow);

            var updateResult = await collection.UpdateOneAsync(filter, update);
            if (updateResult.IsAcknowledged && updateResult.ModifiedCount > 0)
            {
                Console.WriteLine($"Successfully updated {updateResult.ModifiedCount} document(s).");
            }
            else if (updateResult.MatchedCount > 0 && updateResult.ModifiedCount == 0)
            {
                Console.WriteLine("Document matched, but no changes were applied (maybe the values were already the same).");
            }
            else
            {
                Console.WriteLine("No document matched the filter.");
            }
        }

        NotepadDTO res = await GetAsync(id);
        return res;
    }
    // {
    //     var dbNotepad = await collection.Find(x => x.Id == notepad.id).FirstOrDefaultAsync();

    //     if(notepad.name != null) dbNotepad.Name = notepad.name;
    //     if(notepad.notes != null)
    //     {
    //         Note[] notesToSave = [];
    //         foreach(var note in notepad.notes)
    //         {
    //             var noteToSave = new Note
    //             {
    //                 Id = note.id,
    //                 Notes = note.notes,
    //                 Header = note.header,
    //                 Created = DateTime.Parse(note.created),
    //                 Updated = DateTime.Parse(note.updated),
    //             };
    //             notesToSave.Append(noteToSave);
    //         }
    //         dbNotepad.Notes = notesToSave;
    //         NoteDTO[] noteDTOs = [];
    //         foreach (var note in dbNotepad.Notes)
    //         {
    //             var noteDTO = new NoteDTO
    //             {
    //                 id = note.Id,
    //                 notes = note.Notes,
    //                 header = note.Header,
    //                 created = note.Created.ToString(),
    //                 updated = note.Updated.ToString(),
    //             };
    //         }

    //         dbNotepad.Updated = DateTime.UtcNow;

    //         return new NotepadDTO
    //         {
    //             id = dbNotepad.Id,
    //             name = dbNotepad.Name,
    //             notes = noteDTOs,
    //             created = dbNotepad.Created.ToString(),
    //             updated = dbNotepad.Updated.ToString()
    //         };
    //     }
        
    //     dbNotepad.Updated = DateTime.UtcNow;
    //     return new NotepadDTO
    //     {
    //         id = dbNotepad.Id,
    //         name = dbNotepad.Name,
    //         notes = [],
    //         created = dbNotepad.Created.ToString(),
    //         updated = dbNotepad.Updated.ToString()
    //     };
    // }

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
                notes = noteDTOs
            });
        }

        return res;
    }
}
