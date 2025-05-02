using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoAppAPI.Data;
using TodoAppAPI.Models;

namespace TodoAppAPI.Controllers
{
    [Route("api/todos")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoDbContext _context;

        public TodoController(TodoDbContext context)
        {
            _context = context;
        }

        // GET: api/todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos(
            [FromQuery] bool? isCompleted,
            [FromQuery] int? priority,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] bool isDesc = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Todos.AsQueryable();

            // Filtering
            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            // Searching
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "title" => isDesc
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),

                "priority" => isDesc
                    ? query.OrderByDescending(t => t.Priority)
                    : query.OrderBy(t => t.Priority),

                "updatedat" => isDesc
                    ? query.OrderByDescending(t => t.UpdatedAt)
                    : query.OrderBy(t => t.UpdatedAt),

                "createdat" => isDesc
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),

                _ => isDesc
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt)
            };


            // Pagination
            var totalItems = await query.CountAsync();
            var todos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Add metadata to response headers
            Response.Headers.Append("X-Total-Count", totalItems.ToString());

            return Ok(todos);
        }

        // GET: api/todos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            return todo;
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodo(Todo todo)
        {
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
        }

        // PUT: api/todos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo(int id, Todo todo)
        {
            if (id != todo.Id)
                return BadRequest();

            _context.Entry(todo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Todos.Any(e => e.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/todos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE ALL: api/todos/{id}
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllTodos()
        {
            var todos = await _context.Todos.ToListAsync();
            _context.Todos.RemoveRange(todos);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}
