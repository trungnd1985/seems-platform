<script setup lang="ts">
</script>

<template>
  <footer class="site-footer">
    <div class="site-footer-inner">
      <p>&copy; {{ new Date().getFullYear() }} SEEMS. All rights reserved.</p>
    </div>
  </footer>
</template>

<style scoped>
.site-footer {
  border-top: 1px solid #e5e5e5;
  background: #fafafa;
}

.site-footer-inner {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
  text-align: center;
  font-size: 0.875rem;
  color: #6b7280;
}
</style>
