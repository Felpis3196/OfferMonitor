/**
 * Composable para gerenciar estado das ofertas
 */
import { ref, computed } from 'vue';
import { offerService } from '../api/services/offerService';
import type { Offer, OfferInput, ScraperRequest } from '../types/offer';

export function useOffers() {
  const offers = ref<Offer[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  /**
   * Carrega todas as ofertas
   */
  const loadOffers = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      offers.value = await offerService.getAll();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao carregar ofertas';
      console.error('Erro ao carregar ofertas:', err);
    } finally {
      loading.value = false;
    }
  };

  /**
   * Cria uma nova oferta
   */
  const createOffer = async (offerInput: OfferInput) => {
    loading.value = true;
    error.value = null;
    
    try {
      const newOffer = await offerService.create(offerInput);
      offers.value.push(newOffer);
      return newOffer;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao criar oferta';
      console.error('Erro ao criar oferta:', err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  /**
   * Deleta uma oferta
   */
  const deleteOffer = async (id: string) => {
    loading.value = true;
    error.value = null;
    
    try {
      await offerService.deleteById(id);
      offers.value = offers.value.filter(offer => offer.id !== id);
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao deletar oferta';
      console.error('Erro ao deletar oferta:', err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  /**
   * Deleta todas as ofertas
   */
  const deleteAllOffers = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      await offerService.deleteAll();
      offers.value = [];
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao deletar todas as ofertas';
      console.error('Erro ao deletar todas as ofertas:', err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  /**
   * Solicita scraping de uma URL
   */
  const requestScraping = async (url: string) => {
    loading.value = true;
    error.value = null;
    
    try {
      const request: ScraperRequest = { url };
      const message = await offerService.requestScraping(request);
      
      // Recarrega as ofertas após o scraping
      await loadOffers();
      
      return message;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao solicitar scraping';
      console.error('Erro ao solicitar scraping:', err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  /**
   * Computed para verificar se há ofertas
   */
  const hasOffers = computed(() => offers.value.length > 0);

  /**
   * Computed para contar ofertas
   */
  const offersCount = computed(() => offers.value.length);

  return {
    // State
    offers,
    loading,
    error,
    
    // Computed
    hasOffers,
    offersCount,
    
    // Methods
    loadOffers,
    createOffer,
    deleteOffer,
    deleteAllOffers,
    requestScraping,
  };
}



