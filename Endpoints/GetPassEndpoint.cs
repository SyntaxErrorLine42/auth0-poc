namespace _1lab.Endpoints;

using _1lab.Models;
using _1lab.Persistence;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

sealed class GetPassRequest
{
    public Guid Id { get; set; }
}

sealed class GetPassResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Document { get; set; } = "";
    public DateTime IssuedAt { get; set; }
    public PassStatus Status { get; set; }
}

sealed class GetPassEndpoint(AppDbContext db) : Endpoint<GetPassRequest, GetPassResponse>
{
    public override void Configure()
    {
        Get("/passes/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Scopes("read:passes");
    }

    public override async Task HandleAsync(GetPassRequest r, CancellationToken c)
    {
        var pass = await db.Passes.FirstOrDefaultAsync(p => p.Id == r.Id, c);
        if (pass is null)
        {
            await Send.NotFoundAsync(c);
            return;
        }
        Response = new GetPassResponse
        {
            Id = pass.Id,
            Name = pass.Name,
            Document = pass.Document,
            IssuedAt = pass.IssuedAt,
            Status = pass.Status,
        };
    }
}
