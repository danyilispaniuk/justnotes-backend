using MongoDB.Driver;
using Microsoft.Extensions.Options;

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

    public async Task<List<Notepad>> GetNotepadsAsync() =>
        await collection.Find(_ => true).ToListAsync();

    public async Task CreateNotepadAsync(Notepad n) =>
        await collection.InsertOneAsync(n);
}
