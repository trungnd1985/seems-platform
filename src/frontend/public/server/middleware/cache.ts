export default defineEventHandler((event) => {
  const url = getRequestURL(event)

  if (url.pathname.startsWith('/api')) {
    return
  }

  setResponseHeaders(event, {
    'Cache-Control': 's-maxage=60, stale-while-revalidate=300',
  })
})
