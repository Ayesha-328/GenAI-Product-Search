using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetEnv;


public class ProductSearchService
{
    private readonly HttpClient _httpClient;
    private readonly List<Product> _products;
    private readonly string _openAiApiKey;
    private readonly string _openAiEndpoint;
    private readonly string _gpt4oEndpoint;
    private readonly string _gpt4okey;

    public ProductSearchService(IHttpClientFactory httpClientFactory)
    {
        // Load .env from parent directory
        DotNetEnv.Env.Load(".env");

        _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _openAiEndpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
        _gpt4oEndpoint = Environment.GetEnvironmentVariable("GPT4O_ENDPOINT");
        _gpt4okey = Environment.GetEnvironmentVariable("GPT4O_KEY");

        _httpClient = httpClientFactory.CreateClient();

        // Load the JSON file
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "product_embeddings_cleaned.json");

        var json = File.ReadAllText(path);
        _products = JsonSerializer.Deserialize<List<Product>>(json);

    }

    public async Task<List<ProductDto>> SearchProductsAsync(string query)
{
    query = await NormalizeQueryWithGPT4(query);
    var queryEmbedding = await GetEmbeddingAsync(query);
    if (queryEmbedding == null || queryEmbedding.Count == 0)
    {
        Console.WriteLine("❌ Failed to generate embedding for query.");
        return new List<ProductDto>();
    }

    Console.WriteLine("✅ Query embedding generated.");
    Console.WriteLine($"🔍 Searching for: {query}");

    // Extract max price
    int? maxPrice = null;
    var priceMatch = Regex.Match(query.ToLower(), @"(?:under|below|in|upto|less than)?\s*(\d{3,6})");
    if (priceMatch.Success && int.TryParse(priceMatch.Groups[1].Value, out int parsedPrice))
        maxPrice = parsedPrice;

    var priceWords = new[] { "under", "below", "in", "upto", "less", "than", "price", "rs", "pkr" };
    var queryWords = query
        .ToLower()
        .Split(new[] { ' ', ',', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
        .Where(w => !priceWords.Contains(w) && !Regex.IsMatch(w, @"^\d+$"))
        .Distinct()
        .ToList();

    Console.WriteLine("Max Price: " + (maxPrice.HasValue ? maxPrice.Value.ToString() : "null"));
    Console.WriteLine($"Search Words: {string.Join(", ", queryWords)}");

    // Step 1: Strict ALL match
    var filteredProducts = FilterProducts(_products, queryWords, maxPrice, matchAllWords: true);

    // Step 2: Fallback to ANY match
    if (filteredProducts.Count == 0)
    {
        Console.WriteLine("⚠️ No strict match found. Falling back to flexible match...");
        filteredProducts = FilterProducts(_products, queryWords, maxPrice, matchAllWords: false);
    }

    Console.WriteLine($"🔎 Filtered products count (before similarity): {filteredProducts.Count}");

    // Step 3: If STILL no products, exit early (don't run similarity on everything)
    if (filteredProducts.Count == 0)
    {
        Console.WriteLine("❌ No products found after filtering.");
        return new List<ProductDto>();
    }

    // Step 4: Cosine similarity filtering
    var scoredResults = filteredProducts
        .Select(p => new
        {
            Product = p,
            Score = CosineSimilarity(queryEmbedding, p.Embedding)
        })
        .Where(x => x.Score >= 0.75)
        .OrderByDescending(x => x.Score)
        .Take(20)
        .Select(x => MapToDto(x.Product))
        .ToList();

    return scoredResults;
}


    private async Task<List<float>> GetEmbeddingAsync(string input)
    {
        var payload = new { input = input };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", _openAiApiKey);

        var response = await _httpClient.PostAsync(_openAiEndpoint, content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(result);
        var vector = json.RootElement.GetProperty("data")[0].GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToList();
        // Log the embedding vector for debugging
        Console.WriteLine("Query Embedding:");
        // Console.WriteLine(string.Join(", ", vector));

        return vector;
    }

    private double CosineSimilarity(List<float> a, List<float> b)
    {
        double dot = 0.0, normA = 0.0, normB = 0.0;
        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
            normA += Math.Pow(a[i], 2);
            normB += Math.Pow(b[i], 2);
        }
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Brand = product.Brand,
            ProductType = product.ProductType,
            ItemGroupId = product.ItemGroupId,
            Gender = product.Gender,
            Color = product.Color,
            Size = product.Size,
            Availability = product.Availability,
            AvailabilityDate = product.AvailabilityDate,
            Condition = product.Condition,
            Material = product.Material,
            Price = product.Price,
            SalePrice = product.SalePrice,
            AgeGroup = product.AgeGroup,
            ImageLink = product.ImageLink,
            Link = product.Link,
            MPN = product.MPN,
            CustomLabel0 = product.CustomLabel0,
            CustomLabel1 = product.CustomLabel1,
            CustomLabel2 = product.CustomLabel2,
            CustomLabel3 = product.CustomLabel3,
            CustomLabel4 = product.CustomLabel4,
            Tax = product.Tax
        };
    }

    private bool TryParsePrice(string priceStr, out int price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(priceStr)) return false;

        // Remove PKR and commas
        var cleaned = new string(priceStr
            .Replace("PKR", "")
            .Replace(",", "")
            .Where(char.IsDigit)
            .ToArray());

        return int.TryParse(cleaned, out price);
    }

    private List<Product> FilterProducts(List<Product> products, List<string> queryWords, int? maxPrice, bool matchAllWords)
{
    List<Product> textMatchedProducts = new();

    foreach (var product in products)
    {
        Dictionary<string, string> keywordMatches = new(); // keyword -> field

        foreach (var qw in queryWords)
        {
            var matchedField = GetMatchingField(product, qw);
            if (matchedField != null)
                keywordMatches[qw] = matchedField;
        }

        bool isMatch = matchAllWords
            ? queryWords.All(qw => keywordMatches.ContainsKey(qw))
            : queryWords.Any(qw => keywordMatches.ContainsKey(qw));

        if (isMatch)
        {
            // 🔍 Print matching info
            Console.WriteLine($"✅ Product ID: {product.Id}");
            foreach (var match in keywordMatches)
            {
                Console.WriteLine($"   - Matched Keyword: '{match.Key}' in Field: {match.Value}");
            }

            textMatchedProducts.Add(product);
        }
    }

    // If nothing matched, return empty list
    if (textMatchedProducts.Count == 0)
        return new List<Product>();

    // Step 2: Apply max price filter if needed
    var finalFiltered = textMatchedProducts
        .Where(p =>
        {
            if (maxPrice.HasValue)
            {
                bool priceOk = TryParsePrice(p.Price, out int price) && price <= maxPrice.Value;
                bool salePriceOk = TryParsePrice(p.SalePrice, out int salePrice) && salePrice <= maxPrice.Value;
                return priceOk || salePriceOk;
            }
            return true;
        })
        .Where(p => p.Embedding != null)
        .ToList();

    return finalFiltered;
}

    private string GetMatchingField(Product p, string keyword)
    {
        keyword = Regex.Escape(keyword.ToLower()); // escape keyword to make it regex-safe

        // Helper function to match whole word using regex
        bool Matches(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            return Regex.IsMatch(input.ToLower(), $@"\b{keyword}\b");
        }

        if (Matches(p.Id)) return "Id";
        if (Matches(p.Title)) return "Title";
        if (Matches(p.Description)) return "Description";
        if (Matches(p.ProductType)) return "ProductType";
        if (Matches(p.Color)) return "Color";
        if (Matches(p.CustomLabel0)) return "CustomLabel0";
        if (Matches(p.CustomLabel1)) return "CustomLabel1";
        if (Matches(p.CustomLabel2)) return "CustomLabel2";
        if (Matches(p.CustomLabel3)) return "CustomLabel3";
        if (Matches(p.CustomLabel4)) return "CustomLabel4";

        return null; // no match
    }

    private async Task<string> NormalizeQueryWithGPT4(string rawQuery)
{
    var payload = new
    {
        messages = new[]
        {
            new
            {
                role = "system",
                content ="You are a search engine query optimizer that: 1. Correct any spelling errors. 2. Return a list of important keywords including: - Corrected terms  - Their synonyms or related category terms and Pakistani clothing items like if someone searches shirts its Pakistani similar clotings are kurta, top, kameez etc. for scarf its dupatta, shawl and if magenta is searched its similar term might be purple that belong to the same color family just like these examples return similar terms 3. Return only space separated keywords. No explanation or extra text."
            },
            new
            {
                role = "user",
                content = $"Only return the corrected version of this product search query without explanations or formatting: \"{rawQuery}\""
            }
        },
        max_tokens = 70,
        temperature = 0.3,
        top_p = 1,
        frequency_penalty = 0,
        presence_penalty = 0
    };

    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("api-key", _gpt4okey);

    var gpt4Url = _gpt4oEndpoint;

    var response = await _httpClient.PostAsync(gpt4Url, content);
    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadAsStringAsync();
    var json = JsonDocument.Parse(responseBody);

    var correctedQuery = json.RootElement
        .GetProperty("choices")[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString();

    Console.WriteLine($"Original Query: {rawQuery}");
    Console.WriteLine($"GPT Corrected Query: {correctedQuery}");

    return correctedQuery.Trim();
}

}




// @"You are a search engine query optimizer that:
// 1. Corrects any spelling errors.
// 2. Returns a list of important keywords including:
//    - Corrected term
//    - Only directly related or synonymous terms that belong to the *same category*.
//    - and if magenta is searched its similar term might be purple, pink, red that belong to the same color family just like these examples return similar colors
// 3. Categories can be pakistani clothing, colors, and fashion accessories. like:
//    - Upper wear: shirt, top, kurta, kameez
//    - Lower wear: shalwar, trousers, pants, jeans
//    - Headwear: scarf, dupatta, shawl
//    - Shoes: sandals, chappal, heels
// 4. For example:
//    - 'scarf' → scarf dupatta shawl
//    - 'kameez' → kameez kurta top
//    - 'shalwar' → shalwar trousers pants
//     - 'magenta' → magenta purple pink red
// 5. Do NOT return keywords from unrelated categories.
//    - Example: Do NOT return 'kurta' for 'shalwar'
// 6. Output a single line of space-separated keywords without any explanation."