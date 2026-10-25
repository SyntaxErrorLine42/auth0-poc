using _1lab.Entities;
using _1lab.Models;
using _1lab.Persistence;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using QRCoder;

namespace _1lab.Endpoints;

sealed class CreatePassRequest
{
    public string Name { get; set; } = "";
    public string Document { get; set; } = "";
}

sealed class CreatePassEndpoint(AppDbContext db, IConfiguration config)
    : Endpoint<CreatePassRequest>
{
    public override void Configure()
    {
        Post("/passes");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Scopes("create:passes");
    }

    public override async Task HandleAsync(CreatePassRequest r, CancellationToken c)
    {
        if (string.IsNullOrWhiteSpace(r.Name) ||
            string.IsNullOrWhiteSpace(r.Document))
        {
            AddError(x => x.Name, "Name and Document are required.");
            await Send.ErrorsAsync(400, c);
            return;
        }

        var pass = new Pass
        {
            Id = Guid.NewGuid(),
            Name = r.Name,
            Document = r.Document,
            IssuedAt = DateTime.UtcNow,
            Status = PassStatus.ACTIVE
        };

        db.Passes.Add(pass);
        await db.SaveChangesAsync(c);

        var publicBaseUrl = config["PublicBaseUrl"]!.TrimEnd('/');
        var publicUrl = $"{publicBaseUrl}/info/{pass.Id}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(publicUrl, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrData);
        var png = qrCode.GetGraphic(10);

        HttpContext.Response.StatusCode = 201;
        HttpContext.Response.ContentType = "image/png";
        HttpContext.Response.Headers.Location = $"{publicBaseUrl}/passes/{pass.Id}";
        await HttpContext.Response.Body.WriteAsync(png, c);
    }
}
