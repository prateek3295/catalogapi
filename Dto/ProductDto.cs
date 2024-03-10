namespace Catalog.API.Dto
{
    public class ProductDto
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ImagePreSignedUrl { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public int Rating { get; set; }
        public string Brand { get; set; }
    }
}
