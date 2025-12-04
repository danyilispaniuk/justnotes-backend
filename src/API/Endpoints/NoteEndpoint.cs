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
            await service.CreateNoteAsync(n);
            return Results.Ok(n);
            // return Results.Created($"{route}/{n.Id}", n);
        });

        group.MapGet("/{id}", async (NoteService service, string id) =>
        {
            var s = await service.GetAsync(id);
            return s != null ? Results.Ok(s) : Results.NotFound();
        });

        group.MapDelete("/", async (NoteService service, string id) =>
        {
            var note = await service.GetAsync(id);

            if (note is null)
                return Results.NotFound();

            await service.RemoveAsync(id);
            return Results.NoContent();
        });

        return routes;
    }
}
