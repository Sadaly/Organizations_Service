using System.Net;
using System.Text.RegularExpressions;
using OrganizationsService.IntegrationsTests.Infrastructure;

namespace OrganizationsService.IntegrationsTests;

/// <summary>
/// End-to-end tests covering the HTTP, MVC, application, infrastructure, and PostgreSQL layers.
/// </summary>
[Collection(PostgresCollection.Name)]
public partial class OrganizationsEndpointsTests(PostgresFixture fx)
{
    private readonly HttpClient _client = fx.Factory.CreateClient();

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"/Organizations/Details/(\d+)")]
    private static partial Regex DetailsLinkRegex();

    private async Task<HttpResponseMessage> PostFormAsync(
        string url,
        Dictionary<string, string> fields)
    {
        var html = await _client.GetStringAsync(url);
        fields["__RequestVerificationToken"] = TokenRegex().Match(html).Groups[1].Value;

        return await _client.PostAsync(url, new FormUrlEncodedContent(fields));
    }

    private async Task<string> FindIdByTinAsync(string tin)
    {
        var html = await _client.GetStringAsync($"/Organizations?search={tin}");
        return DetailsLinkRegex().Match(html).Groups[1].Value;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Organizations")]
    [InlineData("/Organizations/Create")]
    [InlineData("/Home/Privacy")]
    public async Task Pages_ReturnHtml(string url)
    {
        var response = await _client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        Assert.Equal("Healthy", await _client.GetStringAsync("/health"));
    }

    [Fact]
    public async Task Index_ShowsSeededOrganizations()
    {
        var html = await _client.GetStringAsync("/Organizations");

        Assert.Contains("Boring company", html);
        Assert.Contains("Whatever CO", html);
    }

    [Fact]
    public async Task Details_Unknown_Returns404()
    {
        var response = await _client.GetAsync("/Organizations/Details/100500");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutAntiforgeryToken_IsRejected()
    {
        var response = await _client.PostAsync(
            "/Organizations/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = "X",
                ["Tin"] = "9999999999"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Invalid_ShowsValidationErrors()
    {
        var response = await PostFormAsync("/Organizations/Create", new()
        {
            ["Name"] = "",
            ["Tin"] = "12",
            ["Email"] = "not-an-email",
            ["RegisteredAt"] = "2026-01-01"
        });

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Organization name is required.", html);
        Assert.Contains("TIN must contain 10 or 12 digits.", html);
        Assert.Contains("The email address is invalid.", html);
    }

    [Fact]
    public async Task Create_DuplicateTin_ShowsError()
    {
        var response = await PostFormAsync("/Organizations/Create", new()
        {
            ["Name"] = "Duplicate Organization",
            ["Tin"] = "7701234567",
            ["RegisteredAt"] = "2026-01-01"
        });

        Assert.Contains(
            "An organization with this TIN already exists.",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task FullCrudCycle_Works()
    {
        const string tin = "5555555555";

        // Create
        var create = await PostFormAsync("/Organizations/Create", new()
        {
            ["Name"] = "Integration Test LLC",
            ["Tin"] = tin,
            ["Description"] = "Organization created by an integration test",
            ["Address"] = "10 Test Street",
            ["Phone"] = "+78001234567",
            ["Email"] = "integration@example.com",
            ["RegisteredAt"] = "2026-01-01"
        });

        create.EnsureSuccessStatusCode();
        Assert.Contains("Integration Test LLC", await create.Content.ReadAsStringAsync());

        var id = await FindIdByTinAsync(tin);
        Assert.NotEmpty(id);

        // Read
        var details = await _client.GetStringAsync($"/Organizations/Details/{id}");

        Assert.Contains("Integration Test LLC", details);
        Assert.Contains("Organization created by an integration test", details);
        Assert.Contains("10 Test Street", details);
        Assert.Contains("integration@example.com", details);
        Assert.Contains("01.01.2026", details);

        // Update
        var edit = await PostFormAsync($"/Organizations/Edit/{id}", new()
        {
            ["Id"] = id,
            ["Name"] = "Updated Integration Test LLC",
            ["Tin"] = tin,
            ["Description"] = "Updated organization description",
            ["Address"] = "20 Test Street",
            ["Phone"] = "+78009876543",
            ["Email"] = "updated@example.com",
            ["RegisteredAt"] = "2026-01-01"
        });

        edit.EnsureSuccessStatusCode();

        var updatedDetails = await _client.GetStringAsync($"/Organizations/Details/{id}");

        Assert.Contains("Updated Integration Test LLC", updatedDetails);
        Assert.Contains("Updated organization description", updatedDetails);
        Assert.Contains("20 Test Street", updatedDetails);
        Assert.Contains("updated@example.com", updatedDetails);

        // Delete
        (await PostFormAsync($"/Organizations/Delete/{id}", []))
            .EnsureSuccessStatusCode();

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _client.GetAsync($"/Organizations/Details/{id}")).StatusCode);
    }
}