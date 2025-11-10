/**
 * Serviço para gerenciar ofertas
 */
import { apiClient } from '../client';
import { API_CONFIG } from '../../config/api';
import type { Offer, OfferInput, ScraperRequest } from '../../types/offer';

export class OfferService {
  /**
   * Busca todas as ofertas
   */
  async getAll(): Promise<Offer[]> {
    const response = await apiClient.get<Offer[]>(API_CONFIG.endpoints.offers);
    
    if (response.status !== 200) {
      throw new Error(`Erro ao buscar ofertas: ${response.message}`);
    }
    
    return response.data || [];
  }

  /**
   * Busca uma oferta por ID
   */
  async getById(id: string): Promise<Offer> {
    const response = await apiClient.get<Offer>(
      `${API_CONFIG.endpoints.offers}/${id}`
    );
    
    if (response.status !== 200) {
      throw new Error(`Erro ao buscar oferta: ${response.message}`);
    }
    
    return response.data;
  }

  /**
   * Cria uma nova oferta
   */
  async create(offer: OfferInput): Promise<Offer> {
    const response = await apiClient.post<Offer>(
      API_CONFIG.endpoints.offers,
      offer
    );
    
    if (response.status !== 201 && response.status !== 200) {
      throw new Error(`Erro ao criar oferta: ${response.message}`);
    }
    
    return response.data;
  }

  /**
   * Deleta uma oferta por ID
   */
  async deleteById(id: string): Promise<void> {
    const response = await apiClient.delete<void>(
      `${API_CONFIG.endpoints.offers}/${id}`
    );
    
    if (response.status !== 204 && response.status !== 200) {
      throw new Error(`Erro ao deletar oferta: ${response.message}`);
    }
  }

  /**
   * Deleta todas as ofertas
   */
  async deleteAll(): Promise<void> {
    const response = await apiClient.delete<void>(
      API_CONFIG.endpoints.offers
    );
    
    if (response.status !== 200) {
      throw new Error(`Erro ao deletar todas as ofertas: ${response.message}`);
    }
  }

  /**
   * Solicita scraping de uma URL
   */
  async requestScraping(request: ScraperRequest): Promise<string> {
    const response = await apiClient.post<string>(
      API_CONFIG.endpoints.scrape,
      request
    );
    
    if (response.status !== 200) {
      throw new Error(`Erro ao solicitar scraping: ${response.message}`);
    }
    
    return response.data || 'Scraping solicitado com sucesso';
  }
}

export const offerService = new OfferService();



