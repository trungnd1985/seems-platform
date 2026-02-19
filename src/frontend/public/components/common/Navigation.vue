<script setup lang="ts">
import { useNavigation } from '~/composables/useNavigation'

const { data: navItems } = await useNavigation()
</script>

<template>
  <nav class="site-nav">
    <ul>
      <li v-for="item in navItems" :key="item.slug">
        <NuxtLink :to="`/${item.slug}`">{{ item.label }}</NuxtLink>
      </li>
    </ul>
  </nav>
</template>

<style scoped>
.site-nav ul {
  display: flex;
  gap: 1.5rem;
  list-style: none;
  padding: 0;
}

.site-nav a {
  font-size: 0.9375rem;
  font-weight: 500;
  transition: color 0.15s;
}

.site-nav a:hover {
  color: #3b82f6;
}
</style>
