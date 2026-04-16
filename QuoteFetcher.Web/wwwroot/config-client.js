import { fetchWithTimeout, isTimeoutError } from './api-client.js';

const DEFAULT_CONFIG = {
    apiBaseUrl: '',
    bubbleVfx: {
        preset: 'arcade_punchy',
        seed: 1337,
        allowAudioOverlap: false,
        respawnDelayMs: 550,
        explosionDurationMs: 600,
        clickFeedbackDurationMs: 100
    }
};

function mergeBubbleVfxConfig(loadedBubbleVfx) {
    return {
        ...DEFAULT_CONFIG.bubbleVfx,
        ...(loadedBubbleVfx || {})
    };
}

export async function loadConfig() {
    try {
        const response = await fetchWithTimeout('/api/config');
        if (!response.ok) {
            console.warn('Failed to load config, using relative URLs');
            return { ...DEFAULT_CONFIG };
        }

        const config = await response.json();
        return {
            apiBaseUrl: config.apiBaseUrl || '',
            bubbleVfx: mergeBubbleVfxConfig(config.bubbleVfx)
        };
    } catch (error) {
        if (isTimeoutError(error)) {
            console.warn('Config request timed out, using relative URLs');
            return { ...DEFAULT_CONFIG };
        }

        console.warn('Failed to load config, using relative URLs:', error);
        return { ...DEFAULT_CONFIG };
    }
}
