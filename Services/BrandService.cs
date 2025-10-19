using AutoMapper;
using KASHOP.DTO.Requests;
using KASHOP.DTO.Responses;
using KASHOP.Models;
using KASHOP.Repositories;
using KASHOP.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace KASHOP.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<BrandService> _logger;

        public BrandService(
            IBrandRepository brandRepository,
            IProductRepository productRepository,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<BrandService> logger)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        public async Task<BrandResponse> GetBrandByIdAsync(int id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);
                return _mapper.Map<BrandResponse>(brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand by ID: {BrandId}", id);
                throw;
            }
        }

        public async Task<List<BrandResponse>> GetAllBrandsAsync()
        {
            try
            {
                var brands = await _brandRepository.GetAllAsync();
                return _mapper.Map<List<BrandResponse>>(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                throw;
            }
        }

        public async Task<List<BrandResponse>> GetActiveBrandsAsync()
        {
            try
            {
                var brands = await _brandRepository.GetAllAsync();
                var activeBrands = brands.Where(b => b.IsActive).ToList();
                return _mapper.Map<List<BrandResponse>>(activeBrands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active brands");
                throw;
            }
        }

        public async Task<BrandResponse> CreateBrandAsync(BrandRequest request)
        {
            try
            {
                var brand = _mapper.Map<Brand>(request);
                
                if (request.LogoFile != null && request.LogoFile.Length > 0)
                {
                    brand.LogoUrl = await SaveLogoAsync(request.LogoFile);
                }

                var createdBrand = await _brandRepository.AddAsync(brand);
                await _brandRepository.SaveAsync();

                return _mapper.Map<BrandResponse>(createdBrand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                throw;
            }
        }

        public async Task<BrandResponse> UpdateBrandAsync(int id, BrandRequest request)
        {
            try
            {
                var existingBrand = await _brandRepository.GetByIdAsync(id);
                if (existingBrand == null)
                    return null;

                _mapper.Map(request, existingBrand);
                
                if (request.LogoFile != null && request.LogoFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existingBrand.LogoUrl))
                    {
                        DeleteLogo(existingBrand.LogoUrl);
                    }
                    existingBrand.LogoUrl = await SaveLogoAsync(request.LogoFile);
                }

                _brandRepository.Update(existingBrand);
                await _brandRepository.SaveAsync();

                return _mapper.Map<BrandResponse>(existingBrand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand: {BrandId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);
                if (brand == null)
                    return false;

                // Check if brand has products
                var products = await _productRepository.GetAllAsync();
                var brandProducts = products.Where(p => p.BrandId == id).Any();
                if (brandProducts)
                {
                    throw new InvalidOperationException("Cannot delete brand that has products.");
                }

                if (!string.IsNullOrEmpty(brand.LogoUrl))
                {
                    DeleteLogo(brand.LogoUrl);
                }

                _brandRepository.Delete(brand);
                await _brandRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand: {BrandId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var brand = await _brandRepository.GetByIdAsync(id);
                if (brand == null)
                    return false;

                brand.IsActive = !brand.IsActive;
                _brandRepository.Update(brand);
                await _brandRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for brand: {BrandId}", id);
                throw;
            }
        }

        public async Task<int> GetProductCountAsync(int brandId)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return products.Count(p => p.BrandId == brandId && p.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count for brand: {BrandId}", brandId);
                throw;
            }
        }

        private async Task<string> SaveLogoAsync(IFormFile logoFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "brands");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + logoFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(fileStream);
            }

            return $"/images/brands/{uniqueFileName}";
        }

        private void DeleteLogo(string logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl))
                return;

            var fileName = Path.GetFileName(logoUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "images", "brands", fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}