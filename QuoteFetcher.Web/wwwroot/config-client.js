import { fetchWithTimeout, isTimeoutError } from './api-client.js';

export async function loadConfig() {
    try {
        const response = await fetchWithTimeout('/api/config');
        if (!response.ok) {
            console.warn('Failed to load config, using relative URLs');
            return '';
        }

        const config = await response.json();
        return config.apiBaseUrl || '';
    } catch (error) {
        if (isTimeoutError(error)) {
            console.warn('Config request timed out, using relative URLs');
            return '';
        }

        console.warn('Failed to load config, using relative URLs:', error);
        return '';
    }
}
