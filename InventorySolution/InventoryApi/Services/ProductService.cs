using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryApi.Models;
using InventoryApi.DTOs;
using InventoryApi.Repositories;

namespace InventoryApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return Mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return Mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto productCreateDto)
        {
            var product = Mapper.Map<Product>(productCreateDto);
            await _productRepository.AddAsync(product);
            return Mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductCreateDto productUpdateDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            Mapper.Map(productUpdateDto, product);
            await _productRepository.UpdateAsync(product);
            return Mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            await _productRepository.DeleteAsync(product);
            return true;
        }
    }
}