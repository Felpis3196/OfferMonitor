import { ref, onMounted, onUnmounted } from 'vue';
import { connectGlobalLogsFeed } from '../api/services/scrapeService';

export function useGlobalLogs() {
  const logs = ref<any[]>([]);
  let eventSource: EventSource | null = null;

  onMounted(() => {
    eventSource = connectGlobalLogsFeed((newLog) => {
      logs.value.unshift(newLog); // adiciona no topo do feed
    });
  });

  onUnmounted(() => {
    if (eventSource) eventSource.close();
  });

  return { logs };
}
