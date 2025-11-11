<template>
  <div v-if="visible" class="scraping-logs">
    <div class="logs-header">
      <div class="logs-title">
        <h3>📋 Logs do Scraping</h3>
        <span class="logs-status" :class="statusClass">
          {{ statusText }}
        </span>
      </div>
      <div class="logs-actions">
        <button @click="toggleAutoScroll" class="action-btn" :class="{ active: autoScroll }" title="Auto-scroll">
          {{ autoScroll ? '📌' : '📍' }}
        </button>
        <button @click="clearLogs" class="action-btn" title="Limpar logs">
          🗑️
        </button>
        <button @click="closeLogs" class="action-btn" title="Fechar">
          ×
        </button>
      </div>
    </div>

    <div class="logs-container" ref="logsContainerRef">
      <div v-if="logs.length === 0" class="logs-empty">
        <div class="empty-icon">⏳</div>
        <p>Aguardando logs do scraping...</p>
      </div>
      <div
        v-for="(log, index) in logs"
        :key="index"
        class="log-entry"
        :class="getLogLevelClass(log.level)"
      >
        <span class="log-time">{{ formatTime(log.timestamp) }}</span>
        <span class="log-message">{{ log.message }}</span>
      </div>
      <div v-if="isLoading" class="logs-loading">
        <div class="loading-dot"></div>
        <span>Carregando logs...</span>
      </div>
    </div>

    <div class="logs-footer">
      <span class="logs-count">{{ logs.length }} log(s)</span>
      <span v-if="lastUpdate" class="logs-update">
        Atualizado: {{ formatTime(lastUpdate) }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, onUnmounted } from 'vue';

export interface ScrapingLogEntry {
  requestId: string;
  timestamp: string;
  message: string;
  level: string;
}

interface Props {
  requestId: string | null;
  visible: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const logs = ref<ScrapingLogEntry[]>([]);
const isLoading = ref(false);
const autoScroll = ref(true);
const lastUpdate = ref<Date | null>(null);
const logsContainerRef = ref<HTMLDivElement | null>(null);
const pollingInterval = ref<number | null>(null);
const statusText = ref('Aguardando...');
const statusClass = ref('status-waiting');

const formatTime = (timestamp: string | Date): string => {
  const date = typeof timestamp === 'string' ? new Date(timestamp) : timestamp;
  return date.toLocaleTimeString('pt-BR', { 
    hour: '2-digit', 
    minute: '2-digit', 
    second: '2-digit' 
  });
};

const getLogLevelClass = (level: string): string => {
  const levelLower = level.toLowerCase();
  if (levelLower === 'success') return 'log-success';
  if (levelLower === 'error') return 'log-error';
  if (levelLower === 'warning') return 'log-warning';
  return 'log-info';
};

const toggleAutoScroll = () => {
  autoScroll.value = !autoScroll.value;
  if (autoScroll.value) {
    scrollToBottom();
  }
};

const clearLogs = () => {
  logs.value = [];
};

const closeLogs = () => {
  stopPolling();
  emit('close');
};

const scrollToBottom = () => {
  nextTick(() => {
    if (logsContainerRef.value) {
      logsContainerRef.value.scrollTop = logsContainerRef.value.scrollHeight;
    }
  });
};

const fetchLogs = async () => {
  if (!props.requestId || !props.visible) return;

  try {
    isLoading.value = true;
    const response = await fetch(
      `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'}/api/offers/scrape/logs`
    );

    if (response.ok) {
      const data = await response.json();
      if (data.logs && Array.isArray(data.logs)) {
        logs.value = data.logs;
        lastUpdate.value = new Date();
        updateStatus();
        
        if (autoScroll.value) {
          scrollToBottom();
        }
      }
    }
  } catch (error) {
    console.error('Erro ao buscar logs:', error);
  } finally {
    isLoading.value = false;
  }
};

const updateStatus = () => {
  if (logs.value.length === 0) {
    statusText.value = 'Aguardando...';
    statusClass.value = 'status-waiting';
    return;
  }

  const lastLog = logs.value[logs.value.length - 1];
  const level = lastLog.level.toLowerCase();

  if (level === 'success' && lastLog.message.includes('concluído')) {
    statusText.value = 'Concluído';
    statusClass.value = 'status-success';
  } else if (level === 'error') {
    statusText.value = 'Erro';
    statusClass.value = 'status-error';
  } else if (level === 'warning') {
    statusText.value = 'Aviso';
    statusClass.value = 'status-warning';
  } else {
    statusText.value = 'Processando...';
    statusClass.value = 'status-processing';
  }
};

const startPolling = () => {
  if (pollingInterval.value) {
    clearInterval(pollingInterval.value);
  }

  // Busca imediatamente
  fetchLogs();

  // Depois busca a cada 1 segundo
  pollingInterval.value = window.setInterval(() => {
    fetchLogs();
  }, 1000);
};

const stopPolling = () => {
  if (pollingInterval.value) {
    clearInterval(pollingInterval.value);
    pollingInterval.value = null;
  }
};

watch(() => props.requestId, (newId) => {
  if (newId && props.visible) {
    logs.value = [];
    startPolling();
  } else {
    stopPolling();
  }
});

watch(() => props.visible, (visible) => {
  if (visible && props.requestId) {
    startPolling();
  } else {
    stopPolling();
  }
});

onUnmounted(() => {
  stopPolling();
});
</script>

<style scoped>
.scraping-logs {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  max-height: 500px;
  margin-bottom: 1.5rem;
  overflow: hidden;
}

.logs-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-bottom: 2px solid rgba(255, 255, 255, 0.2);
}

