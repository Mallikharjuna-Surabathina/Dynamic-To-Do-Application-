using Microsoft.EntityFrameworkCore;
using TodoAppAPI.Data;
using TodoAppAPI.Models;

namespace TodoAppAPI.Providers
{
    public class TodoDbProvider : ITodoProvider
    {
        private readonly TodoDbContext _context;

        public TodoDbProvider(TodoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Todo>> GetTodosAsync(string search)
        {
            return string.IsNullOrEmpty(search)
                ? await _context.Todos.ToListAsync()
                : await _context.Todos.Where(t => t.Title.Contains(search)).ToListAsync();
        }

        public async Task AddTodoAsync(Todo todo)
        {
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTodoAsync(Todo todo)
        {
            _context.Entry(todo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTodoAsync(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
