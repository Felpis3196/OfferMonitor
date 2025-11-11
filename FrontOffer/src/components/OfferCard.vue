<template>
  <div class="offer-card">
    <div class="card-gradient"></div>
    <div class="card-content">
      <div class="offer-header">
        <div class="offer-badges">
          <span class="badge store-badge">{{ offer.store }}</span>
          <span class="badge category-badge">{{ offer.category }}</span>
        </div>
        <button 
          class="delete-btn" 
          @click="$emit('delete', offer.id)"
          :disabled="loading"
          title="Deletar oferta"
        >
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M12 4L4 12M4 4l8 8" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
      
      <h3 class="offer-title">{{ offer.title }}</h3>
      
      <div class="offer-price-section">
        <div class="price-label">Preço</div>
        <div class="offer-price">R$ {{ formatPrice(offer.price) }}</div>
      </div>
      
      <a 
        :href="offer.url" 
        target="_blank" 
        rel="noopener noreferrer"
        class="offer-link"
      >
        <span>Ver Oferta</span>
        <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
          <path d="M7.5 15L12.5 10L7.5 5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
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
  position: relative;
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.offer-card:hover {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
}

.card-gradient {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 50%, #f093fb 100%);
  opacity: 0;
  transition: opacity 0.3s;
}

.offer-card:hover .card-gradient {
  opacity: 1;
}

.card-content {
  padding: 1.5rem;
  position: relative;
  z-index: 1;
}

.offer-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
  gap: 0.75rem;
}

.offer-badges {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  flex: 1;
}

.badge {
  padding: 0.375rem 0.75rem;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.store-badge {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.category-badge {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  color: white;
}

.delete-btn {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border: none;
  border-radius: 8px;
  width: 32px;
  height: 32px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  flex-shrink: 0;
}

.delete-btn:hover:not(:disabled) {
  background: #ef4444;
  color: white;
  transform: rotate(90deg);
}

.delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.offer-title {
  margin: 0 0 1.25rem 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.offer-price-section {
  margin-bottom: 1.25rem;
  padding: 1rem;
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border-radius: 12px;
}

.price-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #0369a1;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.25rem;
}

.offer-price {
  font-size: 2rem;
  font-weight: 700;
  background: linear-gradient(135deg, #059669 0%, #10b981 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  line-height: 1;
}

.offer-link {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  text-decoration: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.875rem;
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.3);
}

.offer-link:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
}

.offer-link svg {
  transition: transform 0.2s;
}

.offer-link:hover svg {
  transform: translateX(4px);
}
</style>




