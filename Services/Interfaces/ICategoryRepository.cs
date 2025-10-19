using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;

namespace KASHOP.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponse> GetCategoryByIdAsync(int id);
        Task<List<CategoryResponse>> GetAllCategoriesAsync();
        Task<List<CategoryResponse>> GetActiveCategoriesAsync();
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequest request);
        Task<CategoryResponse> UpdateCategoryAsync(int id, CategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
        Task<int> GetProductCountAsync(int categoryId);
    }
}