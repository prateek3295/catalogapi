using Catalog.API.Dto;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using DnsClient.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OpenSearch.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;
        private readonly AWSS3Service _s3Service;
        private readonly OpenSearchService openSearchService;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger, AWSS3Service s3Service, OpenSearchService openSearchService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._s3Service = s3Service;
            this.openSearchService = openSearchService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            
            var productDtoList = new List<ProductDto>();

            var products = await _repository.GetProducts();

            products.ToList().ForEach(product =>
            {
                productDtoList.Add(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImagePreSignedUrl = _s3Service.GetCloudFrontUrl(product.ImageS3Key),
                    Summary = product.Summary,
                    Category = product.Category,
                    Description = product.Description,
                    InStock = product.InStock,
                    Rating = product.Rating,
                    Brand = product.Brand,
                });
            });

            return Ok(productDtoList);
        }

        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductDto>> GetProductById(string id)
        {
            var product = await _repository.GetProduct(id);

            if (product == null)
            {
                _logger.LogError($"Product with id: {id}, not found.");
                return NotFound();
            }

            var response = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImagePreSignedUrl = _s3Service.GetCloudFrontUrl(product.ImageS3Key),
                Summary = product.Summary,
                Category = product.Category,
                Description = product.Description,
                Rating = product.Rating,
                InStock = product.InStock,
                Brand = product.Brand,
                Categories = product.Categories  
            };

            return Ok(response);
        }

        [Route("[action]/{category}", Name = "GetProductByCategory")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(string category)
        {
            var products = await _repository.GetProductByCategory(category);
            return Ok(products);
        }

        [Route("[action]", Name = "GetProductBrands")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetProductBrands(string searchQuery)
        {
            IEnumerable<string> brands = new List<string>();

            if (string.IsNullOrEmpty(searchQuery))
            {
                brands = await _repository.GetProductBrands();
            }
            else
            {
                var filteredProducts = await openSearchService.FetchResultsFromOpenSearch(searchQuery);
                brands = filteredProducts.Select(x => x.Brand).Distinct();
            }
            return Ok(brands);
        }

        [Route("[action]", Name = "Search")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search(string searchQuery)
        {
            var items = await openSearchService.FetchResultsFromOpenSearch(searchQuery);
           
            return Ok(items);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            await _repository.CreateProduct(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateProduct([FromBody] Product product)
        {
            return Ok(await _repository.UpdateProduct(product));
        }

        [HttpDelete("{id:length(24)}", Name = "DeleteProduct")]        
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteProductById(string id)
        {
            return Ok(await _repository.DeleteProduct(id));
        }

        [HttpPost("uploadImageToS3")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var file = Request.Form.Files[0];

                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var key = $"{Guid.NewGuid()}-{file.FileName}";
                var imageUrl = await _s3Service.UploadFileToS3Async(tempFilePath, key);

                return Ok(imageUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
