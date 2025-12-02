using API.Models;
using API.Services;

namespace API.Endpoints;

public static class NotepadEndpoint
{
    private static string route = "/notepads";

    public static IEndpointRouteBuilder MapNotepads(
        this IEndpointRouteBuilder routes
    )
    {
        var group = routes.MapGroup(route);

        group.MapGet("/", async (NotepadService service) =>
        {
            var list = await service.GetNotepadsAsync();
            return Results.Ok(list);
        });

        group.MapPost("/", async (NotepadService service, Notepad n) =>
        {
            n.Deleted = null;
            n.Id = null;
            await service.CreateNotepadAsync(n);
            return Results.Ok(n);
            // return Results.Created($"{route}/{n.Id}", n);
        });

        return routes;
    }
}
