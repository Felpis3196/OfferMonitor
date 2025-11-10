<template>
  <div class="offers-view">
    <header class="header">
      <h1>Monitor de Ofertas</h1>
      <div class="header-actions">
        <button 
          @click="handleRefresh" 
          :disabled="loading"
          class="refresh-btn"
          title="Atualizar ofertas"
        >
          🔄 Atualizar
        </button>
        <button 
          v-if="hasOffers"
          @click="handleDeleteAll" 
          :disabled="loading"
          class="delete-all-btn"
          title="Deletar todas as ofertas"
        >
          🗑️ Limpar Tudo
        </button>
      </div>
    </header>

    <ScrapeForm 
      :loading="loading"
      @scrape="handleScrape"
      ref="scrapeFormRef"
    />

    <div v-if="error" class="error-message">
      ⚠️ {{ error }}
    </div>

    <div v-if="loading && !hasOffers" class="loading">
      Carregando ofertas...
    </div>

    <div v-else-if="!hasOffers" class="empty-state">
      <p>Nenhuma oferta encontrada.</p>
      <p class="empty-hint">Use o formulário acima para solicitar um scraping.</p>
    </div>

    <div v-else class="offers-grid">
      <OfferCard
        v-for="offer in offers"
        :key="offer.id"
        :offer="offer"
        :loading="loading"
        @delete="handleDelete"
      />
    </div>

    <div v-if="hasOffers" class="stats">
      Total de ofertas: {{ offersCount }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOffers } from '../composables/useOffers';
import OfferCard from '../components/OfferCard.vue';
import ScrapeForm from '../components/ScrapeForm.vue';

const {
  offers,
  loading,
  error,
  hasOffers,
  offersCount,
  loadOffers,
  deleteOffer,
  deleteAllOffers,
  requestScraping,
} = useOffers();

const scrapeFormRef = ref<InstanceType<typeof ScrapeForm> | null>(null);

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
</script>

<style scoped>
.offers-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.header h1 {
  margin: 0;
  font-size: 2rem;
  color: #1f2937;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.refresh-btn,
.delete-all-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.refresh-btn {
  background: #3b82f6;
  color: white;
}

.refresh-btn:hover:not(:disabled) {
  background: #2563eb;
}

.delete-all-btn {
  background: #ef4444;
  color: white;
}

.delete-all-btn:hover:not(:disabled) {
  background: #dc2626;
}

.refresh-btn:disabled,
.delete-all-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error-message {
  background: #fee2e2;
  color: #991b1b;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 1.5rem;
}

.loading {
  text-align: center;
  padding: 3rem;
  color: #6b7280;
  font-size: 1.125rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.empty-state p {
  margin: 0.5rem 0;
  color: #6b7280;
}

.empty-hint {
  font-size: 0.875rem;
  color: #9ca3af;
}

.offers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stats {
  text-align: center;
  padding: 1rem;
  background: #f3f4f6;
  border-radius: 6px;
  color: #6b7280;
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .offers-view {
    padding: 1rem;
  }
  
  .header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .offers-grid {
    grid-template-columns: 1fr;
  }
}
</style>



