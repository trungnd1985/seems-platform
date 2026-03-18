<script setup lang="ts">
import { ref } from 'vue'
import { useNavigation, type NavItem } from '~/composables/useNavigation'

const props = defineProps<{
  mobile?: boolean
}>()

const emit = defineEmits<{
  navigate: []
}>()

const { data: navItems } = await useNavigation()

const openSlug = ref<string | null>(null)

function handleMouseEnter(item: NavItem) {
  if (!props.mobile && item.children?.length) openSlug.value = item.slug
}

function handleMouseLeave() {
  if (!props.mobile) openSlug.value = null
}

function handleLinkClick(item: NavItem) {
  if (props.mobile && item.children?.length) {
    openSlug.value = openSlug.value === item.slug ? null : item.slug
  } else {
    emit('navigate')
  }
}
</script>

<template>
  <ul :class="mobile ? 'nav-mobile' : 'nav-desktop'">
    <li
      v-for="item in navItems"
      :key="item.slug"
      :class="['nav-item', { 'has-dropdown': item.children?.length }]"
      @mouseenter="handleMouseEnter(item)"
      @mouseleave="handleMouseLeave"
    >
      <NuxtLink
        :to="`/${item.slug}`"
        class="nav-link"
        active-class="is-active"
        @click.prevent="handleLinkClick(item)"
      >
        {{ item.label }}
        <svg
          v-if="item.children?.length"
          class="nav-arrow"
          width="10"
          height="6"
          viewBox="0 0 10 6"
          fill="none"
          aria-hidden="true"
        >
          <path d="M1 1l4 4 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </NuxtLink>

      <ul
        v-if="item.children?.length"
        class="dropdown"
        :class="{ 'is-open': openSlug === item.slug }"
      >
        <li v-for="child in item.children" :key="child.slug">
          <NuxtLink :to="`/${child.slug}`" class="dropdown-link" @click="emit('navigate')">
            {{ child.label }}
          </NuxtLink>
        </li>
      </ul>
    </li>
  </ul>
</template>

<style scoped>
/* ─── Desktop ─── */
.nav-desktop {
  display: flex;
  align-items: center;
}

.nav-item {
  position: relative;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 0.5rem 0.9rem;
  font-size: 0.8rem;
  font-weight: 700;
  color: #1d2127;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
  transition: color 0.2s;
}

.nav-link:hover,
.nav-link.is-active {
  color: #0088cc;
}

.nav-arrow {
  transition: transform 0.2s;
  flex-shrink: 0;
}

.has-dropdown:hover .nav-arrow {
  transform: rotate(180deg);
}

/* Dropdown */
.dropdown {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  background: #fff;
  min-width: 210px;
  box-shadow: 0 8px 28px rgba(0, 0, 0, 0.12);
  border-top: 3px solid #0088cc;
  border-radius: 0 0 4px 4px;
  opacity: 0;
  visibility: hidden;
  transform: translateY(8px);
  transition: opacity 0.2s ease, transform 0.2s ease, visibility 0.2s;
  z-index: 200;
}

.dropdown.is-open,
.has-dropdown:hover .dropdown {
  opacity: 1;
  visibility: visible;
  transform: translateY(0);
}

.dropdown-link {
  display: block;
  padding: 9px 18px;
  font-size: 0.8rem;
  font-weight: 500;
  color: #555;
  border-bottom: 1px solid #f5f5f5;
  transition: color 0.15s, padding-left 0.15s, background 0.15s;
}

.dropdown-link:last-child { border-bottom: none; }

.dropdown-link:hover {
  color: #0088cc;
  padding-left: 24px;
  background: #fafafa;
}

/* ─── Mobile ─── */
.nav-mobile {
  display: flex;
  flex-direction: column;
}

.nav-mobile .nav-link {
  padding: 0.7rem 0;
  border-bottom: 1px solid #f0f0f0;
  justify-content: space-between;
  font-size: 0.82rem;
}

.nav-mobile .nav-link:hover,
.nav-mobile .nav-link.is-active {
  color: #0088cc;
}

.nav-mobile .nav-arrow {
  margin-left: auto;
}

.nav-mobile .is-open .nav-arrow {
  transform: rotate(180deg);
}

.nav-mobile .dropdown {
  position: static;
  opacity: 1;
  visibility: visible;
  transform: none;
  box-shadow: none;
  border-top: none;
  border-radius: 0;
  border-left: 3px solid #0088cc;
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease;
}

.nav-mobile .dropdown.is-open {
  max-height: 600px;
}

.nav-mobile .dropdown-link {
  padding: 8px 14px;
  border-bottom: 1px solid #f8f8f8;
}
</style>
