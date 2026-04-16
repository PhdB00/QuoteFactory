const ERROR_DISPLAY_DURATION = 5000;
const CRAWL_CLEANUP_DELAY_MS = 26000;

export function createUi() {
    let errorTimeoutId = null;

    function displayCrawl(text) {
        const crawlDiv = document.createElement('div');
        crawlDiv.className = 'crawl';
        crawlDiv.textContent = text;

        const container = document.getElementById('crawl-container');
        container.appendChild(crawlDiv);

        setTimeout(() => {
            if (container.contains(crawlDiv)) {
                container.removeChild(crawlDiv);
            }
        }, CRAWL_CLEANUP_DELAY_MS);
    }

    function showError(message) {
        const errorDiv = document.getElementById('error-message');
        errorDiv.textContent = message;
        errorDiv.classList.remove('hidden');

        if (errorTimeoutId !== null) {
            clearTimeout(errorTimeoutId);
        }

        errorTimeoutId = setTimeout(() => {
            errorDiv.classList.add('hidden');
            errorTimeoutId = null;
        }, ERROR_DISPLAY_DURATION);
    }

    function cleanup() {
        if (errorTimeoutId !== null) {
            clearTimeout(errorTimeoutId);
            errorTimeoutId = null;
        }
    }

    return {
        displayCrawl,
        showError,
        cleanup
    };
}
