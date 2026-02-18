<script setup lang="ts">
</script>

<template>
  <header class="site-header">
    <div class="site-header-inner">
      <NuxtLink to="/" class="site-logo">SEEMS</NuxtLink>
      <Navigation />
    </div>
  </header>
</template>

<style scoped>
.site-header {
  border-bottom: 1px solid #e5e5e5;
}

.site-header-inner {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.site-logo {
  font-size: 1.25rem;
  font-weight: 700;
  letter-spacing: 0.05em;
}
</style>
