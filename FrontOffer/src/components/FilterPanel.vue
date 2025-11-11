<template>
  <div class="filter-panel">
    <div class="filter-header">
      <h3>Filtros</h3>
      <button v-if="hasActiveFilters" @click="clearAllFilters" class="clear-all-btn">
        Limpar
      </button>
    </div>

    <div class="filter-section">
      <label class="filter-label">Loja</label>
      <div class="filter-options">
        <button
          v-for="store in stores"
          :key="store"
          @click="toggleStore(store)"
          :class="['filter-chip', { active: selectedStores.includes(store) }]"
        >
          {{ store }}
        </button>
      </div>
    </div>

    <div class="filter-section">
      <label class="filter-label">Categoria</label>
      <div class="filter-options">
        <button
          v-for="category in categories"
          :key="category"
          @click="toggleCategory(category)"
          :class="['filter-chip', { active: selectedCategories.includes(category) }]"
        >
          {{ category }}
        </button>
      </div>
    </div>

    <div class="filter-section">
      <label class="filter-label">Faixa de Preço</label>
      <div class="price-range">
        <input
          v-model.number="minPrice"
          type="number"
          placeholder="Mín"
          class="price-input"
          min="0"
          step="0.01"
          @input="handlePriceChange"
        />
        <span class="price-separator">-</span>
        <input
          v-model.number="maxPrice"
          type="number"
          placeholder="Máx"
          class="price-input"
          min="0"
          step="0.01"
          @input="handlePriceChange"
        />
      </div>
    </div>

    <div class="filter-section">
      <label class="filter-label">Ordenar por</label>
      <select v-model="sortBy" @change="handleSortChange" class="sort-select">
        <option value="price-asc">Preço: Menor para Maior</option>
        <option value="price-desc">Preço: Maior para Menor</option>
        <option value="title-asc">Título: A-Z</option>
        <option value="title-desc">Título: Z-A</option>
        <option value="store-asc">Loja: A-Z</option>
      </select>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { FilterState, SortOption } from '../composables/useOffers';

interface Props {
  stores: string[];
  categories: string[];
}

const props = defineProps<Props>();

const emit = defineEmits<{
  filtersChange: [filters: FilterState];
  sortChange: [sort: SortOption];
}>();

const selectedStores = ref<string[]>([]);
const selectedCategories = ref<string[]>([]);
const minPrice = ref<number | null>(null);
const maxPrice = ref<number | null>(null);
const sortBy = ref<string>('price-asc');

const hasActiveFilters = computed(() => {
  return (
    selectedStores.value.length > 0 ||
    selectedCategories.value.length > 0 ||
    minPrice.value !== null ||
    maxPrice.value !== null
  );
});

const toggleStore = (store: string) => {
  const index = selectedStores.value.indexOf(store);
  if (index > -1) {
    selectedStores.value.splice(index, 1);
  } else {
    selectedStores.value.push(store);
  }
  emitFilters();
};

const toggleCategory = (category: string) => {
  const index = selectedCategories.value.indexOf(category);
  if (index > -1) {
    selectedCategories.value.splice(index, 1);
  } else {
    selectedCategories.value.push(category);
  }
  emitFilters();
};

const handlePriceChange = () => {
  emitFilters();
};

const handleSortChange = () => {
  const [field, direction] = sortBy.value.split('-') as [string, string];
  emit('sortChange', {
    field: field as 'price' | 'title' | 'store',
    direction: direction as 'asc' | 'desc',
  });
};

const emitFilters = () => {
  emit('filtersChange', {
    stores: selectedStores.value,
    categories: selectedCategories.value,
    minPrice: minPrice.value,
    maxPrice: maxPrice.value,
  });
};

const clearAllFilters = () => {
  selectedStores.value = [];
  selectedCategories.value = [];
  minPrice.value = null;
  maxPrice.value = null;
  sortBy.value = 'price-asc';
  emitFilters();
  handleSortChange();
};
</script>

<style scoped>
.filter-panel {
  background: white;
  border-radius: 12px;
  padding: 1.25rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  width: 100%;
  box-sizing: border-box;
}

.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.25rem;
  gap: 0.75rem;
}

.filter-header h3 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
}

.clear-all-btn {
  background: transparent;
  border: 1px solid #e5e7eb;
  color: #6b7280;
  padding: 0.375rem 0.75rem;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
  flex-shrink: 0;
}

.clear-all-btn:hover {
  background: #f3f4f6;
  border-color: #d1d5db;
  color: #374151;
}

.filter-section {
  margin-bottom: 1.25rem;
}

.filter-section:last-child {
  margin-bottom: 0;
}

.filter-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.75rem;
}

.filter-options {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  width: 100%;
}

.filter-chip {
  background: #f3f4f6;
  border: 2px solid transparent;
  color: #6b7280;
  padding: 0.5rem 0.875rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
  flex-shrink: 0;
}

.filter-chip:hover {
  background: #e5e7eb;
  color: #374151;
}

.filter-chip.active {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border-color: #2563eb;
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
}

.price-range {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
}

.price-input {
  flex: 1;
  min-width: 0;
  padding: 0.5rem 0.75rem;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.875rem;
  transition: border-color 0.2s;
  box-sizing: border-box;
  width: 100%;
  max-width: 100%;
}

.price-input:focus {
  outline: none;
  border-color: #3b82f6;
}

.price-separator {
  color: #9ca3af;
  font-weight: 500;
  flex-shrink: 0;
  padding: 0 0.25rem;
}

.sort-select {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.875rem;
  background: white;
  color: #1f2937;
  cursor: pointer;
  transition: border-color 0.2s;
  box-sizing: border-box;
  max-width: 100%;
}

.sort-select:focus {
  outline: none;
  border-color: #3b82f6;
}
</style>

