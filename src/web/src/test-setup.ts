import '@testing-library/jest-dom'

// Node.js 22+ provides a native localStorage global that lacks Storage methods,
// conflicting with jsdom's implementation. Provide a proper mock.
const createStorageMock = (): Storage => {
  let store = new Map<string, string>()
  return {
    getItem: (key: string) => store.get(key) ?? null,
    setItem: (key: string, value: string) => {
      store.set(key, value)
    },
    removeItem: (key: string) => {
      store.delete(key)
    },
    clear: () => {
      store = new Map()
    },
    get length() {
      return store.size
    },
    key: (index: number) => [...store.keys()][index] ?? null,
  }
}

Object.defineProperty(globalThis, 'localStorage', {
  value: createStorageMock(),
  writable: true,
})
