using AutoMapper;
using KASHOP.DTO;
using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;
using KASHOP.Models;
using KASHOP.Repositories;
using KASHOP.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace KASHOP.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ProductResponse> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return null;

                return _mapper.Map<ProductResponse>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductListResponse>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return _mapper.Map<List<ProductListResponse>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                throw;
            }
        }

        public async Task<PagedResponse<ProductListResponse>> GetProductsPagedAsync(ProductFilterRequest filter)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                
                // Apply filters
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    products = products.Where(p => 
                        p.Name.Contains(filter.SearchTerm) || 
                        p.Description.Contains(filter.SearchTerm));
                }

                if (filter.CategoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == filter.CategoryId.Value);
                }

                if (filter.BrandId.HasValue)
                {
                    products = products.Where(p => p.BrandId == filter.BrandId.Value);
                }

                if (filter.MinPrice.HasValue)
                {
                    products = products.Where(p => p.Price >= filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue)
                {
                    products = products.Where(p => p.Price <= filter.MaxPrice.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    products = products.Where(p => p.IsFeatured == filter.IsFeatured.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    products = products.Where(p => p.IsActive == filter.IsActive.Value);
                }

                // Apply sorting
                products = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDescending ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name),
                    "price" => filter.SortDescending ? products.OrderByDescending(p => p.Price) : products.OrderBy(p => p.Price),
                    "stock" => filter.SortDescending ? products.OrderByDescending(p => p.Stock) : products.OrderBy(p => p.Stock),
                    _ => filter.SortDescending ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name)
                };

                // Pagination
                var totalCount = products.Count();
                var items = products
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var response = new PagedResponse<ProductListResponse>
                {
                    Items = _mapper.Map<List<ProductListResponse>>(items),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged products");
                throw;
            }
        }

        public async Task<ProductResponse> CreateProductAsync(ProductRequest request)
        {
            try
            {
                var product = _mapper.Map<Product>(request);
                
                // Handle image upload
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    product.ImageUrl = await SaveImageAsync(request.ImageFile);
                }

                var createdProduct = await _productRepository.AddAsync(product);
                await _productRepository.SaveAsync();

                return _mapper.Map<ProductResponse>(createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductResponse> UpdateProductAsync(ProductUpdateRequest request)
        {
            try
            {
                var existingProduct = await _productRepository.GetByIdAsync(request.Id);
                if (existingProduct == null)
                    return null;

                _mapper.Map(request, existingProduct);
                
                // Handle image upload if new file provided
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        DeleteImage(existingProduct.ImageUrl);
                    }
                    existingProduct.ImageUrl = await SaveImageAsync(request.ImageFile);
                }

                _productRepository.Update(existingProduct);
                await _productRepository.SaveAsync();

                return _mapper.Map<ProductResponse>(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", request.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return false;

                // Delete associated image
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteImage(product.ImageUrl);
                }

                _productRepository.Delete(product);
                await _productRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleFeaturedAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return false;

                product.IsFeatured = !product.IsFeatured;
                _productRepository.Update(product);
                await _productRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured status for product: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return false;

                product.IsActive = !product.IsActive;
                _productRepository.Update(product);
                await _productRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for product: {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductListResponse>> GetFeaturedProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var featuredProducts = products.Where(p => p.IsFeatured && p.IsActive).ToList();
                return _mapper.Map<List<ProductListResponse>>(featuredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                throw;
            }
        }

        public async Task<List<ProductListResponse>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var categoryProducts = products.Where(p => p.CategoryId == categoryId && p.IsActive).ToList();
                return _mapper.Map<List<ProductListResponse>>(categoryProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {CategoryId}", categoryId);
                throw;
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
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

            return $"/images/products/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}