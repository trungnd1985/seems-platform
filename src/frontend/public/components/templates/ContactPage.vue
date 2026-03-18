<script setup lang="ts">
import type { Page, SlotMapping } from '~/types/page'

const props = defineProps<{
  page: Page
  slots: SlotMapping[]
}>()

function getSlot(slotKey: string): SlotMapping[] {
  return props.slots
    .filter((s) => s.slotKey === slotKey)
    .sort((a, b) => a.order - b.order)
}
</script>

<template>
  <div class="contact-page">
    <!-- ── Page Header ─────────────────────────────────────── -->
    <div class="page-header">
      <div class="container">
        <div class="page-header-inner">
          <h1 class="page-header-title">{{ page.title || 'Contact Us' }}</h1>
          <nav class="breadcrumb-nav" aria-label="Breadcrumb">
            <ol>
              <li><NuxtLink to="/">Home</NuxtLink></li>
              <li aria-current="page">{{ page.title || 'Contact Us' }}</li>
            </ol>
          </nav>
        </div>
      </div>
    </div>

    <!-- ── Map slot (full-width) ──────────────────────────── -->
    <div class="contact-map">
      <template v-if="getSlot('map').length">
        <SlotRenderer
          v-for="mapping in getSlot('map')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </template>
      <div v-else class="map-placeholder">
        <div class="map-placeholder-inner">
          <svg class="map-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z" />
            <circle cx="12" cy="10" r="3" />
          </svg>
          <p>Assign a map module to the <code>map</code> slot to display a location here.</p>
        </div>
      </div>
    </div>

    <!-- ── Two-column body ────────────────────────────────── -->
    <div class="contact-body">
      <div class="container">
        <div class="contact-grid">

          <!-- Left: form slot -->
          <div class="contact-form-col">
            <span class="subheading">Send a Message</span>
            <h2 class="contact-form-title">Contact <strong>Us</strong></h2>
            <p class="contact-form-subtitle">Feel free to ask for details, don't save any questions!</p>

            <template v-if="getSlot('form').length">
              <SlotRenderer
                v-for="mapping in getSlot('form')"
                :key="mapping.targetId"
                :mapping="mapping"
              />
            </template>
            <div v-else class="form-placeholder">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <rect x="3" y="3" width="18" height="18" rx="2" />
                <path d="M7 8h10M7 12h6M7 16h4" />
              </svg>
              <p>Add the <strong>Contact Form</strong> module to the <code>form</code> slot to enable this form.</p>
            </div>
          </div>

          <!-- Right: info slot or fallback -->
          <div class="contact-info-col">
            <template v-if="getSlot('info').length">
              <SlotRenderer
                v-for="mapping in getSlot('info')"
                :key="mapping.targetId"
                :mapping="mapping"
              />
            </template>
            <template v-else>
              <!-- Our Office -->
              <div class="info-block appear appear-delay-1">
                <h4 class="info-block-title">Our <strong>Office</strong></h4>
                <ul class="info-list">
                  <li>
                    <span class="info-icon" aria-hidden="true">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z" /><circle cx="12" cy="10" r="3" />
                      </svg>
                    </span>
                    <span><strong>Address:</strong> Melbourne, 121 King St, Australia</span>
                  </li>
                  <li>
                    <span class="info-icon" aria-hidden="true">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.72 9.5 19.79 19.79 0 01.67 4.9 2 2 0 012.65 3h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L6.91 10.9a16 16 0 006 6l1.06-1.06a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z" />
                      </svg>
                    </span>
                    <span><strong>Phone:</strong> <a href="tel:+1234567890">(123) 456-789</a></span>
                  </li>
                  <li>
                    <span class="info-icon" aria-hidden="true">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" />
                      </svg>
                    </span>
                    <span><strong>Email:</strong> <a href="mailto:mail@example.com">mail@example.com</a></span>
                  </li>
                </ul>
              </div>

              <!-- Business Hours -->
              <div class="info-block appear appear-delay-2">
                <h4 class="info-block-title">Business <strong>Hours</strong></h4>
                <ul class="hours-list">
                  <li>
                    <span class="hours-day">Monday – Friday</span>
                    <span class="hours-time">9:00 AM – 5:00 PM</span>
                  </li>
                  <li>
                    <span class="hours-day">Saturday</span>
                    <span class="hours-time">9:00 AM – 2:00 PM</span>
                  </li>
                  <li class="hours-closed">
                    <span class="hours-day">Sunday</span>
                    <span class="hours-time">Closed</span>
                  </li>
                </ul>
              </div>

              <!-- Get in Touch -->
              <div class="info-block appear appear-delay-3">
                <h4 class="info-block-title">Get in <strong>Touch</strong></h4>
                <p class="info-lead">
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur eget leo at
                  velit imperdiet varius. In eu ipsum vitae velit congue iaculis vitae at risus.
                </p>
              </div>
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- ── CTA strip ──────────────────────────────────────── -->
    <section class="contact-cta">
      <template v-if="getSlot('cta').length">
        <SlotRenderer
          v-for="mapping in getSlot('cta')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </template>
      <div v-else class="container">
        <div class="cta-inner">
          <div class="cta-content">
            <h2 class="cta-title">Ready to <strong>get started?</strong> We'd love to hear from you.</h2>
            <p class="cta-sub">Our team is happy to answer all your questions.</p>
          </div>
          <div class="cta-action">
            <a href="tel:+1234567890" class="btn btn-dark btn-lg">Call Us Now</a>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
