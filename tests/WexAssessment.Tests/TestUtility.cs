

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using WexAssessment.Api.DTOs;

namespace WexAssessment.Tests;

public static class TestUtility
{
    public static void AssertBodyMatchesExpected(JsonElement body, object expected)
    {
        var expectedJson = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(expected));
        foreach (var property in expectedJson.EnumerateObject())
        {
            var actual = body.GetProperty(property.Name);

            if (property.Value.ValueKind == JsonValueKind.Number)
                actual.GetDecimal().Should().Be(property.Value.GetDecimal());
            else
                actual.ToString().Should().Be(property.Value.ToString());
        }
    }

    public static async Task<Guid> CreateCardAsync(HttpClient _client, decimal creditLimit)
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCard(creditLimit));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }
}