using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ProductSearchService _searchService;

    public SearchController(ProductSearchService searchService)
    {
         Console.WriteLine("✅ SearchController constructor called!");
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required.");

        var results = await _searchService.SearchProductsAsync(query);
        return Ok(results);
    }
}
// This controller handles search requests. It uses the ProductSearchService to perform the search based on the query parameter.