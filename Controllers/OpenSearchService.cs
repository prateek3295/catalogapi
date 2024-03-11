using Catalog.API.Dto;
using Microsoft.Extensions.Configuration;
using OpenSearch.Client;
using OpenSearch.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API;

public class OpenSearchService
{
    private OpenSearchClient _client;
    public OpenSearchService(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public async Task<List<ProductDto>> FetchResultsFromOpenSearch(string searchQuery)
    {
        var settings = new ConnectionSettings(new Uri(Configuration["OpenSearch:Domain"])).
                         BasicAuthentication(Configuration["OpenSearch:User"], Configuration["OpenSearch:Password"]);

        _client = new OpenSearchClient(settings);

        
       var searchResponseLow = _client.Search<ProductDto>(x => x.Index("catalog").Size(500).Query(q => q
                                                                           .MultiMatch(m => m
                                                                           .Fields(fs => fs
                                                                           .Field(p => p.Name)
                                                                           .Field(p => p.Categories)
                                                                           .Field(p => p.Category)
                                                                           .Field(p => p.Brand)
                                                                           .Field(p => p.Summary))
                                                                           .Operator(Operator.Or)
                                                                           .Query(searchQuery)
                                                                            )));

        return searchResponseLow.Documents.ToList();

    }
}
