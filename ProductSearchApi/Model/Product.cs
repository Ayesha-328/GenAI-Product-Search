using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("brand")]
    public string Brand { get; set; }

    [JsonPropertyName("product_type")]
    public string ProductType { get; set; }

    [JsonPropertyName("item_group_id")]
    public string ItemGroupId { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("size")]
    public string Size { get; set; }

    [JsonPropertyName("availability")]
    public string Availability { get; set; }

    [JsonPropertyName("availability_date")]
    public string AvailabilityDate { get; set; }

    [JsonPropertyName("condition")]
    public string Condition { get; set; }

    [JsonPropertyName("material")]
    public string Material { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; }

    [JsonPropertyName("sale_price")]
    public string SalePrice { get; set; }

    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; }

    [JsonPropertyName("image_link")]
    public string ImageLink { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("mpn")]
    public long? MPN { get; set; }

    [JsonPropertyName("custom_label_0")]
    public string CustomLabel0 { get; set; }

    [JsonPropertyName("custom_label_1")]
    public string CustomLabel1 { get; set; }

    [JsonPropertyName("custom_label_2")]
    public string CustomLabel2 { get; set; }

    [JsonPropertyName("custom_label_3")]
    public string CustomLabel3 { get; set; }

    [JsonPropertyName("custom_label_4")]
    public string CustomLabel4 { get; set; }

    [JsonPropertyName("tax")]
    public string Tax { get; set; }

    [JsonPropertyName("embedding")]
    public List<float> Embedding { get; set; }
}
