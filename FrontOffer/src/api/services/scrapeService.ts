// src/api/services/scrapeService.ts
export function connectGlobalLogsFeed(onMessage: (log: any) => void) {
  const eventSource = new EventSource('http://localhost:8000/scrape/logs/stream'); // ajuste a URL da sua API

  eventSource.onmessage = (event) => {
    try {
      const data = JSON.parse(event.data);
      onMessage(data);
    } catch (err) {
      console.error('Erro ao parsear log:', err);
    }
  };

  eventSource.onerror = (error) => {
    console.error('Erro no stream SSE:', error);
    eventSource.close();
  };

  return eventSource;
}
