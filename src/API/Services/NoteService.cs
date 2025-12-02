using MongoDB.Driver;
using Microsoft.Extensions.Options;

using API.Models;

namespace API.Services;
public class NoteService
{
    private readonly IMongoCollection<Note> collection;

    public NoteService(IOptions<NoteDatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        collection = db.GetCollection<Note>(settings.Value.CollectionName);
    }

    public async Task<List<Note>> GetNotesAsync() =>
        await collection.Find(_ => true).ToListAsync();

    public async Task CreateNoteAsync(Note n) =>
        await collection.InsertOneAsync(n);
}
