<template>
  <div class="offers-view">
    <!-- Header -->
    <header class="header">
      <div class="header-content">
        <div class="header-title">
          <h1>🎯 Monitor de Ofertas</h1>
          <p class="header-subtitle">Encontre as melhores ofertas em tempo real</p>
        </div>
        <div class="header-actions">
          <button 
            @click="handleRefresh" 
            :disabled="loading"
            class="action-btn refresh-btn"
            title="Atualizar ofertas"
          >
            <svg viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M10 3v4m0 0v4m0-4h4m-4 0H6m8 7a7 7 0 11-14 0 7 7 0 0114 0z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <span>Atualizar</span>
          </button>
          <button 
            v-if="hasOffers"
            @click="handleDeleteAll" 
            :disabled="loading"
            class="action-btn delete-btn"
            title="Deletar todas as ofertas"
          >
            <svg viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M3 6h14m-2 0v9a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <span>Limpar Tudo</span>
          </button>
        </div>
      </div>
    </header>

    <!-- Scrape Form -->
    <ScrapeForm 
      :loading="loading"
      @scrape="handleScrape"
      ref="scrapeFormRef"
    />

    <!-- Error Message -->
    <transition name="fade">
      <div v-if="error" class="error-message">
        <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" fill="currentColor"/>
        </svg>
        {{ error }}
      </div>
    </transition>

    <!-- Loading State -->
    <div v-if="loading && !hasOffers" class="loading-state">
      <div class="spinner-large"></div>
      <p>Carregando ofertas...</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="!hasOffers" class="empty-state">
      <div class="empty-icon">📦</div>
      <h2>Nenhuma oferta encontrada</h2>
      <p>Use o formulário acima para solicitar um scraping e começar a monitorar ofertas.</p>
    </div>

    <!-- Main Content -->
    <div v-else class="main-content">
      <!-- Stats -->
      <OffersStats 
        :offers="offers"
        :filtered-offers="filteredOffers"
      />

      <!-- Search and Filters -->
      <div class="filters-section">
        <div class="search-wrapper">
          <SearchBar @search="handleSearch" ref="searchBarRef" />
        </div>
        <div class="filter-wrapper">
          <FilterPanel
            :stores="availableStores"
            :categories="availableCategories"
            @filters-change="handleFiltersChange"
            @sort-change="handleSortChange"
          />
        </div>
      </div>

      <!-- Results Info -->
      <div v-if="filteredCount !== offersCount" class="results-info">
        <span class="results-count">
          Mostrando <strong>{{ filteredCount }}</strong> de <strong>{{ offersCount }}</strong> ofertas
        </span>
        <button @click="clearAllFilters" class="clear-filters-btn">
          Limpar filtros
        </button>
      </div>

      <!-- Offers Grid -->
      <transition-group name="list" tag="div" class="offers-grid">
        <OfferCard
          v-for="offer in filteredOffers"
          :key="offer.id"
          :offer="offer"
          :loading="loading"
          @delete="handleDelete"
        />
      </transition-group>

      <!-- No Results -->
      <div v-if="filteredCount === 0 && hasOffers" class="no-results">
        <div class="no-results-icon">🔍</div>
        <h3>Nenhuma oferta encontrada</h3>
        <p>Tente ajustar os filtros ou a busca para encontrar mais resultados.</p>
        <button @click="clearAllFilters" class="clear-filters-btn">
          Limpar todos os filtros
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOffers } from '../composables/useOffers';
import type { FilterState, SortOption } from '../composables/useOffers';
import OfferCard from '../components/OfferCard.vue';
import ScrapeForm from '../components/ScrapeForm.vue';
import SearchBar from '../components/SearchBar.vue';
import FilterPanel from '../components/FilterPanel.vue';
import OffersStats from '../components/OffersStats.vue';

const {
  offers,
  loading,
  error,
  hasOffers,
  offersCount,
  filteredOffers,
  filteredCount,
  availableStores,
  availableCategories,
  loadOffers,
  deleteOffer,
  deleteAllOffers,
  requestScraping,
  setSearchQuery,
  setFilters,
  setSortOption,
} = useOffers();

const scrapeFormRef = ref<InstanceType<typeof ScrapeForm> | null>(null);
const searchBarRef = ref<InstanceType<typeof SearchBar> | null>(null);

onMounted(() => {
  loadOffers();
});

const handleRefresh = async () => {
  await loadOffers();
};

const handleDelete = async (id: string) => {
  if (!confirm('Tem certeza que deseja deletar esta oferta?')) {
    return;
  }
  
  try {
    await deleteOffer(id);
  } catch (err) {
    console.error('Erro ao deletar oferta:', err);
  }
};

const handleDeleteAll = async () => {
  if (!confirm('Tem certeza que deseja deletar TODAS as ofertas?')) {
    return;
  }
  
  try {
    await deleteAllOffers();
  } catch (err) {
    console.error('Erro ao deletar todas as ofertas:', err);
  }
};

