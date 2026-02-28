// Re-exports the complete Vue 3 public API from the host Nuxt app's Vue
// instance (window.__SEEMS_VUE__) so the module bundle never imports bare
// 'vue' and always shares the same runtime as the host app.
// Aliased as 'vue' via resolve.alias in vite.config.ts.
const _v = window.__SEEMS_VUE__

// ── Reactivity ────────────────────────────────────────────────────────────
export const ref = _v.ref
export const isRef = _v.isRef
export const unref = _v.unref
export const toRef = _v.toRef
export const toValue = _v.toValue
export const toRefs = _v.toRefs
export const proxyRefs = _v.proxyRefs
export const shallowRef = _v.shallowRef
export const triggerRef = _v.triggerRef
export const customRef = _v.customRef
export const reactive = _v.reactive
export const isReactive = _v.isReactive
export const shallowReactive = _v.shallowReactive
export const readonly = _v.readonly
export const isReadonly = _v.isReadonly
export const shallowReadonly = _v.shallowReadonly
export const isProxy = _v.isProxy
export const isShallow = _v.isShallow
export const markRaw = _v.markRaw
export const toRaw = _v.toRaw
export const computed = _v.computed
export const watch = _v.watch
export const watchEffect = _v.watchEffect
export const watchPostEffect = _v.watchPostEffect
export const watchSyncEffect = _v.watchSyncEffect
export const effectScope = _v.effectScope
export const getCurrentScope = _v.getCurrentScope
export const onScopeDispose = _v.onScopeDispose

// ── Lifecycle hooks ───────────────────────────────────────────────────────
export const onBeforeMount = _v.onBeforeMount
export const onMounted = _v.onMounted
export const onBeforeUpdate = _v.onBeforeUpdate
export const onUpdated = _v.onUpdated
export const onBeforeUnmount = _v.onBeforeUnmount
export const onUnmounted = _v.onUnmounted
export const onActivated = _v.onActivated
export const onDeactivated = _v.onDeactivated
export const onErrorCaptured = _v.onErrorCaptured
export const onRenderTracked = _v.onRenderTracked
export const onRenderTriggered = _v.onRenderTriggered
export const onServerPrefetch = _v.onServerPrefetch

// ── Component / instance ──────────────────────────────────────────────────
export const defineComponent = _v.defineComponent
export const defineAsyncComponent = _v.defineAsyncComponent
export const getCurrentInstance = _v.getCurrentInstance
export const useAttrs = _v.useAttrs
export const useSlots = _v.useSlots
export const useModel = _v.useModel
export const provide = _v.provide
export const inject = _v.inject
export const hasInjectionContext = _v.hasInjectionContext
export const nextTick = _v.nextTick

// ── Vnode / createElement helpers ─────────────────────────────────────────
export const h = _v.h
export const createVNode = _v.createVNode
export const createElementVNode = _v.createElementVNode
export const createTextVNode = _v.createTextVNode
export const createCommentVNode = _v.createCommentVNode
export const createStaticVNode = _v.createStaticVNode
export const cloneVNode = _v.cloneVNode
export const mergeProps = _v.mergeProps
export const isVNode = _v.isVNode

// ── Block helpers (compiled template) ────────────────────────────────────
export const openBlock = _v.openBlock
export const closeBlock = _v.closeBlock
export const createBlock = _v.createBlock
export const createElementBlock = _v.createElementBlock
export const setBlockTracking = _v.setBlockTracking

// ── Render helpers (compiled template) ───────────────────────────────────
export const renderList = _v.renderList
export const renderSlot = _v.renderSlot
export const toDisplayString = _v.toDisplayString
export const normalizeClass = _v.normalizeClass
export const normalizeStyle = _v.normalizeStyle
export const normalizeProps = _v.normalizeProps
export const guardReactiveProps = _v.guardReactiveProps
export const withCtx = _v.withCtx
export const withDirectives = _v.withDirectives
export const withModifiers = _v.withModifiers
export const withKeys = _v.withKeys

// ── Component resolution ──────────────────────────────────────────────────
export const resolveComponent = _v.resolveComponent
export const resolveDirective = _v.resolveDirective
export const resolveDynamicComponent = _v.resolveDynamicComponent

// ── Built-in directives ───────────────────────────────────────────────────
export const vShow = _v.vShow
export const vModelText = _v.vModelText
export const vModelCheckbox = _v.vModelCheckbox
export const vModelSelect = _v.vModelSelect
export const vModelDynamic = _v.vModelDynamic

// ── Built-in components ───────────────────────────────────────────────────
export const Transition = _v.Transition
export const TransitionGroup = _v.TransitionGroup
export const KeepAlive = _v.KeepAlive
export const Teleport = _v.Teleport
export const Suspense = _v.Suspense
export const BaseTransition = _v.BaseTransition
export const BaseTransitionPropsValidators = _v.BaseTransitionPropsValidators

// ── Special vnode types ───────────────────────────────────────────────────
export const Fragment = _v.Fragment
export const Text = _v.Text
export const Comment = _v.Comment
export const Static = _v.Static

// ── Misc ──────────────────────────────────────────────────────────────────
export const camelize = _v.camelize
export const capitalize = _v.capitalize
export const registerRuntimeCompiler = _v.registerRuntimeCompiler

export default _v
