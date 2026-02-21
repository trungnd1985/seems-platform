export default defineEventHandler((event) => {
  const url = getRequestURL(event)

  if (url.pathname.startsWith('/api')) {
    return
  }

  const cookies = parseCookies(event)
  if (cookies['seems_preview_token']) {
    // Preview requests must never be served from or written to the CDN/ISR cache.
    setResponseHeaders(event, {
      'Cache-Control': 'no-store',
    })
    return
  }

  setResponseHeaders(event, {
    'Cache-Control': 's-maxage=60, stale-while-revalidate=300',
  })
})
