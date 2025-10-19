using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;

namespace KASHOP.Services.Interfaces
{
    public interface IBrandService
    {
        Task<BrandResponse> GetBrandByIdAsync(int id);
        Task<List<BrandResponse>> GetAllBrandsAsync();
        Task<List<BrandResponse>> GetActiveBrandsAsync();
        Task<BrandResponse> CreateBrandAsync(BrandRequest request);
        Task<BrandResponse> UpdateBrandAsync(int id, BrandRequest request);
        Task<bool> DeleteBrandAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
        Task<int> GetProductCountAsync(int brandId);
    }
}