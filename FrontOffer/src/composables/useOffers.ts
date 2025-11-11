/**
 * Composable para gerenciar estado das ofertas
 */
import { ref, computed } from 'vue';
import { offerService } from '../api/services/offerService';
import type { Offer, OfferInput, ScraperRequest } from '../types/offer';

export interface FilterState {
  stores: string[];
  categories: string[];
  minPrice: number | null;
  maxPrice: number | null;
}

export interface SortOption {
  field: 'price' | 'title' | 'store';
  direction: 'asc' | 'desc';
}

export function useOffers() {
  const offers = ref<Offer[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const searchQuery = ref('');
  const filters = ref<FilterState>({
    stores: [],
    categories: [],
    minPrice: null,
    maxPrice: null,
  });
  const sortOption = ref<SortOption>({
    field: 'price',
    direction: 'asc',
  });

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
      const result = await offerService.requestScraping(request);
      
      // Não recarrega as ofertas imediatamente - aguarda o scraping terminar
      // O usuário pode acompanhar pelos logs
      
      return result;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erro ao solicitar scraping';
      console.error('Erro ao solicitar scraping:', err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  /**
   * Filtra e busca ofertas
   */
  const filteredOffers = computed(() => {
    let result = [...offers.value];

    // Aplica busca por texto
    if (searchQuery.value.trim()) {
      const query = searchQuery.value.toLowerCase().trim();
      result = result.filter((offer) => {
        return (
          offer.title.toLowerCase().includes(query) ||
          offer.store.toLowerCase().includes(query) ||
          offer.category.toLowerCase().includes(query) ||
          offer.url.toLowerCase().includes(query)
        );
      });
    }

    // Aplica filtros
    if (filters.value.stores.length > 0) {
      result = result.filter((offer) =>
        filters.value.stores.includes(offer.store)
      );
    }

    if (filters.value.categories.length > 0) {
      result = result.filter((offer) =>
        filters.value.categories.includes(offer.category)
      );
    }

    if (filters.value.minPrice !== null) {
      result = result.filter((offer) => offer.price >= filters.value.minPrice!);
    }

    if (filters.value.maxPrice !== null) {
      result = result.filter((offer) => offer.price <= filters.value.maxPrice!);
    }

    // Aplica ordenação
    result.sort((a, b) => {
      let comparison = 0;

      switch (sortOption.value.field) {
        case 'price':
          comparison = a.price - b.price;
          break;
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'store':
          comparison = a.store.localeCompare(b.store);
          break;
      }

      return sortOption.value.direction === 'asc' ? comparison : -comparison;
    });

    return result;
  });

  /**
   * Obtém lista única de lojas
   */
  const availableStores = computed(() => {
    const stores = new Set(offers.value.map((offer) => offer.store));
    return Array.from(stores).sort();
  });

  /**
   * Obtém lista única de categorias
   */
  const availableCategories = computed(() => {
    const categories = new Set(offers.value.map((offer) => offer.category));
    return Array.from(categories).sort();
  });

  /**
   * Define a query de busca
   */
  const setSearchQuery = (query: string) => {
    searchQuery.value = query;
  };

  /**
   * Define os filtros
   */
  const setFilters = (newFilters: FilterState) => {
    filters.value = newFilters;
  };

  /**
   * Define a opção de ordenação
   */
  const setSortOption = (option: SortOption) => {
    sortOption.value = option;
  };

  /**
   * Computed para verificar se há ofertas
   */
  const hasOffers = computed(() => offers.value.length > 0);

  /**
   * Computed para contar ofertas
   */
  const offersCount = computed(() => offers.value.length);

  /**
   * Computed para contar ofertas filtradas
   */
  const filteredCount = computed(() => filteredOffers.value.length);

  return {
    // State
    offers,
    loading,
    error,
    searchQuery,
    filters,
    sortOption,
    
    // Computed
    hasOffers,
    offersCount,
    filteredOffers,
    filteredCount,
    availableStores,
    availableCategories,
    
    // Methods
    loadOffers,
    createOffer,
    deleteOffer,
    deleteAllOffers,
    requestScraping,
    setSearchQuery,
    setFilters,
    setSortOption,
  };
}




