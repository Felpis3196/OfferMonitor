<template>
  <div class="offer-card">
    <div class="offer-header">
      <h3 class="offer-title">{{ offer.title }}</h3>
      <button 
        class="delete-btn" 
        @click="$emit('delete', offer.id)"
        :disabled="loading"
        title="Deletar oferta"
      >
        ×
      </button>
    </div>
    
    <div class="offer-body">
      <div class="offer-info">
        <span class="offer-store">{{ offer.store }}</span>
        <span class="offer-category">{{ offer.category }}</span>
      </div>
      
      <div class="offer-price">
        R$ {{ formatPrice(offer.price) }}
      </div>
      
      <a 
        :href="offer.url" 
        target="_blank" 
        rel="noopener noreferrer"
        class="offer-link"
      >
        Ver oferta →
      </a>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Offer } from '../types/offer';

interface Props {
  offer: Offer;
  loading?: boolean;
}

defineProps<Props>();
defineEmits<{
  delete: [id: string];
}>();

const formatPrice = (price: number): string => {
  return price.toFixed(2).replace('.', ',');
};
</script>

<style scoped>
.offer-card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
}

.offer-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

.offer-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.offer-title {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #333;
  flex: 1;
  margin-right: 0.5rem;
}

.delete-btn {
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  width: 28px;
  height: 28px;
  font-size: 1.25rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
  flex-shrink: 0;
}

.delete-btn:hover:not(:disabled) {
  background: #dc2626;
}

.delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.offer-body {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.offer-info {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.offer-store,
.offer-category {
  background: #f3f4f6;
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  font-size: 0.875rem;
  color: #6b7280;
}

.offer-price {
  font-size: 1.5rem;
  font-weight: 700;
  color: #059669;
}

.offer-link {
  color: #3b82f6;
  text-decoration: none;
  font-size: 0.875rem;
  transition: color 0.2s;
}

.offer-link:hover {
  color: #2563eb;
  text-decoration: underline;
}
</style>