const handleScrape = async (url: string) => {
  try {
    const message = await requestScraping(url);
    scrapeFormRef.value?.showMessage(message, 'success');
  } catch (err) {
    const errorMessage = err instanceof Error ? err.message : 'Erro ao solicitar scraping';
    scrapeFormRef.value?.showMessage(errorMessage, 'error');
  }
};

const handleSearch = (query: string) => {
  setSearchQuery(query);
};

const handleFiltersChange = (filters: FilterState) => {
  setFilters(filters);
};

const handleSortChange = (sort: SortOption) => {
  setSortOption(sort);
};

const clearAllFilters = () => {
  setSearchQuery('');
  setFilters({
    stores: [],
    categories: [],
    minPrice: null,
    maxPrice: null,
  });
  searchBarRef.value?.clear();
};
</script>

<style scoped>
.offers-view {
  width: 80%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 1.5rem;
  min-height: 100vh;
}

/* Header */
.header {
  margin-bottom: 1.5rem;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 1rem;
  width: 100%;
}

.header-title {
  flex: 1;
  min-width: 0;
}

.header-title h1 {
  margin: 0 0 0.5rem 0;
  font-size: 2rem;
  font-weight: 800;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  line-height: 1.2;
  word-wrap: break-word;
  overflow-wrap: break-word;
  hyphens: auto;
}

.header-subtitle {
  margin: 0;
  font-size: 1rem;
  color: #6b7280;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
  align-items: center;
  flex-shrink: 0;
  min-width: fit-content;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem 1.25rem;
  border: none;
  border-radius: 10px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  white-space: nowrap;
  min-width: fit-content;
  line-height: 1;
}

.action-btn svg {
  flex-shrink: 0;
  width: 18px;
  height: 18px;
  display: block;
  overflow: visible;
}

.action-btn span {
  white-space: nowrap;
}

.refresh-btn {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
}

.refresh-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.delete-btn {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
}

.delete-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* Error Message */
.error-message {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: linear-gradient(135deg, rgba(239, 68, 68, 0.1) 0%, rgba(220, 38, 38, 0.1) 100%);
  border: 2px solid rgba(239, 68, 68, 0.3);
  color: #dc2626;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  margin-bottom: 2rem;
  font-weight: 500;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* Loading State */
.loading-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #6b7280;
}

.spinner-large {
  width: 48px;
  height: 48px;
  border: 4px solid #e5e7eb;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-state h2 {
  margin: 0 0 0.5rem 0;
  font-size: 1.5rem;
  color: #1f2937;
}

.empty-state p {
  margin: 0;
  color: #6b7280;
}

/* Main Content */
.main-content {
  animation: fadeIn 0.5s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Filters Section */
.filters-section {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 1.25rem;
  margin-bottom: 1.5rem;
  width: 100%;
  box-sizing: border-box;
}

.search-wrapper {
  grid-column: 1;
  min-width: 0;
  width: 100%;
}

.filter-wrapper {
  grid-column: 2;
  min-width: 0;
  width: 100%;
}

/* Results Info */
.results-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.25rem;
  background: white;
  border-radius: 12px;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  width: 100%;
  box-sizing: border-box;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.results-count {
  color: #6b7280;
  font-size: 0.875rem;
}

.results-count strong {
  color: #1f2937;
  font-weight: 600;
}

.clear-filters-btn {
  background: transparent;
  border: 2px solid #e5e7eb;
  color: #6b7280;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.clear-filters-btn:hover {
  background: #f3f4f6;
  border-color: #d1d5db;
  color: #374151;
}

/* Offers Grid */
.offers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.25rem;
  margin-bottom: 1.5rem;
  width: 100%;
  box-sizing: border-box;
}

.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}

.list-enter-from {
  opacity: 0;
  transform: scale(0.9) translateY(20px);
}

.list-leave-to {
  opacity: 0;
  transform: scale(0.9) translateY(-20px);
}

.list-move {
  transition: transform 0.3s ease;
}

/* No Results */
.no-results {
  text-align: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.no-results-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.no-results h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.5rem;
  color: #1f2937;
}

.no-results p {
  margin: 0 0 1.5rem 0;
  color: #6b7280;
}

/* Responsive */
@media (max-width: 1024px) {
  .offers-view {
    width: 90%;
    padding: 1.25rem;
  }

  .filters-section {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .filter-wrapper {
    grid-column: 1;
  }

  .offers-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
}

@media (max-width: 768px) {
  .offers-view {
    width: 95%;
    padding: 1rem;
  }

  .header-title h1 {
    font-size: 1.75rem;
  }

  .header-content {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    width: 100%;
    justify-content: stretch;
  }

  .action-btn {
    flex: 1;
    justify-content: center;
    min-width: 0;
  }

  .filters-section {
    gap: 1rem;
  }

  .offers-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .results-info {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }

  .clear-filters-btn {
    width: 100%;
  }

  .price-range {
    flex-wrap: wrap;
  }

  .price-input {
    min-width: 120px;
  }
}
</style>
