import { defineStore } from "pinia"
import { ref, watch } from "vue"

const storageKey = "aiSelectedInstanceId"

function readStoredId(): number | undefined {
    const stored = localStorage.getItem(storageKey)
    if (stored) {
        const parsed = parseInt(stored, 10)
        if (!isNaN(parsed) && parsed > 0) {
            return parsed
        }
    }
    return undefined
}

export const useAiSelectedInstanceStore = defineStore('aiSelectedInstance', () => {
    const selectedInstanceId = ref<number | undefined>(readStoredId())

    watch(selectedInstanceId, (id) => {
        if (id && id > 0) {
            localStorage.setItem(storageKey, id.toString())
        } else {
            localStorage.removeItem(storageKey)
        }
    })

    function setSelectedInstanceId(id: number | undefined) {
        selectedInstanceId.value = id
    }

    function clearSelectedInstanceId() {
        selectedInstanceId.value = undefined
    }

    return { selectedInstanceId, setSelectedInstanceId, clearSelectedInstanceId }
})
