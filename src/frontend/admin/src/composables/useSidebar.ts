import { ref } from 'vue'

// Module-level singletons — shared across all components using this composable
const collapsed = ref(false)
const mobileOpen = ref(false)

export function useSidebar() {
  return { collapsed, mobileOpen }
}
