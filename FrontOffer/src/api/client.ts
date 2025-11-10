/**
 * Cliente HTTP para comunicação com a API
 */
import { getApiUrl } from '../config/api';
import type { ApiResponse } from '../types/offer';

class ApiClient {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = getApiUrl(endpoint);
    
    const defaultHeaders: HeadersInit = {
      'Content-Type': 'application/json',
    };

    const config: RequestInit = {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, config);
      
      // Para respostas 204 (No Content), não há body para parsear
      let data: T | null = null;
      const contentType = response.headers.get('content-type');
      
      if (response.status !== 204 && contentType?.includes('application/json')) {
        try {
          data = await response.json();
        } catch {
          // Se não conseguir parsear JSON, retorna null
          data = null;
        }
      }

      return {
        data: data as T,
        status: response.status,
        message: response.statusText,
      };
    } catch (error) {
      throw new Error(
        `Erro na requisição: ${error instanceof Error ? error.message : 'Erro desconhecido'}`
      );
    }
  }

  async get<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'GET' });
  }

  async post<T>(endpoint: string, body?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();

