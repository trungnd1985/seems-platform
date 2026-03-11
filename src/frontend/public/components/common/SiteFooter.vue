<script setup lang="ts">
import { useNavigation } from '~/composables/useNavigation'

const { data: navItems } = await useNavigation()
</script>

<template>
  <footer class="site-footer">
    <!-- Main Footer -->
    <div class="footer-main">
      <div class="container">
        <div class="footer-grid">
          <!-- Brand column -->
          <div class="footer-col footer-col--brand">
            <NuxtLink to="/" class="footer-logo" aria-label="Home">SEEMS</NuxtLink>
            <p class="footer-tagline">
              A modern headless CMS platform powering flexible, fast, and scalable web experiences.
            </p>
            <ul class="footer-social">
              <li>
                <a href="#" aria-label="Facebook" target="_blank" rel="noopener noreferrer">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                    <path d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z" />
                  </svg>
                </a>
              </li>
              <li>
                <a href="#" aria-label="X / Twitter" target="_blank" rel="noopener noreferrer">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                    <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
                  </svg>
                </a>
              </li>
              <li>
                <a href="#" aria-label="LinkedIn" target="_blank" rel="noopener noreferrer">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                    <path d="M16 8a6 6 0 016 6v7h-4v-7a2 2 0 00-2-2 2 2 0 00-2 2v7h-4v-7a6 6 0 016-6zM2 9h4v12H2z" />
                    <circle cx="4" cy="4" r="2" />
                  </svg>
                </a>
              </li>
            </ul>
          </div>

          <!-- Quick Links -->
          <div class="footer-col">
            <h5 class="footer-heading">Quick Links</h5>
            <ul class="footer-links">
              <li v-for="item in navItems?.slice(0, 6)" :key="item.slug">
                <NuxtLink :to="`/${item.slug}`">
                  <svg width="8" height="8" viewBox="0 0 8 8" fill="none" aria-hidden="true">
                    <path d="M1 4h6M4 1l3 3-3 3" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                  </svg>
                  {{ item.label }}
                </NuxtLink>
              </li>
            </ul>
          </div>

          <!-- Contact -->
          <div class="footer-col">
            <h5 class="footer-heading">Contact Us</h5>
            <ul class="footer-contact">
              <li>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
                  <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z" />
                  <circle cx="12" cy="10" r="3" />
                </svg>
                234 Street Name, City Name
              </li>
              <li>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.72 9.5 19.79 19.79 0 01.67 4.9 2 2 0 012.65 3h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L6.91 10.9a16 16 0 006 6l1.06-1.06a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z" />
                </svg>
                <a href="tel:8001234567">(800) 123-4567</a>
              </li>
              <li>
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                  <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                  <polyline points="22,6 12,13 2,6" />
                </svg>
                <a href="mailto:mail@example.com">mail@example.com</a>
              </li>
            </ul>
          </div>

          <!-- Newsletter -->
          <div class="footer-col">
            <h5 class="footer-heading">Newsletter</h5>
            <p class="footer-newsletter-text">
              Keep up on our evolving product features. Enter your e-mail to subscribe.
            </p>
            <form class="footer-newsletter-form" @submit.prevent>
              <input type="email" placeholder="Email Address" aria-label="Email address" />
              <button type="submit" class="footer-newsletter-btn">Go</button>
            </form>
          </div>
        </div>
      </div>
    </div>

    <!-- Copyright Bar -->
    <div class="footer-bottom">
      <div class="container">
        <div class="footer-bottom-inner">
          <p>&copy; {{ new Date().getFullYear() }} SEEMS. All Rights Reserved.</p>
          <nav class="footer-bottom-nav" aria-label="Footer navigation">
            <NuxtLink to="/faq">FAQ</NuxtLink>
            <NuxtLink to="/sitemap">Sitemap</NuxtLink>
            <NuxtLink to="/contact">Contact</NuxtLink>
          </nav>
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped>
.site-footer {
  font-size: 0.82rem;
}

/* ─── Main Footer ─── */
.footer-main {
  background: #1d2127;
  color: #9da5b1;
  padding: 60px 0 40px;
}

.footer-grid {
  display: grid;
  grid-template-columns: 2fr 1fr 1.2fr 1.5fr;
  gap: 40px;
}

.footer-heading {
  color: #fff;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  margin-bottom: 1.25rem;
  padding-bottom: 0.6rem;
  border-bottom: 2px solid #0088cc;
  display: inline-block;
}

/* Brand */
.footer-logo {
  display: inline-block;
  font-size: 1.5rem;
  font-weight: 800;
  color: #fff;
  letter-spacing: 0.06em;
  margin-bottom: 1rem;
}

.footer-tagline {
  color: #9da5b1;
  line-height: 1.7;
  margin-bottom: 1.5rem;
  max-width: 280px;
  font-size: 0.8rem;
}

.footer-social {
  display: flex;
  gap: 8px;
}

.footer-social a {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: rgba(255, 255, 255, 0.06);
  border-radius: 50%;
  color: #9da5b1;
  transition: background 0.2s, color 0.2s;
}

.footer-social a:hover {
  background: #0088cc;
  color: #fff;
}

/* Quick Links */
.footer-links li { margin-bottom: 8px; }

.footer-links a {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #9da5b1;
  transition: color 0.2s, padding-left 0.2s;
}

.footer-links a:hover {
  color: #fff;
  padding-left: 4px;
}

.footer-links svg {
  flex-shrink: 0;
  color: #0088cc;
}

/* Contact */
.footer-contact li {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  color: #9da5b1;
  margin-bottom: 12px;
  line-height: 1.5;
}

.footer-contact svg {
  flex-shrink: 0;
  margin-top: 2px;
  color: #0088cc;
}

.footer-contact a {
  color: #9da5b1;
  transition: color 0.2s;
}

.footer-contact a:hover { color: #fff; }

/* Newsletter */
.footer-newsletter-text {
  color: #9da5b1;
  margin-bottom: 1rem;
  line-height: 1.6;
}

.footer-newsletter-form {
  display: flex;
  gap: 6px;
}

.footer-newsletter-form input {
  flex: 1;
  min-width: 0;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 3px;
  color: #fff;
  font-family: inherit;
  font-size: 0.82rem;
  outline: none;
  transition: border-color 0.2s;
}

.footer-newsletter-form input::placeholder { color: #555; }
.footer-newsletter-form input:focus { border-color: #0088cc; }

.footer-newsletter-btn {
  padding: 8px 16px;
  background: #0088cc;
  border: none;
  border-radius: 3px;
  color: #fff;
  font-family: inherit;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.2s;
}

.footer-newsletter-btn:hover { background: #006fa3; }

/* ─── Copyright Bar ─── */
.footer-bottom {
  background: #16191f;
  padding: 14px 0;
}

.footer-bottom-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #555;
}

.footer-bottom-nav {
  display: flex;
  gap: 1.25rem;
}

.footer-bottom-nav a {
  color: #555;
  transition: color 0.2s;
}

.footer-bottom-nav a:hover { color: #0088cc; }

/* ─── Responsive ─── */
@media (max-width: 991px) {
  .footer-grid { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 576px) {
  .footer-grid { grid-template-columns: 1fr; }

  .footer-bottom-inner {
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }
}
</style>
