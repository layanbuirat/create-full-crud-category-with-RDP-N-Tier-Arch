namespace KASHOP.DTO.Responses
{
    public class ProductListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Active" : "Inactive";
        public string FeaturedStatus => IsFeatured ? "Featured" : "Regular";
    }
}