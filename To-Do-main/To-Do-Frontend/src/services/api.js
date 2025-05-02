// src/services/api.js
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5025/api/todos', // your backend URL
});

export default api;