.logs-title {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex: 1;
}

.logs-title h3 {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.logs-status {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-waiting {
  background: rgba(255, 255, 255, 0.3);
}

.status-processing {
  background: rgba(59, 130, 246, 0.8);
  animation: pulse 2s infinite;
}

.status-success {
  background: rgba(16, 185, 129, 0.8);
}

.status-error {
  background: rgba(239, 68, 68, 0.8);
}

.status-warning {
  background: rgba(245, 158, 11, 0.8);
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

.logs-actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  background: rgba(255, 255, 255, 0.2);
  border: none;
  border-radius: 6px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 1rem;
  color: white;
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.1);
}

.action-btn.active {
  background: rgba(255, 255, 255, 0.4);
}

.logs-container {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  background: #1e1e1e;
  color: #d4d4d4;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 0.875rem;
  line-height: 1.6;
  max-height: 400px;
}

.logs-container::-webkit-scrollbar {
  width: 8px;
}

.logs-container::-webkit-scrollbar-track {
  background: #2d2d2d;
}

.logs-container::-webkit-scrollbar-thumb {
  background: #555;
  border-radius: 4px;
}

.logs-container::-webkit-scrollbar-thumb:hover {
  background: #777;
}

.logs-empty {
  text-align: center;
  padding: 2rem;
  color: #888;
}

.empty-icon {
  font-size: 2rem;
  margin-bottom: 0.5rem;
}

.log-entry {
  display: flex;
  gap: 1rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(-10px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.log-time {
  color: #888;
  flex-shrink: 0;
  min-width: 80px;
}

.log-message {
  flex: 1;
  word-break: break-word;
}

.log-info {
  color: #d4d4d4;
}

.log-success {
  color: #4ec9b0;
}

.log-success .log-message::before {
  content: "✓ ";
  color: #4ec9b0;
}

.log-warning {
  color: #dcdcaa;
}

.log-warning .log-message::before {
  content: "⚠ ";
  color: #dcdcaa;
}

.log-error {
  color: #f48771;
}

.log-error .log-message::before {
  content: "✗ ";
  color: #f48771;
}

.logs-loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  color: #888;
  justify-content: center;
}

.loading-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #667eea;
  animation: bounce 1.4s infinite ease-in-out both;
}

.loading-dot::before,
.loading-dot::after {
  content: '';
  position: absolute;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #667eea;
  animation: bounce 1.4s infinite ease-in-out both;
}

.loading-dot::before {
  left: -12px;
  animation-delay: -0.32s;
}

.loading-dot::after {
  left: 12px;
  animation-delay: 0.32s;
}

@keyframes bounce {
  0%, 80%, 100% {
    transform: scale(0);
  }
  40% {
    transform: scale(1);
  }
}

.logs-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1.5rem;
  background: #2d2d2d;
  color: #888;
  font-size: 0.75rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.logs-count {
  font-weight: 500;
}

.logs-update {
  font-size: 0.7rem;
}

@media (max-width: 768px) {
  .scraping-logs {
    max-height: 400px;
  }

  .logs-container {
    max-height: 300px;
    font-size: 0.75rem;
  }

  .logs-header {
    padding: 0.75rem 1rem;
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .logs-title h3 {
    font-size: 1rem;
  }

  .logs-footer {
    flex-direction: column;
    gap: 0.25rem;
    align-items: flex-start;
  }
}
</style>

