const REQUEST_TIMEOUT_MS = 8000;

class UserFacingError extends Error {
    constructor(userMessage, logMessage, cause) {
        super(logMessage || userMessage);
        this.name = 'UserFacingError';
        this.userMessage = userMessage;
        this.cause = cause;
    }
}

function createTimeoutError(timeoutMs) {
    const error = new Error(`Request timed out after ${timeoutMs}ms`);
    error.name = 'TimeoutError';
    return error;
}

export function isTimeoutError(error) {
    return error && error.name === 'TimeoutError';
}

export async function fetchWithTimeout(url, options = {}, timeoutMs = REQUEST_TIMEOUT_MS) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    try {
        return await fetch(url, { ...options, signal: controller.signal });
    } catch (error) {
        if (error && error.name === 'AbortError') {
            throw createTimeoutError(timeoutMs);
        }

        throw error;
    } finally {
        clearTimeout(timeoutId);
    }
}

function normalizeBaseUrl(apiBaseUrl) {
    return apiBaseUrl || '';
}

export async function getCategories(apiBaseUrl) {
    let response;

    try {
        response = await fetchWithTimeout(`${normalizeBaseUrl(apiBaseUrl)}/quote_category`);
    } catch (error) {
        if (isTimeoutError(error)) {
            throw new UserFacingError(
                'Unable to load categories: request timed out. Please refresh the page.',
                'Category request timed out.',
                error
            );
        }

        throw new UserFacingError(
            'Unable to connect to the quote service. Please ensure the API is running.',
            'Unable to connect while loading categories.',
            error
        );
    }

    if (!response.ok) {
        throw new UserFacingError(
            'Unable to load categories. Please refresh the page.',
            `Failed to fetch categories: ${response.status}`
        );
    }

    const categories = await response.json();
    const categoryNames = Array.isArray(categories)
        ? categories.filter((cat) => typeof cat === 'string' && cat.trim().length > 0)
        : [];

    if (categoryNames.length === 0) {
        throw new UserFacingError(
            'No categories found. Please check the API response.',
            'No valid categories found in response.'
        );
    }

    return categoryNames;
}

export async function getQuote(apiBaseUrl, category) {
    let response;

    try {
        response = await fetchWithTimeout(
            `${normalizeBaseUrl(apiBaseUrl)}/quote?category=${encodeURIComponent(category)}`
        );
    } catch (error) {
        if (isTimeoutError(error)) {
            throw new UserFacingError(
                'Unable to fetch quote: request timed out. Please try again.',
                'Quote request timed out.',
                error
            );
        }

        throw new UserFacingError(
            'Unable to connect to the quote service. Please check if the API is running.',
            'Unable to connect while fetching quote.',
            error
        );
    }

    if (!response.ok) {
        throw new UserFacingError(
            'Unable to fetch quote. Please try again.',
            `Quote endpoint returned ${response.status}.`
        );
    }

    const data = await response.json();
    return data.Value || data.value || data.quote || data.text || JSON.stringify(data);
}

export function getUserMessage(error, fallbackMessage) {
    if (error && error.userMessage) {
        return error.userMessage;
    }

    return fallbackMessage;
}

export function getLogMessage(error, fallbackMessage) {
    if (error && error.message) {
        return error.message;
    }

    return fallbackMessage;
}
