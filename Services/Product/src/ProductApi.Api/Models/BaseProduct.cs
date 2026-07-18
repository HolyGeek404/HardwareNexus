using MongoDB.Bson.Serialization.Attributes;

namespace ProductApi.Api.Models;

[BsonIgnoreExtraElements]
public class BaseProduct
{
    public required string Name { get; set; }
    public required string Team { get; set; }
    public required string Price { get; set; }
    public string? ProductImg { get; set; }
    public virtual required string Category { get; init; }

    [BsonElement("id")]
    public required string ProductGuid { get; set; }

    public required string Warranty { get; set; }
    public required string ProducerCode { get; set; }
}