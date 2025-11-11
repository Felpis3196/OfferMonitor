<template>
  <div class="stats-container">
    <div class="stat-card">
      <div class="stat-icon">📦</div>
      <div class="stat-content">
        <div class="stat-value">{{ totalOffers }}</div>
        <div class="stat-label">Total de Ofertas</div>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">💰</div>
      <div class="stat-content">
        <div class="stat-value">R$ {{ formatPrice(averagePrice) }}</div>
        <div class="stat-label">Preço Médio</div>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">🏷️</div>
      <div class="stat-content">
        <div class="stat-value">R$ {{ formatPrice(lowestPrice) }}</div>
        <div class="stat-label">Menor Preço</div>
      </div>
    </div>

    <div class="stat-card">
      <div class="stat-icon">⭐</div>
      <div class="stat-content">
        <div class="stat-value">R$ {{ formatPrice(highestPrice) }}</div>
        <div class="stat-label">Maior Preço</div>
      </div>
    </div>

    <div class="stat-card highlight">
      <div class="stat-icon">🎯</div>
      <div class="stat-content">
        <div class="stat-value">{{ filteredCount }}</div>
        <div class="stat-label">Ofertas Filtradas</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { Offer } from '../types/offer';

interface Props {
  offers: Offer[];
  filteredOffers: Offer[];
}

const props = defineProps<Props>();

const totalOffers = computed(() => props.offers.length);
const filteredCount = computed(() => props.filteredOffers.length);

const averagePrice = computed(() => {
  if (props.filteredOffers.length === 0) return 0;
  const sum = props.filteredOffers.reduce((acc, offer) => acc + offer.price, 0);
  return sum / props.filteredOffers.length;
});

const lowestPrice = computed(() => {
  if (props.filteredOffers.length === 0) return 0;
  return Math.min(...props.filteredOffers.map((offer) => offer.price));
});

const highestPrice = computed(() => {
  if (props.filteredOffers.length === 0) return 0;
  return Math.max(...props.filteredOffers.map((offer) => offer.price));
});

const formatPrice = (price: number): string => {
  return price.toFixed(2).replace('.', ',');
};
</script>

<style scoped>
.stats-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
  width: 100%;
  box-sizing: border-box;
}

.stat-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  padding: 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
  transition: transform 0.2s, box-shadow 0.2s;
  min-width: 0;
  width: 100%;
  box-sizing: border-box;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4);
}

.stat-card:nth-child(2) {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  box-shadow: 0 4px 12px rgba(245, 87, 108, 0.3);
}

.stat-card:nth-child(2):hover {
  box-shadow: 0 6px 16px rgba(245, 87, 108, 0.4);
}

.stat-card:nth-child(3) {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  box-shadow: 0 4px 12px rgba(79, 172, 254, 0.3);
}

.stat-card:nth-child(3):hover {
  box-shadow: 0 6px 16px rgba(79, 172, 254, 0.4);
}

.stat-card:nth-child(4) {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
  box-shadow: 0 4px 12px rgba(67, 233, 123, 0.3);
}

.stat-card:nth-child(4):hover {
  box-shadow: 0 6px 16px rgba(67, 233, 123, 0.4);
}

.stat-card.highlight {
  background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
  box-shadow: 0 4px 12px rgba(250, 112, 154, 0.3);
}

.stat-card.highlight:hover {
  box-shadow: 0 6px 16px rgba(250, 112, 154, 0.4);
}

.stat-icon {
  font-size: 1.75rem;
  opacity: 0.9;
  flex-shrink: 0;
  line-height: 1;
}

.stat-content {
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.25rem;
  line-height: 1.2;
  word-break: break-word;
  overflow-wrap: break-word;
}

.stat-label {
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.9);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  line-height: 1.2;
  word-break: break-word;
}

@media (max-width: 768px) {
  .stats-container {
    grid-template-columns: repeat(2, 1fr);
  }

  .stat-card {
    padding: 1rem;
  }

  .stat-icon {
    font-size: 1.5rem;
  }

  .stat-value {
    font-size: 1.25rem;
  }
}
</style>

