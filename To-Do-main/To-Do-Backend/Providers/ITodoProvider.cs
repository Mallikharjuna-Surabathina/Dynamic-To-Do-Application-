using TodoAppAPI.Models;

namespace TodoAppAPI.Providers
{
    public interface ITodoProvider
    {
        Task<List<Todo>> GetTodosAsync(string search);     // Get todos with optional search
        Task AddTodoAsync(Todo todo);                      // Add a new todo
        Task UpdateTodoAsync(Todo todo);                   // Update an existing todo
        Task DeleteTodoAsync(int id);                      // Delete a todo by id
    }
}
