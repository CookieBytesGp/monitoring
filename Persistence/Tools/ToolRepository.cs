using Domain.Aggregates.Tools;
using Microsoft.EntityFrameworkCore;
using Persistence.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class ToolRepository : IToolRepository
    {
        private readonly DatabaseContext _context;

        public ToolRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Tool> GetByIdAsync(Guid id)
        {
            return await _context.Tools
                .Include(t => t.Templates)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Tool>> GetAllAsync()
        {
            return await _context.Tools
                .Include(t => t.Templates)
                .ToListAsync();
        }

        public async Task AddAsync(Tool tool)
        {
            await _context.Tools.AddAsync(tool);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tool tool)
        {
            _context.Tools.Update(tool);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var tool = await _context.Tools.FindAsync(id);
            if (tool != null)
            {
                _context.Tools.Remove(tool);
                await _context.SaveChangesAsync();
            }
        }
    }
}
