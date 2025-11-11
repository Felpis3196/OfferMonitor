<template>
  <div class="search-bar">
    <div class="search-icon">🔍</div>
    <input
      v-model="searchQuery"
      type="text"
      placeholder="Buscar ofertas por título, loja ou categoria..."
      class="search-input"
      @input="handleInput"
    />
    <button
      v-if="searchQuery"
      @click="clearSearch"
      class="clear-btn"
      title="Limpar busca"
    >
      ×
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const emit = defineEmits<{
  search: [query: string];
}>();

const searchQuery = ref('');

const handleInput = () => {
  emit('search', searchQuery.value);
};

const clearSearch = () => {
  searchQuery.value = '';
  emit('search', '');
};

// Permite controle externo
defineExpose({
  clear: clearSearch,
});
</script>

<style scoped>
.search-bar {
  position: relative;
  display: flex;
  align-items: center;
  background: white;
  border-radius: 12px;
  padding: 0.75rem 1rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: box-shadow 0.3s ease;
  width: 100%;
  box-sizing: border-box;
  min-width: 0;
}

.search-bar:focus-within {
  box-shadow: 0 4px 16px rgba(59, 130, 246, 0.2);
}

.search-icon {
  font-size: 1.25rem;
  margin-right: 0.75rem;
  opacity: 0.6;
}

.search-input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 1rem;
  background: transparent;
  color: #1f2937;
  min-width: 0;
  width: 100%;
}

.search-input::placeholder {
  color: #9ca3af;
}

.clear-btn {
  background: #f3f4f6;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6b7280;
  transition: all 0.2s;
  margin-left: 0.5rem;
}

.clear-btn:hover {
  background: #e5e7eb;
  color: #374151;
}
</style>

