using API.DTOs;
using API.Models;
using API.Services;

namespace API.Endpoints;

public static class NoteEndpoint
{
    private static string route = "/notes";

    public static IEndpointRouteBuilder MapNotes(
        this IEndpointRouteBuilder routes
    )
    {
        var group = routes.MapGroup(route);

        group.MapGet("/", async (NoteService service) =>
        {
            var list = await service.GetNotesAsync();
            return Results.Ok(list);
        });

        group.MapPost("/", async (NoteService service, NewNoteDTO n) =>
        {
            var created = await service.CreateAsync(n);
            Console.WriteLine($"Note created: {created.Id}");
            return Results.Created($"{route}/{created.Id}", created);
        });

        group.MapGet("/{id}", async (NoteService service, string id) =>
        {
            var s = await service.GetAsync(id);
            return s != null ? Results.Ok(s) : Results.NotFound();
        });

        group.MapPut("/{id}", async (NoteService service, string id, NoteDTO n) =>
        {
            await service.UpdateAsync(id, n);
            Console.WriteLine($"Note updated: {id}");
        });

        group.MapDelete("/{id}", async (NoteService service, string id) =>
        {
            var note = await service.GetAsync(id);

            if (note is null)
                return Results.NotFound();

            await service.RemoveAsync(id);
            Console.WriteLine($"Note deleted: {id}");
            return Results.NoContent();
        });

        group.MapGet("/notepad/{id}", async (NoteService service, string id) =>
        {
            var s = await service.GetByNotepadAsync(id);
            return s != null ? Results.Ok(s) : Results.NotFound();
        });

        return routes;
    }
}
