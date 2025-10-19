using AutoMapper;
using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;
using KASHOP.Models;
using KASHOP.Repositories;
using KASHOP.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace KASHOP.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                return _mapper.Map<CategoryResponse>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<List<CategoryResponse>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return _mapper.Map<List<CategoryResponse>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<List<CategoryResponse>> GetActiveCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var activeCategories = categories.Where(c => c.IsActive).ToList();
                return _mapper.Map<List<CategoryResponse>>(activeCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                throw;
            }
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest request)
        {
            try
            {
                var category = _mapper.Map<Category>(request);
                
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    category.ImageUrl = await SaveImageAsync(request.ImageFile);
                }

                var createdCategory = await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveAsync();

                return _mapper.Map<CategoryResponse>(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, CategoryRequest request)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                    return null;

                _mapper.Map(request, existingCategory);
                
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        DeleteImage(existingCategory.ImageUrl);
                    }
                    existingCategory.ImageUrl = await SaveImageAsync(request.ImageFile);
                }

                _categoryRepository.Update(existingCategory);
                await _categoryRepository.SaveAsync();

                return _mapper.Map<CategoryResponse>(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return false;

                // Check if category has products
                var products = await _productRepository.GetAllAsync();
                var categoryProducts = products.Where(p => p.CategoryId == id).Any();
                if (categoryProducts)
                {
                    throw new InvalidOperationException("Cannot delete category that has products.");
                }

                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    DeleteImage(category.ImageUrl);
                }

                _categoryRepository.Delete(category);
                await _categoryRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return false;

                category.IsActive = !category.IsActive;
                _categoryRepository.Update(category);
                await _categoryRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return products.Count(p => p.CategoryId == categoryId && p.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for category: {CategoryId}", categoryId);
                throw;
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categories");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/categories/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "images", "categories", fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}