/* ─── Page Header ─── */
.page-header {
  background: linear-gradient(135deg, #1d2127 0%, #2a3140 100%);
  padding: 50px 0;
}

.page-header-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.page-header-title {
  font-size: 1.8rem;
  font-weight: 800;
  color: #fff;
  margin: 0;
}

.breadcrumb-nav ol {
  display: flex;
  align-items: center;
  gap: 0;
  font-size: 0.78rem;
}

.breadcrumb-nav li {
  color: rgba(255, 255, 255, 0.5);
}

.breadcrumb-nav li + li {
  padding-left: 0.5rem;
}

.breadcrumb-nav li + li::before {
  content: '›';
  margin-right: 0.5rem;
  color: rgba(255, 255, 255, 0.3);
}

.breadcrumb-nav a {
  color: rgba(255, 255, 255, 0.6);
  transition: color 0.2s;
}

.breadcrumb-nav a:hover { color: #fff; }
.breadcrumb-nav li:last-child { color: #fff; }

/* ─── Map ─── */
.contact-map {
  height: 420px;
  overflow: hidden;
}

.map-placeholder {
  height: 100%;
  background: linear-gradient(135deg, rgba(0, 136, 204, 0.05) 0%, rgba(29, 33, 39, 0.04) 100%);
  border-bottom: 1px solid #e9e9e9;
  display: flex;
  align-items: center;
  justify-content: center;
}

.map-placeholder-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  text-align: center;
  max-width: 280px;
}

.map-icon {
  width: 44px;
  height: 44px;
  color: #0088cc;
  opacity: 0.35;
}

.map-placeholder-inner p {
  font-size: 0.82rem;
  color: #bbb;
  margin: 0;
}

.map-placeholder-inner code {
  background: #f0f0f0;
  padding: 1px 5px;
  border-radius: 3px;
  font-size: 0.78rem;
  color: #555;
}

/* ─── Body ─── */
.contact-body {
  padding: 70px 0 60px;
}

.contact-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 60px;
  align-items: start;
}

/* ─── Form column ─── */
.contact-form-col {
  min-width: 0;
}

.contact-form-title {
  font-size: 1.75rem;
  font-weight: 400;
  margin-bottom: 0.25rem;
  color: #1d2127;
}

.contact-form-title strong { font-weight: 800; }

.contact-form-subtitle {
  color: #999;
  font-size: 0.875rem;
  margin-bottom: 1.75rem;
}

/* Form not configured placeholder */
.form-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 40px 20px;
  border: 2px dashed #e0e0e0;
  border-radius: 4px;
  text-align: center;
  color: #bbb;
}

.form-placeholder svg {
  width: 40px;
  height: 40px;
  opacity: 0.4;
}

.form-placeholder p {
  font-size: 0.82rem;
  color: #aaa;
  margin: 0;
}

.form-placeholder strong { color: #777; }

.form-placeholder code {
  background: #f5f5f5;
  padding: 1px 5px;
  border-radius: 3px;
  font-size: 0.78rem;
  color: #555;
}

/* ─── Info column ─── */
.contact-info-col {
  min-width: 0;
}

.info-block {
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #f0f0f0;
}

.info-block:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.info-block-title {
  font-size: 1.1rem;
  font-weight: 400;
  color: #1d2127;
  margin-bottom: 1rem;
}

.info-block-title strong { font-weight: 800; }

.info-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.info-list li {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  font-size: 0.875rem;
  color: #555;
}

.info-icon {
  flex-shrink: 0;
  width: 18px;
  height: 18px;
  margin-top: 1px;
  color: #0088cc;
}

.info-icon svg { width: 100%; height: 100%; }

.info-list a { color: #0088cc; transition: color 0.2s; }
.info-list a:hover { color: #006fa3; }

.hours-list {
  border: 1px solid #f0f0f0;
  border-radius: 3px;
  overflow: hidden;
}

.hours-list li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  font-size: 0.85rem;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.15s;
}

.hours-list li:last-child { border-bottom: none; }
.hours-list li:hover { background: #fafafa; }

.hours-day { font-weight: 600; color: #1d2127; }
.hours-time { color: #777; }
.hours-closed .hours-time { color: #e05252; font-weight: 600; }

.info-lead {
  font-size: 0.9rem;
  line-height: 1.8;
  color: #777;
  margin: 0;
}

/* ─── CTA ─── */
.contact-cta {
  background: #1d2127;
  padding: 50px 0;
}

.cta-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2rem;
  flex-wrap: wrap;
}

.cta-title {
  font-size: 1.4rem;
  font-weight: 400;
  color: #fff;
  margin-bottom: 0.25rem;
}

.cta-title strong { font-weight: 800; }

.cta-sub {
  color: rgba(255, 255, 255, 0.45);
  font-size: 0.875rem;
  margin: 0;
}

.cta-action { flex-shrink: 0; }

/* ─── Responsive ─── */
@media (max-width: 991px) {
  .contact-grid { grid-template-columns: 1fr; gap: 40px; }
  .contact-map { height: 300px; }
}

@media (max-width: 576px) {
  .page-header-inner { flex-direction: column; align-items: flex-start; gap: 0.5rem; }
  .cta-inner { flex-direction: column; align-items: flex-start; }
}
</style>
