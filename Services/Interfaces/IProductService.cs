using KASHOP.DTO;
using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;

namespace KASHOP.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> GetProductByIdAsync(int id);
        Task<List<ProductListResponse>> GetAllProductsAsync();
        Task<PagedResponse<ProductListResponse>> GetProductsPagedAsync(ProductFilterRequest filter);
        Task<ProductResponse> CreateProductAsync(ProductRequest request);
        Task<ProductResponse> UpdateProductAsync(ProductUpdateRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ToggleFeaturedAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
        Task<List<ProductListResponse>> GetFeaturedProductsAsync();
        Task<List<ProductListResponse>> GetProductsByCategoryAsync(int categoryId);
    }
}