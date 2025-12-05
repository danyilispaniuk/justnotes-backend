using System.Globalization;
using API.DTOs;
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

        group.MapPost("/", async (NotepadService service, NewNotepadDTO n) =>
        {
            await service.CreateAsync(n);
            return Results.Ok(n);
            // return Results.Created($"{route}/{n.Id}", n);
        });

        group.MapGet("/{id}", async (NotepadService service, string id) =>
        {
            var s = await service.GetAsync(id);
            return s != null ? Results.Ok(s) : Results.NotFound();
        });

        group.MapPut("/{id}", async (NotepadService service, string id, NotepadDTO n) =>
        {
            var r = await service.UpdateAsync(id, n);
            return Results.Ok(r);
        });

        group.MapDelete("/", async (NotepadService service, string id) =>
        {
            var student = await service.GetAsync(id);

            if (student is null)
                return Results.NotFound();

            await service.RemoveAsync(id);
            return Results.NoContent();
        });

        return routes;
    }
}
