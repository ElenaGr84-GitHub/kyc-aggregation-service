using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Models;

public sealed record ErrorResponse([property: JsonPropertyName("error")] string Error);