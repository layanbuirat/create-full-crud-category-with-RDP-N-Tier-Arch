namespace KASHOP.DTO.Requests
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}