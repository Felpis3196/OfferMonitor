/**
 * Configuração da API
 */
export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  endpoints: {
    offers: '/api/offers',
    scrape: '/api/offers/scrape',
  },
} as const;

export const getApiUrl = (endpoint: string): string => {
  return `${API_CONFIG.baseURL}${endpoint}`;
};



