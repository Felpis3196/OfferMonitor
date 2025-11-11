/**
 * Tipos relacionados a Ofertas
 */

export interface Offer {
  id: string;
  title: string;
  store: string;
  category: string;
  url: string;
  price: number;
}

export interface OfferInput {
  title: string;
  url: string;
  store: string;
  category: string;
  price: number;
}

export interface ScraperRequest {
  url: string;
}

export interface ScraperResponse {
  message: string;
  url: string;
  success: boolean;
  error?: string;
  requestId?: string;
}

export interface ScrapingLogEntry {
  requestId: string;
  timestamp: string;
  message: string;
  level: string;
}

export interface ApiResponse<T> {
  data: T;
  status: number;
  message?: string;
}




