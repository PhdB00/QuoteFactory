import { loadConfig } from './config-client.js';
import { getCategories, getLogMessage, getQuote, getUserMessage } from './api-client.js';
import { Bubble, BUBBLE_SIZE, checkCollision, resolveCollision } from './bubble-model.js';
import { SpatialGrid } from './spatial-grid.js';
import { createUi } from './ui.js';

const BUBBLE_HORIZONTAL_MARGIN = 200;
const BUBBLE_VERTICAL_MARGIN = 100;
const BUBBLE_MIN_OFFSET = 50;
const MAX_PLACEMENT_ATTEMPTS = 50;

function createApp() {
    const state = {
        apiBaseUrl: '',
        bubbles: [],
        spatialGrid: null,
        animationFrameId: null,
        nextBubbleId: 0
    };
    const ui = createUi();

    function createPairKey(bubbleA, bubbleB) {
        return bubbleA.id < bubbleB.id
            ? `${bubbleA.id},${bubbleB.id}`
            : `${bubbleB.id},${bubbleA.id}`;
    }

    function generateBubblePosition() {
        return {
            x: Math.random() * (window.innerWidth - BUBBLE_HORIZONTAL_MARGIN) + BUBBLE_MIN_OFFSET,
            y: Math.random() * (window.innerHeight - BUBBLE_VERTICAL_MARGIN) + BUBBLE_MIN_OFFSET
        };
    }

    function isOverlapping(x, y, existingBubble) {
        const dx = (x + BUBBLE_SIZE) - existingBubble.getCenterX();
        const dy = (y + BUBBLE_SIZE) - existingBubble.getCenterY();
        const distance = Math.sqrt(dx * dx + dy * dy);
        const minDistance = BUBBLE_SIZE + existingBubble.radius;
        return distance < minDistance;
    }

    function createNonOverlappingBubble(category) {
        let x = BUBBLE_MIN_OFFSET;
        let y = BUBBLE_MIN_OFFSET;
        let attempts = 0;
        let positionFound = false;

        while (attempts < MAX_PLACEMENT_ATTEMPTS && !positionFound) {
            const candidate = generateBubblePosition();
            x = candidate.x;
            y = candidate.y;

            positionFound = true;
            for (const existingBubble of state.bubbles) {
                if (isOverlapping(x, y, existingBubble)) {
                    positionFound = false;
                    break;
                }
            }

            attempts++;
        }

        const bubble = new Bubble({
            id: state.nextBubbleId++,
            category,
            x,
            y,
            onBubbleClick: handleBubbleClick
        });
        state.bubbles.push(bubble);
    }

    async function handleBubbleClick(bubble) {
        try {
            const quoteText = await getQuote(state.apiBaseUrl, bubble.category);
            ui.displayCrawl(quoteText);
        } catch (error) {
            console.error('Error fetching quote:', getLogMessage(error, 'Unknown error'));
            ui.showError(getUserMessage(error, 'Unable to fetch quote. Please try again.'));
        }
    }

    function animate() {
        state.bubbles.forEach((bubble) => bubble.update());

        state.spatialGrid.clear();
        state.bubbles.forEach((bubble) => state.spatialGrid.insert(bubble));

        const checkedPairs = new Set();
        state.bubbles.forEach((bubble) => {
            const nearby = state.spatialGrid.getNearby(bubble);
            nearby.forEach((other) => {
                if (bubble === other) {
                    return;
                }

                const pairKey = createPairKey(bubble, other);
                if (checkedPairs.has(pairKey)) {
                    return;
                }

                checkedPairs.add(pairKey);
                if (checkCollision(bubble, other)) {
                    resolveCollision(bubble, other);
                }
            });
        });

        state.animationFrameId = requestAnimationFrame(animate);
    }

    function handleVisibilityChange() {
        if (document.hidden) {
            if (state.animationFrameId !== null) {
                cancelAnimationFrame(state.animationFrameId);
                state.animationFrameId = null;
            }
            return;
        }

        if (state.animationFrameId === null && state.bubbles.length > 0) {
            animate();
        }
    }

    async function initialize() {
        try {
            state.apiBaseUrl = await loadConfig();
            const categories = await getCategories(state.apiBaseUrl);
            categories.forEach((category) => createNonOverlappingBubble(category));

            state.spatialGrid = new SpatialGrid(BUBBLE_SIZE * 2);
            animate();
            document.addEventListener('visibilitychange', handleVisibilityChange);
        } catch (error) {
            console.error('Error initializing application:', getLogMessage(error, 'Unknown error'));
            ui.showError(getUserMessage(error, 'Unable to connect to the quote service. Please ensure the API is running.'));
        }
    }

    function cleanup() {
        if (state.animationFrameId !== null) {
            cancelAnimationFrame(state.animationFrameId);
            state.animationFrameId = null;
        }

        state.bubbles.forEach((bubble) => bubble.destroy());
        state.bubbles = [];

        document.removeEventListener('visibilitychange', handleVisibilityChange);
        ui.cleanup();
    }

    function start() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initialize, { once: true });
        } else {
            initialize();
        }

        window.addEventListener('beforeunload', cleanup, { once: true });
    }

    return {
        start,
        cleanup
    };
}

const app = createApp();
app.start();
