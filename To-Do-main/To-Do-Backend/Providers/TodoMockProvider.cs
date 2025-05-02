using TodoAppAPI.Models;

namespace TodoAppAPI.Providers
{
    public class TodoMockProvider : ITodoProvider
    {
        private readonly List<Todo> _todos = new List<Todo>();

        public async Task<List<Todo>> GetTodosAsync(string search)
        {
            return await Task.FromResult(_todos.Where(t => t.Title.Contains(search)).ToList());
        }

        public async Task AddTodoAsync(Todo todo)
        {
            _todos.Add(todo);
            await Task.CompletedTask;
        }

        public async Task UpdateTodoAsync(Todo todo)
        {
            var existingTodo = _todos.FirstOrDefault(t => t.Id == todo.Id);
            if (existingTodo != null)
            {
                existingTodo.Title = todo.Title;
                existingTodo.Description = todo.Description;
                existingTodo.IsCompleted = todo.IsCompleted;
                await Task.CompletedTask;
            }
        }

        public async Task DeleteTodoAsync(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                _todos.Remove(todo);
                await Task.CompletedTask;
            }
        }
    }
}
