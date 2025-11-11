<template>
  <div class="scrape-form">
    <div class="form-header">
      <div class="form-icon">🚀</div>
      <div>
        <h2>Solicitar Scraping</h2>
        <p class="form-subtitle">Adicione uma URL para iniciar o monitoramento de ofertas</p>
      </div>
    </div>
    
    <form @submit.prevent="handleSubmit" class="scrape-form-content">
      <div class="form-group">
        <label for="url">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M11 3a1 1 0 100 2h2.586l-6.293 6.293a1 1 0 101.414 1.414L15 6.414V9a1 1 0 102 0V4a1 1 0 00-1-1h-5z" fill="currentColor"/>
            <path d="M5 5a2 2 0 00-2 2v8a2 2 0 002 2h8a2 2 0 002-2v-3a1 1 0 10-2 0v3H5V7h3a1 1 0 000-2H5z" fill="currentColor"/>
          </svg>
          URL para scraping
        </label>
        <input
          id="url"
          v-model="url"
          type="url"
          placeholder="https://www.amazon.com.br/..."
          required
          :disabled="loading"
          class="url-input"
        />
      </div>
      
      <button 
        type="submit" 
        :disabled="loading || !url.trim()"
        class="submit-btn"
      >
        <span v-if="loading" class="btn-content">
          <svg class="spinner" width="20" height="20" viewBox="0 0 20 20">
            <circle cx="10" cy="10" r="8" stroke="currentColor" stroke-width="2" fill="none" stroke-dasharray="31.416" stroke-dashoffset="31.416">
              <animate attributeName="stroke-dasharray" dur="2s" values="0 31.416;15.708 15.708;0 31.416;0 31.416" repeatCount="indefinite"/>
              <animate attributeName="stroke-dashoffset" dur="2s" values="0;-15.708;-31.416;-31.416" repeatCount="indefinite"/>
            </circle>
          </svg>
          Processando...
        </span>
        <span v-else class="btn-content">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
            <path d="M10 2a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V3a1 1 0 011-1z" fill="currentColor"/>
          </svg>
          Iniciar Scraping
        </span>
      </button>
    </form>
    
    <transition name="message">
      <div v-if="message" class="message" :class="messageType">
        <svg v-if="messageType === 'success'" width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" fill="currentColor"/>
        </svg>
        <svg v-else width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" fill="currentColor"/>
        </svg>
        <span>{{ message }}</span>
      </div>
    </transition>
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
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
  margin-bottom: 1.5rem;
  color: white;
  width: 100%;
  box-sizing: border-box;
}

.form-header {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.form-icon {
  font-size: 2.5rem;
  line-height: 1;
}

.form-header h2 {
  margin: 0 0 0.5rem 0;
  font-size: 1.75rem;
  font-weight: 700;
  color: white;
}

.form-subtitle {
  margin: 0;
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.9);
  opacity: 0.9;
}

.scrape-form-content {
  background: white;
  border-radius: 12px;
  padding: 1.25rem;
  width: 100%;
  box-sizing: border-box;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
}

.form-group label svg {
  color: #667eea;
}

.url-input {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  font-size: 1rem;
  transition: all 0.2s;
  box-sizing: border-box;
  color: #1f2937;
  min-width: 0;
  max-width: 100%;
}

.url-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
}

.url-input:disabled {
  background: #f3f4f6;
  cursor: not-allowed;
  opacity: 0.7;
}

.url-input::placeholder {
  color: #9ca3af;
}

.submit-btn {
  width: 100%;
  padding: 1rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.5);
}

.submit-btn:active:not(:disabled) {
  transform: translateY(0);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-content {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.message {
  margin-top: 1rem;
  padding: 1rem;
  border-radius: 10px;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  animation: slideDown 0.3s ease;
}

.message-enter-active {
  animation: slideDown 0.3s ease;
}

.message-leave-active {
  animation: slideUp 0.3s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideUp {
  from {
    opacity: 1;
    transform: translateY(0);
  }
  to {
    opacity: 0;
    transform: translateY(-10px);
  }
}

.message.success {
  background: rgba(16, 185, 129, 0.2);
  color: white;
  border: 1px solid rgba(16, 185, 129, 0.3);
}

.message.error {
  background: rgba(239, 68, 68, 0.2);
  color: white;
  border: 1px solid rgba(239, 68, 68, 0.3);
}

.message svg {
  flex-shrink: 0;
}
</style>




