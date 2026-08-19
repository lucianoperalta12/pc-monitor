<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const API_KEY = '8cpry3Pe5RqHcN-AelgzQSZhf77HVpigAey56exaf0Gk9hi-zW0glr6rVy4DvJNU'
const MACHINE_ID = 'PC-CASA'
const POLL_MS = 10_000

const status   = ref(null)   // objeto del /api/pc/status
const sessions = ref([])
const loading  = ref(true)
const error    = ref(null)
const now      = ref(new Date())

let pollTimer = null
let clockTimer = null

// ─── Fetch ───────────────────────────────────────────────────────────────────
async function apiFetch(path) {
  const res = await fetch(`/api${path}?machineId=${MACHINE_ID}`, {
    headers: { Authorization: `Bearer ${API_KEY}` }
  })
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  return res.json()
}

async function fetchAll() {
  try {
    const [st, hist] = await Promise.all([
      apiFetch('/pc/status'),
      apiFetch('/pc/sessions'),
    ])
    status.value  = st
    sessions.value = hist
    error.value   = null
  } catch (e) {
    error.value = `No se pudo conectar con el servidor. ${e.message}`
  } finally {
    loading.value = false
  }
}

// ─── Computed ─────────────────────────────────────────────────────────────────
const isOnline = computed(() => status.value?.status === 'ONLINE')

const lastContactAgo = computed(() => {
  if (!status.value) return '—'
  const diff = Math.floor((now.value - new Date(status.value.lastContact)) / 1000)
  if (diff < 5)  return 'justo ahora'
  if (diff < 60) return `hace ${diff}s`
  if (diff < 3600) return `hace ${Math.floor(diff/60)}m ${diff%60}s`
  return `hace ${Math.floor(diff/3600)}h`
})

const uptimeLive = computed(() => {
  if (!isOnline.value || !status.value?.currentSession) return null
  const diff = Math.floor((now.value - new Date(status.value.currentSession.startedAt)) / 1000)
  const h = Math.floor(diff / 3600)
  const m = Math.floor((diff % 3600) / 60)
  const s = diff % 60
  if (h > 0) return `${h}h ${m}m`
  if (m > 0) return `${m}m ${s}s`
  return `${s}s`
})

// ─── Formatters ───────────────────────────────────────────────────────────────
function fmtDate(iso) {
  if (!iso) return '—'
  return new Date(iso).toLocaleString('es-AR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

function fmtTime(iso) {
  if (!iso) return '—'
  return new Date(iso).toLocaleString('es-AR', {
    day: '2-digit', month: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(() => {
  fetchAll()
  pollTimer  = setInterval(fetchAll, POLL_MS)
  clockTimer = setInterval(() => { now.value = new Date() }, 1000)
})
onUnmounted(() => {
  clearInterval(pollTimer)
  clearInterval(clockTimer)
})
</script>

<template>
  <div id="app">
    <div class="container">

      <!-- Header -->
      <header class="header">
        <span class="header-icon">🖥️</span>
        <h1>PC Monitor</h1>
        <span class="header-sub">{{ MACHINE_ID }}</span>
      </header>

      <!-- Loading -->
      <div v-if="loading" class="loader">
        <div class="spinner"></div>
        <span>Conectando…</span>
      </div>

      <template v-else>

        <!-- Error -->
        <div v-if="error" class="error-box">⚠️ {{ error }}</div>

        <template v-if="status">

          <!-- Status principal -->
          <div class="status-card" :class="isOnline ? 'online' : 'offline'">
            <div>
              <div class="status-badge">
                <span class="status-dot"></span>
                {{ isOnline ? 'ENCENDIDA' : 'APAGADA' }}
              </div>
            </div>
            <div class="status-meta">
              <div class="last-contact-label">Último contacto</div>
              <div class="last-contact-value">{{ lastContactAgo }}</div>
            </div>
          </div>

          <!-- Stats -->
          <div class="stat-grid" v-if="isOnline && status.currentSession">
            <div class="stat-box">
              <div class="stat-label">Encendida desde</div>
              <div class="stat-value">{{ fmtDate(status.currentSession.startedAt) }}</div>
            </div>
            <div class="stat-box">
              <div class="stat-label">Tiempo encendida</div>
              <div class="stat-value big">{{ uptimeLive }}</div>
            </div>
          </div>

          <!-- Última sesión cerrada -->
          <div class="card" v-if="status.lastSession">
            <div class="section-title">Última sesión</div>
            <div class="session-row header-row">
              <span>Inicio</span>
              <span>Fin</span>
              <span>Duración</span>
            </div>
            <div class="session-row">
              <span>{{ fmtTime(status.lastSession.startedAt) }}</span>
              <span>{{ fmtTime(status.lastSession.endedAt) }}</span>
              <span class="duration-chip">{{ status.lastSession.duration }}</span>
            </div>
          </div>

          <!-- Historial -->
          <div class="card" v-if="sessions.length > 0">
            <div class="section-title">Historial de sesiones</div>
            <div class="session-row header-row">
              <span>Inicio</span>
              <span>Fin</span>
              <span>Duración</span>
            </div>
            <div
              v-for="s in sessions"
              :key="s.id"
              class="session-row"
            >
              <span>{{ fmtTime(s.startedAt) }}</span>
              <span>{{ fmtTime(s.endedAt) }}</span>
              <span class="duration-chip">{{ s.duration }}</span>
            </div>
          </div>

        </template>
      </template>

    </div>
  </div>
</template>
