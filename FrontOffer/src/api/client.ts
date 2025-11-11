/**
 * Cliente HTTP para comunicação com a API
 */
import { getApiUrl } from '../config/api';
import type { ApiResponse } from '../types/offer';

// Timeout padrão de 30 segundos
const DEFAULT_TIMEOUT = 30000;

class ApiClient {
  private createTimeoutSignal(timeoutMs: number): AbortSignal {
    const controller = new AbortController();
    setTimeout(() => controller.abort(), timeoutMs);
    return controller.signal;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
    timeoutMs: number = DEFAULT_TIMEOUT
  ): Promise<ApiResponse<T>> {
    const url = getApiUrl(endpoint);
    const baseUrl = url.substring(0, url.lastIndexOf(endpoint));
    
    const defaultHeaders: HeadersInit = {
      'Content-Type': 'application/json',
    };

    // Cria signal para timeout
    const timeoutSignal = this.createTimeoutSignal(timeoutMs);

    const config: RequestInit = {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
      signal: timeoutSignal,
    };

    try {
      const response = await fetch(url, config);
      
      // Para respostas 204 (No Content), não há body para parsear
      let data: T | null = null;
      const contentType = response.headers.get('content-type');
      let responseText = '';
      
      // Tenta ler o texto da resposta
      if (response.status !== 204 && contentType?.includes('application/json')) {
        try {
          responseText = await response.text();
          if (responseText) {
            data = JSON.parse(responseText) as T;
          }
        } catch (parseError) {
          console.error('Erro ao parsear JSON:', parseError);
          // Continua mesmo se não conseguir parsear
        }
      }

      // Se a resposta não foi bem-sucedida, lança erro com detalhes
      if (!response.ok) {
        // Tenta usar a mensagem do JSON se disponível
        const errorData = data as any;
        const errorMessage = errorData?.message || responseText || response.statusText || `Erro ${response.status}`;
        throw new Error(errorMessage);
      }

      return {
        data: data as T,
        status: response.status,
        message: response.statusText,
      };
    } catch (error) {
      // Trata diferentes tipos de erro
      if (error instanceof Error) {
        // Erro de abort (timeout)
        if (error.name === 'AbortError' || error.message.toLowerCase().includes('aborted')) {
          throw new Error('Tempo de espera esgotado. A requisição demorou muito para responder.');
        }
        // Erro de rede (Failed to fetch, NetworkError, etc.)
        const errorMessageLower = error.message.toLowerCase();
        if (
          errorMessageLower.includes('failed to fetch') ||
          errorMessageLower.includes('networkerror') ||
          errorMessageLower.includes('network error') ||
          errorMessageLower.includes('fetch') ||
          errorMessageLower.includes('network') ||
          errorMessageLower.includes('connection') ||
          errorMessageLower.includes('cors') ||
          errorMessageLower.includes('unable to fetch')
        ) {
          throw new Error(
            `Erro de conexão com a API. Verifique se a API está rodando em ${baseUrl} e se o CORS está configurado corretamente.`
          );
        }
        // Propaga outros erros (já tratados acima com mensagens adequadas)
        throw error;
      }
      // Erro desconhecido
      throw new Error(`Erro desconhecido na requisição para ${url}`);
    }
  }

  async get<T>(endpoint: string, timeoutMs?: number): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'GET' }, timeoutMs);
  }

  async post<T>(endpoint: string, body?: unknown, timeoutMs?: number): Promise<ApiResponse<T>> {
    return this.request<T>(
      endpoint,
      {
        method: 'POST',
        body: body ? JSON.stringify(body) : undefined,
      },
      timeoutMs
    );
  }

  async delete<T>(endpoint: string, timeoutMs?: number): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'DELETE' }, timeoutMs);
  }
}

export const apiClient = new ApiClient();

