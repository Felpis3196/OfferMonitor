<template>
  <div class="scrape-form">
    <h2>Solicitar Scraping</h2>
    
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="url">URL para scraping</label>
        <input
          id="url"
          v-model="url"
          type="url"
          placeholder="https://www.example.com"
          required
          :disabled="loading"
        />
      </div>
      
      <button 
        type="submit" 
        :disabled="loading || !url.trim()"
        class="submit-btn"
      >
        <span v-if="loading">Processando...</span>
        <span v-else>Solicitar Scraping</span>
      </button>
    </form>
    
    <div v-if="message" class="message" :class="messageType">
      {{ message }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

interface Props {
  loading?: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  scrape: [url: string];
}>();

const url = ref('');
const message = ref('');
const messageType = ref<'success' | 'error'>('success');

const handleSubmit = () => {
  if (!url.value.trim()) return;
  
  message.value = '';
  emit('scrape', url.value.trim());
  
  // Limpa o formulário após um delay
  setTimeout(() => {
    url.value = '';
  }, 1000);
};

const showMessage = (msg: string, type: 'success' | 'error') => {
  message.value = msg;
  messageType.value = type;
  setTimeout(() => {
    message.value = '';
  }, 5000);
};

defineExpose({
  showMessage,
});
</script>

<style scoped>
.scrape-form {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.scrape-form h2 {
  margin: 0 0 1.5rem 0;
  font-size: 1.5rem;
  color: #333;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #374151;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-group input:disabled {
  background: #f3f4f6;
  cursor: not-allowed;
}

.submit-btn {
  width: 100%;
  padding: 0.75rem 1.5rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.submit-btn:hover:not(:disabled) {
  background: #2563eb;
}

.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.message {
  margin-top: 1rem;
  padding: 0.75rem;
  border-radius: 6px;
  font-size: 0.875rem;
}

.message.success {
  background: #d1fae5;
  color: #065f46;
}

.message.error {
  background: #fee2e2;
  color: #991b1b;
}
</style>



