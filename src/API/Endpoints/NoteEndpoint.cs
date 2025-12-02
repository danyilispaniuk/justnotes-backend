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

        group.MapPost("/", async (NoteService service, Note n) =>
        {
            n.Deleted = null;
            n.Id = null;
            await service.CreateNoteAsync(n);
            return Results.Ok(n);
            // return Results.Created($"{route}/{n.Id}", n);
        });

        return routes;
    }
}
