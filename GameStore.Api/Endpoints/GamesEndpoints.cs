using GameStore.Api.Data;
using GameStore.Api.DTOs;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;
namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();
        //GET /games
        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Games.Include(game => game.Genre).Select(game => game.ToGameSummaryDto()).AsNoTracking().ToListAsync()
        );

        //GET /games/1
        group.MapGet("/{id}", async (int id,GameStoreContext dbContext) =>
        {
            Game? game = await dbContext.Games.FindAsync(id);
            return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        });

        // POST /games
        group.MapPost("/",async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();
            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/games/{game.Id}", game.ToGameDetailsDto());
        });

        // PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame,GameStoreContext dbContext) =>
        {
            var exisitingGame = await dbContext.Games.FindAsync(id);
            if (exisitingGame is null)
            {
                return Results.NotFound();
            }
            dbContext.Entry(exisitingGame).CurrentValues.SetValues(updatedGame.ToEntity(id));
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}",async (int id,GameStoreContext dbContext) =>
        {
            await dbContext.Games.Where(game => game.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        return group;
    }
}
