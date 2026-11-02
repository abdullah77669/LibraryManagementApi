using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Repository
{
    public class PublisherService : IPublisher
    {
        private readonly ApplicationDBContext _context;

        public PublisherService(ApplicationDBContext context)
        {
            _context = context;
        }

        public Task<List<Publishers>> GetPublishersAsync()
        {
            return _context.Publishers.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        }

        public Task<Publishers?> GetPublisherByIdAsync(int id)
        {
            return _context.Publishers.FindAsync(id).AsTask();
        }

        public async Task<ServiceResult<Publishers>> CreatePublisherAsync(Publishers publisher)
        {
            publisher.PublisherID = 0;
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();
            return new ServiceResult<Publishers>(publisher);
        }

        public async Task<ServiceResult<bool>> UpdatePublisherAsync(int id, Publishers publisher)
        {
            var existing = await _context.Publishers.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);

            existing.Name = publisher.Name;
            existing.Address = publisher.Address;
            existing.ContactNumber = publisher.ContactNumber;
            existing.Email = publisher.Email;
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> DeletePublisherAsync(int id)
        {
            var existing = await _context.Publishers.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);
            if (await _context.Books.AnyAsync(b => b.PublisherID == id))
                return new ServiceResult<bool>(false, 409, "Cannot delete this publisher because books still use it.");

            _context.Publishers.Remove(existing);
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }
    }
}
