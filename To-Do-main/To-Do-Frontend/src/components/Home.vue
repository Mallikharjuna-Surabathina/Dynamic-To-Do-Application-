<template>
  <div class="container">
    <h1 class="header">Todo Management</h1>

    <div class="section-1">
      <div class="top-bar">
        <!-- Search Section -->
        <div class="search-bar">
          <input
            v-model="search"
            type="text"
            placeholder="Search Todos..."
            @input="debouncedSearch"
          />
          <!-- <button @click="applySearch" class="button">Search</button> -->
        </div>

        <!-- Filters Section -->
        <div class="filters">
          <h3>Filters :</h3>
          <select v-model="priority">
            <option value="">All Priorities</option>
            <option value="1">Priority 1</option>
            <option value="2">Priority 2</option>
            <option value="3">Priority 3</option>
          </select>

          <select v-model="isCompleted">
            <option :value="null">All</option>
            <option :value="true">Completed</option>
            <option :value="false">Pending</option>
          </select>

          <select v-model="sortBy">
            <option value="createdAt">Created At</option>
            <option value="updatedAt">Updated At</option>
            <option value="title">Title</option>
            <option value="priority">Priority</option>
          </select>

          <select v-model="isDesc">
            <option :value="false">Ascending</option>
            <option :value="true">Descending</option>
          </select>

          <button @click="applyFilters" class="button apply-filters">
            Apply Filters
          </button>
          <button @click="resetFilters" class="button reset-filters">
            Reset Filters
          </button>
          <button @click="deleteAllTodos" class="button delete-all">
            Delete All Tasks
          </button>
        </div>
      </div>

      <!-- Todo Form -->
      <div class="todo-form">
        <input v-model="newTodo.title" type="text" placeholder="Title" />

        <textarea
          v-model="newTodo.description"
          placeholder="Description"
        ></textarea>

        <!-- Priority Dropdown -->
        <select v-model.number="newTodo.priority">
          <option disabled value="">Select Priority</option>
          <option v-for="n in 3" :key="n" :value="n">Priority {{ n }}</option>
        </select>

        <!-- Completed Status Dropdown -->
        <select v-model="newTodo.isCompleted">
          <option disabled value="">Select Status</option>
          <option :value="false">Pending</option>
          <option :value="true">Completed</option>
        </select>

        <button @click="saveTodo" class="button add-todo">
          {{ editingTodo ? "Update Todo" : "Add Todo" }}
        </button>

        <button
          v-if="editingTodo"
          @click="cancelEdit"
          class="button cancel-edit"
        >
          Cancel Edit
        </button>
      </div>
    </div>

    <!-- Todo List -->
    <div v-if="todos.length === 0" class="empty">No todos found.</div>
    <div v-else class="todo-container">
      <div v-for="todo in todos" :key="todo.id" class="todo-card">
        <h3 class="todo-title">{{ todo.title }}</h3>
        <p class="todo-description">{{ todo.description }}</p>
        <p class="todo-priority">
          Priority: <span>{{ todo.priority }}</span>
        </p>
        <p class="todo-completed">
          Completed: <span>{{ todo.isCompleted ? "Yes" : "No" }}</span>
        </p>
        <p class="todo-created">
          Created At:
          <span>{{ new Date(todo.createdAt).toLocaleString() }}</span>
        </p>
        <button @click="editTodo(todo)" class="button edit">Edit</button>
        <button @click="deleteTodo(todo.id)" class="button delete">
          Delete
        </button>
        <button
          @click="markAsComplete(todo)"
          class="button mark-complete"
          v-if="!todo.isCompleted"
        >
          Mark as Complete
        </button>
      </div>
    </div>

    <!-- Pagination -->
    <div class="pagination">
      <button @click="prevPage" :disabled="page === 1">Previous</button>
      <span>Page {{ page }}</span>
      <button @click="nextPage" :disabled="todos.length < pageSize">
        Next
      </button>
    </div>
  </div>
</template>

<script>
import api from "../services/api"; // your API service
import debounce from "lodash.debounce"; // Import lodash debounce

export default {
  name: "Home",
  data() {
    return {
      todos: [],
      newTodo: {
        title: "",
        description: "",
        priority: "", // Set default priority to empty
        isCompleted: "", // Set default completed status to empty
      },
      editingTodo: null,
      // Filters
      isCompleted: null,
      priority: "",
      search: "",
      sortBy: "createdAt",
      isDesc: true,
      page: 1,
      pageSize: 4,
    };
  },
  mounted() {
    this.fetchTodos();
    this.debouncedSearch = debounce(this.applySearch, 500);
  },
  methods: {
    async fetchTodos() {
      try {
        const params = {
          isCompleted: this.isCompleted,
          priority: this.priority,
          search: this.search,
          sortBy: this.sortBy,
          isDesc: this.isDesc === true ? true : false,
          page: this.page,
          pageSize: this.pageSize,
        };
        const response = await api.get("/", { params });
        this.todos = response.data;
      } catch (error) {
        console.error("Error fetching todos:", error);
      }
    },
    async saveTodo() {
      try {
        if (this.editingTodo) {
          await api.put(`/${this.editingTodo.id}`, this.newTodo);
        } else {
          await api.post("/", this.newTodo);
        }
        this.resetForm();
        this.fetchTodos();
      } catch (error) {
        console.error("Error saving todo:", error);
      }
    },
    editTodo(todo) {
      this.editingTodo = todo;
      this.newTodo = { ...todo };
    },
    cancelEdit() {
      this.resetForm();
    },
    resetForm() {
      this.newTodo = {
        title: "",
        description: "",
        priority: "", // Reset priority to empty
        isCompleted: "", // Reset completed status to empty
      };
      this.editingTodo = null;
    },
    async deleteTodo(id) {
      try {
        await api.delete(`/${id}`);
        this.fetchTodos();
      } catch (error) {
        console.error("Error deleting todo:", error);
      }
    },
    async deleteAllTodos() {
      if (confirm("Are you sure you want to delete all tasks?")) {
        try {
          await api.delete("/delete-all"); // make sure this is the correct endpoint
          this.fetchTodos(); // refresh the list of todos after deletion
        } catch (error) {
          console.error("Error deleting all todos:", error);
        }
      }
    },

    nextPage() {
      this.page++;
      this.fetchTodos();
    },
    prevPage() {
      if (this.page > 1) {
        this.page--;
        this.fetchTodos();
      }
    },
    applyFilters() {
      this.page = 1;
      this.fetchTodos();
    },
    resetFilters() {
      this.priority = "";
      this.isCompleted = null;
      this.sortBy = "createdAt";
      this.isDesc = true;
      this.page = 1;
      this.fetchTodos();
    },
    applySearch() {
      this.page = 1;
      this.fetchTodos();
    },
    // New method to mark a todo as complete
    async markAsComplete(todo) {
      try {
        todo.isCompleted = true;
        await api.put(`/${todo.id}`, todo); // Update the todo status in the backend
        this.fetchTodos(); // Refresh the list
      } catch (error) {
        console.error("Error marking todo as completed:", error);
      }
    },
  },
};
</script>
