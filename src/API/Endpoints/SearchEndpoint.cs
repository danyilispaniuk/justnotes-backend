using API.DTOs;
using API.Services;

namespace API.Endpoints;

public static class SearchEndpoint
{
    private static string route = "/search";

    public static IEndpointRouteBuilder MapSearches(
        this IEndpointRouteBuilder routes
    )
    {
        var group = routes.MapGroup(route);

        group.MapGet("/{searchWord}", async (NoteService noteService, NotepadService notepadService, string searchWord) =>
        {
            var noteList = await noteService.SearchNote(searchWord);
            var notepadList = await notepadService.SearchNotepad(searchWord);

            List<object> list = [noteList, notepadList];

            return Results.Ok(list);
        });


        return routes;
    }
}
