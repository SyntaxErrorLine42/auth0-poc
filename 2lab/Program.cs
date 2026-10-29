using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");
var app = builder.Build();

var xssEnabled = true;
var accessControlBroken = true;
var comments = new List<string>();
var loggedInUsers = new Dictionary<string, string>(); // sessionId -> username
var passwords = new Dictionary<string, string> { ["user"] = "user123", ["admin"] = "admin123" };


string? CurrentUser(HttpRequest request) =>
    loggedInUsers.GetValueOrDefault(request.Cookies["sid"] ?? "");

IResult Page(string html) => Results.Content(html, "text/html; charset=utf-8");

app.MapGet("/", (HttpRequest request) =>
{
    var commentList = string.Join("", comments.Select(c => $"<li>{(xssEnabled ? c : WebUtility.HtmlEncode(c))}</li>"));
    return Page($"""
        <h1>Security lab</h1>
        <p>{(CurrentUser(request) is { } name ? "Logged in as: " + WebUtility.HtmlEncode(name) : "Not logged in")}</p>
        <p>{(request.Query.ContainsKey("error") ? "Invalid username or password!" : "")}</p>
        <form method=post action=/login>
            <input name=username placeholder=username>
            <input name=password type=password placeholder=password>
            <button>Login</button>
        </form>

        <hr></hr>

        <h2>XSS (stored)</h2>
        <form method=post action=/toggle/xss>
            <label><input type=checkbox name=on {(xssEnabled ? "checked" : "")} onchange="this.form.submit()"> Vulnerability enabled</label>
            <p>Instructions: log in as user / user123, then post <code>&lt;script&gt;alert(document.cookie)&lt;/script&gt;</code> as a comment. With the vulnerability enabled the script runs on every page load and shows your session cookie. With it disabled the comment is HTML-encoded and shown as plain text.</p>
        </form>
        <form method=post action=/comment>
            <input name=text size=50> <button>Post comment</button>
        </form>
        <ul>{commentList}</ul>

        <hr></hr>

        <h2>Broken Access Control</h2>
        <form method=post action=/toggle/access>
            <label><input type=checkbox name=on {(accessControlBroken ? "checked" : "")} onchange="this.form.submit()"> Vulnerability enabled</label>
            <p>Instructions: log in as user / user123 (only admin sees the /admin link), then type <code>/admin</code> into the address bar manually. With the vulnerability enabled you see all passwords. With it disabled you get 403. Login as admin / admin123 always works and also shows the URL.</p>
        </form>
        {(CurrentUser(request) == "admin" ? "<a href=/admin>Open /admin</a>" : "")}
        """);
});

app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    if (passwords.GetValueOrDefault(username) == form["password"].ToString())
    {
        var sessionId = Guid.NewGuid().ToString();
        loggedInUsers[sessionId] = username;
        context.Response.Cookies.Append("sid", sessionId);
    } else {
        context.Request.Cookies.TryGetValue("sid", out var sessionId);
        if (!string.IsNullOrWhiteSpace(sessionId)) loggedInUsers.Remove(sessionId);
        return Results.Redirect("/?error");
    }
    return Results.Redirect("/");
});

app.MapPost("/comment", async (HttpContext context) =>
{
    comments.Add((await context.Request.ReadFormAsync())["text"].ToString());
    return Results.Redirect("/");
});

app.MapPost("/toggle/xss", async (HttpRequest request) =>
{
    xssEnabled = (await request.ReadFormAsync()).ContainsKey("on");
    return Results.Redirect("/");
});

app.MapPost("/toggle/access", async (HttpRequest request) =>
{
    accessControlBroken = (await request.ReadFormAsync()).ContainsKey("on");
    return Results.Redirect("/");
});

app.MapGet("/admin", (HttpRequest request) =>
{
    var user = CurrentUser(request);
    if (user == null) return Results.Unauthorized();
    if (!accessControlBroken && user != "admin") return Results.StatusCode(403);

    var passwordList = string.Join("", passwords.Select(p => $"<p>{p.Key}: {p.Value}</p>"));
    return Page($"<h1>Admin panel</h1>{passwordList}<a href=/>Back</a>");
});

app.Run();
