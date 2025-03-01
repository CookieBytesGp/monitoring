using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Persistence.Page
{
    public class PageRepository : IPageRepository
    {
        private readonly DatabaseContext _context;

        public PageRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Domain.Aggregates.Page.Page> GetByIdAsync(Guid id)
        {
            return await _context.Pages.FindAsync(id);
        }

        public async Task<IEnumerable<Domain.Aggregates.Page.Page>> GetAllAsync()
        {
            return await _context.Pages.ToListAsync();
        }

        public async Task AddAsync(Domain.Aggregates.Page.Page page)
        {
            await _context.Pages.AddAsync(page);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Domain.Aggregates.Page.Page page)
        {
            _context.Pages.Update(page);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var page = await GetByIdAsync(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();
            }
        }
    }
}
