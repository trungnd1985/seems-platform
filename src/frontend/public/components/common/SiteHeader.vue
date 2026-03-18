<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const isSticky = ref(false)
const mobileOpen = ref(false)

function onScroll() {
  isSticky.value = window.scrollY > 45
}

onMounted(() => {
  window.addEventListener('scroll', onScroll, { passive: true })
})

onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
})
</script>

<template>
  <!-- position:fixed — does not participate in document flow -->
  <header class="site-header" :class="{ 'is-sticky': isSticky }">
    <!-- Top Bar -->
    <div class="header-top">
      <div class="container">
        <div class="header-top-inner">
          <ul class="header-top-nav">
            <li><NuxtLink to="/about">About Us</NuxtLink></li>
            <li><NuxtLink to="/contact">Contact Us</NuxtLink></li>
          </ul>
          <div class="header-phone">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.72 9.5 19.79 19.79 0 01.67 4.9 2 2 0 012.65 3h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L6.91 10.9a16 16 0 006 6l1.06-1.06a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z" />
            </svg>
            (123) 456-789
          </div>
        </div>
      </div>
    </div>

    <!-- Main Header Body -->
    <div class="header-body">
      <div class="container">
        <div class="header-row">
          <div class="header-logo">
            <NuxtLink to="/" aria-label="Home">
              <span class="logo-text">SEEMS</span>
            </NuxtLink>
          </div>

          <nav class="header-nav" aria-label="Main navigation">
            <Navigation />
          </nav>

          <button
            class="hamburger"
            :class="{ 'is-active': mobileOpen }"
            :aria-expanded="mobileOpen"
            aria-label="Toggle menu"
            @click="mobileOpen = !mobileOpen"
          >
            <span />
            <span />
            <span />
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile Drawer -->
    <div v-show="mobileOpen" class="mobile-drawer">
      <div class="container">
        <Navigation :mobile="true" @navigate="mobileOpen = false" />
      </div>
    </div>
  </header>

  <!-- Pushes body content below the fixed header -->
  <div class="header-spacer" :class="{ 'is-sticky': isSticky }" aria-hidden="true" />
</template>

<style scoped>
/* ─── Fixed header ─── */
.site-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: #fff;
  transition: box-shadow 0.3s ease;
}

.site-header.is-sticky {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

/* ─── Top Bar ─── */
.header-top {
  background: #1d2127;
  color: #9da5b1;
  font-size: 0.78rem;
  height: 45px;
  display: flex;
  align-items: center;
  overflow: hidden;
  transition: height 0.3s ease, opacity 0.25s ease;
}

.is-sticky .header-top {
  height: 0;
  opacity: 0;
  pointer-events: none;
}

.header-top-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.header-top-nav {
  display: flex;
  gap: 0;
}

.header-top-nav li {
  position: relative;
  padding: 0 0.875rem;
}

.header-top-nav li + li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 1px;
  height: 10px;
  background: rgba(255, 255, 255, 0.15);
}

.header-top-nav a {
  color: #9da5b1;
  font-weight: 500;
  transition: color 0.2s;
}

.header-top-nav a:hover { color: #fff; }

.header-phone {
  display: flex;
  align-items: center;
  gap: 7px;
  font-weight: 600;
  color: #9da5b1;
}

/* ─── Header Body ─── */
.header-body {
  height: 80px;
  display: flex;
  align-items: center;
  transition: height 0.3s ease;
}

.is-sticky .header-body { height: 64px; }

.header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2rem;
  width: 100%;
}

.header-logo { flex-shrink: 0; }

.logo-text {
  font-size: 1.6rem;
  font-weight: 800;
  letter-spacing: 0.06em;
  color: #1d2127;
  transition: font-size 0.3s ease;
  display: block;
}

.is-sticky .logo-text { font-size: 1.35rem; }

.header-nav {
  flex: 1;
  display: flex;
  justify-content: flex-end;
}

/* ─── Hamburger ─── */
.hamburger {
  display: none;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  width: 34px;
  height: 34px;
}

.hamburger span {
  display: block;
  height: 2px;
  background: #1d2127;
  border-radius: 2px;
  transition: all 0.25s ease;
  transform-origin: center;
}

.hamburger.is-active span:nth-child(1) { transform: translateY(7px) rotate(45deg); }
.hamburger.is-active span:nth-child(2) { opacity: 0; }
.hamburger.is-active span:nth-child(3) { transform: translateY(-7px) rotate(-45deg); }

/* ─── Mobile Drawer ─── */
.mobile-drawer {
  background: #fff;
  border-top: 1px solid #e9e9e9;
  padding: 0.75rem 0;
  max-height: 80vh;
  overflow-y: auto;
}

/* ─── Spacer (keeps content below fixed header) ─── */
.header-spacer {
  height: 125px; /* 45px top-bar + 80px body */
  transition: height 0.3s ease;
}

.header-spacer.is-sticky { height: 64px; }

/* ─── Responsive ─── */
@media (max-width: 991px) {
  .header-top { display: none; }
  .header-nav { display: none; }
  .hamburger { display: flex; }
  .header-spacer,
  .header-spacer.is-sticky { height: 80px; }
}
</style>
