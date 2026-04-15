// Configuration
// API_BASE_URL will be loaded from /api/config endpoint
let API_BASE_URL = '';
const BUBBLE_SPEED = 2;
const BUBBLE_SIZE = 60; // Approximate radius for collision detection
const ERROR_DISPLAY_DURATION = 5000; // 5 seconds

// State
let bubbles = [];
let animationFrameId = null;
let spatialGrid = null;
let errorTimeoutId = null;

// Bubble class
class Bubble {
    constructor(category, x, y) {
        this.category = category;
        this.x = x;
        this.y = y;
        this.vx = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.vy = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.element = this.createElement();
        this.radius = this.element.offsetWidth / 2;
    }

    createElement() {
        const div = document.createElement('div');
        div.className = 'bubble';
        div.textContent = this.category;
        // Use transform instead of left/top for GPU acceleration
        div.style.transform = `translate3d(${this.x}px, ${this.y}px, 0)`;
        div.style.willChange = 'transform';
        this.boundClickHandler = () => this.onClick();
        div.addEventListener('click', this.boundClickHandler);
        document.getElementById('bubble-container').appendChild(div);
        return div;
    }

    async onClick() {
        // Visual feedback
        this.element.classList.add('clicked');
        setTimeout(() => this.element.classList.remove('clicked'), 300);

        try {
            const response = await fetch(`${API_BASE_URL}/quote?category=${encodeURIComponent(this.category)}`);
            if (!response.ok) {
                showError(`Unable to fetch quote. Please try again.`);
                return;
            }
            const data = await response.json();
            // API returns { icon_url: string, Value: string }
            const quoteText = data.Value || data.value || data.quote || data.text || JSON.stringify(data);
            displayCrawl(quoteText);
        } catch (error) {
            console.error('Error fetching quote:', error.message || 'Unknown error');
            showError('Unable to connect to the quote service. Please check if the API is running.');
        }
    }

    update() {
        // Update position
        this.x += this.vx;
        this.y += this.vy;

        // Bounce off walls
        const maxX = window.innerWidth - this.radius * 2;
        const maxY = window.innerHeight - this.radius * 2;

        if (this.x <= 0 || this.x >= maxX) {
            this.vx *= -1;
            this.x = Math.max(0, Math.min(this.x, maxX));
        }

        if (this.y <= 0 || this.y >= maxY) {
            this.vy *= -1;
            this.y = Math.max(0, Math.min(this.y, maxY));
        }

        // Update DOM using transform for GPU acceleration (avoids layout thrashing)
        this.element.style.transform = `translate3d(${this.x}px, ${this.y}px, 0)`;
    }

    getCenterX() {
        return this.x + this.radius;
    }

    getCenterY() {
        return this.y + this.radius;
    }

    destroy() {
        // Clean up event listeners to prevent memory leaks
        if (this.boundClickHandler) {
            this.element.removeEventListener('click', this.boundClickHandler);
        }
        // Remove from DOM
        if (this.element.parentNode) {
            this.element.parentNode.removeChild(this.element);
        }
    }
}

// Calculate distance between two bubbles
function getDistance(bubbleA, bubbleB) {
    const dx = bubbleA.getCenterX() - bubbleB.getCenterX();
    const dy = bubbleA.getCenterY() - bubbleB.getCenterY();
    return Math.sqrt(dx * dx + dy * dy);
}

// Collision detection between two bubbles
function checkCollision(bubble1, bubble2) {
    const distance = getDistance(bubble1, bubble2);
    const minDistance = bubble1.radius + bubble2.radius;
    return distance < minDistance;
}

// Resolve collision between two bubbles
function resolveCollision(bubble1, bubble2) {
    const dx = bubble2.getCenterX() - bubble1.getCenterX();
    const dy = bubble2.getCenterY() - bubble1.getCenterY();
    const distance = Math.sqrt(dx * dx + dy * dy);
    
    // Guard against invalid distance
    if (distance === 0 || !isFinite(distance)) return;

    // Normalize collision vector
    const nx = dx / distance;
    const ny = dy / distance;

    // Relative velocity
    const dvx = bubble2.vx - bubble1.vx;
    const dvy = bubble2.vy - bubble1.vy;

    // Relative velocity in collision normal direction
    const dotProduct = dvx * nx + dvy * ny;

    // Do not resolve if velocities are separating
    if (dotProduct > 0) return;

    // Calculate impulse
    const impulse = 2 * dotProduct / 2; // Assuming equal mass

    // Apply impulse to velocities
    bubble1.vx += impulse * nx;
    bubble1.vy += impulse * ny;
    bubble2.vx -= impulse * nx;
    bubble2.vy -= impulse * ny;

    // Separate overlapping bubbles
    const overlap = bubble1.radius + bubble2.radius - distance;
    const separationX = (overlap / 2) * nx;
    const separationY = (overlap / 2) * ny;

    bubble1.x -= separationX;
    bubble1.y -= separationY;
    bubble2.x += separationX;
    bubble2.y += separationY;
}

// Spatial grid for efficient collision detection
class SpatialGrid {
    constructor(cellSize) {
        this.cellSize = cellSize;
        this.grid = new Map();
    }

    clear() {
        this.grid.clear();
    }

    _getKey(x, y) {
        return `${x},${y}`;
    }

    _getCellCoords(worldX, worldY) {
        return {
            x: Math.floor(worldX / this.cellSize),
            y: Math.floor(worldY / this.cellSize)
        };
    }

    insert(bubble) {
        const cell = this._getCellCoords(bubble.getCenterX(), bubble.getCenterY());
        const key = this._getKey(cell.x, cell.y);

        if (!this.grid.has(key)) {
            this.grid.set(key, []);
        }
        this.grid.get(key).push(bubble);
    }

    getNearby(bubble) {
        const cell = this._getCellCoords(bubble.getCenterX(), bubble.getCenterY());
        const nearby = [];

        // Check current cell and 8 adjacent cells
        for (let dx = -1; dx <= 1; dx++) {
            for (let dy = -1; dy <= 1; dy++) {
                const key = this._getKey(cell.x + dx, cell.y + dy);
                const cellBubbles = this.grid.get(key);
                if (cellBubbles) {
                    nearby.push(...cellBubbles);
                }
            }
        }

        return nearby;
    }
}

// Animation loop
function animate() {
    // Update all bubbles
    bubbles.forEach(bubble => bubble.update());

    // Rebuild spatial grid
    spatialGrid.clear();
    bubbles.forEach(bubble => spatialGrid.insert(bubble));

    // Check collisions using spatial grid
    const checked = new Set();
    bubbles.forEach(bubble => {
        const nearby = spatialGrid.getNearby(bubble);
        nearby.forEach(other => {
            if (bubble !== other) {
                // Create unique pair key to avoid checking same pair twice
                const pairKey = bubble < other ? `${bubbles.indexOf(bubble)},${bubbles.indexOf(other)}` : `${bubbles.indexOf(other)},${bubbles.indexOf(bubble)}`;
                if (!checked.has(pairKey)) {
                    checked.add(pairKey);
                    if (checkCollision(bubble, other)) {
                        resolveCollision(bubble, other);
                    }
                }
            }
        });
    });

    animationFrameId = requestAnimationFrame(animate);
}

// Pause animation when page is hidden to save resources
function handleVisibilityChange() {
    if (document.hidden) {
        // Page is hidden, cancel animation
        if (animationFrameId !== null) {
            cancelAnimationFrame(animationFrameId);
            animationFrameId = null;
        }
    } else {
        // Page is visible, resume animation
        if (animationFrameId === null && bubbles.length > 0) {
            animate();
        }
    }
}

// Display Star Wars crawl
function displayCrawl(text) {
    const crawlDiv = document.createElement('div');
    crawlDiv.className = 'crawl';
    crawlDiv.textContent = text;

    const container = document.getElementById('crawl-container');
    container.appendChild(crawlDiv);

    // Remove crawl after animation completes (25s animation + 1s buffer)
    setTimeout(() => {
        if (container.contains(crawlDiv)) {
            container.removeChild(crawlDiv);
        }
    }, 26000);
}

// Show error message
function showError(message) {
    const errorDiv = document.getElementById('error-message');
    errorDiv.textContent = message;
    errorDiv.classList.remove('hidden');

    // Clear any pending error timeout to prevent accumulation
    if (errorTimeoutId !== null) {
        clearTimeout(errorTimeoutId);
    }

    // Auto-hide after duration
    errorTimeoutId = setTimeout(() => {
        errorDiv.classList.add('hidden');
        errorTimeoutId = null;
    }, ERROR_DISPLAY_DURATION);
}

// Load configuration from server
async function loadConfig() {
    try {
        const response = await fetch('/api/config');
        if (!response.ok) {
            console.warn('Failed to load config, using relative URLs');
            return '';
        }
        const config = await response.json();
        return config.apiBaseUrl || '';
    } catch (error) {
        console.warn('Failed to load config, using relative URLs:', error);
        return '';
    }
}

// Fetch categories and create bubbles
async function initialize() {
    try {
        // Load configuration first
        API_BASE_URL = await loadConfig();

        const response = await fetch(`${API_BASE_URL}/quote_category`);
        if (!response.ok) {
            showError(`Unable to load categories. Please refresh the page.`);
            console.error('Failed to fetch categories');
            return;
        }

        const categories = await response.json();

        // API returns a simple string array: ["animal", "celebrity", "food", ...]
        let categoryNames = [];
        if (Array.isArray(categories)) {
            categoryNames = categories.filter(cat => typeof cat === 'string' && cat.trim().length > 0);
        }

        if (categoryNames.length === 0) {
            showError('No categories found. Please check the API response.');
            console.error('No valid categories found in response:', categories);
            return;
        }

        // Create bubbles with random positions
        const BUBBLE_HORIZONTAL_MARGIN = 200;
        const BUBBLE_VERTICAL_MARGIN = 100;
        const BUBBLE_MIN_OFFSET = 50;
        const MAX_PLACEMENT_ATTEMPTS = 50;

        categoryNames.forEach((category, index) => {
            let x, y, attempts = 0;
            let positionFound = false;

            // Try to find a non-overlapping position
            while (attempts < MAX_PLACEMENT_ATTEMPTS && !positionFound) {
                x = Math.random() * (window.innerWidth - BUBBLE_HORIZONTAL_MARGIN) + BUBBLE_MIN_OFFSET;
                y = Math.random() * (window.innerHeight - BUBBLE_VERTICAL_MARGIN) + BUBBLE_MIN_OFFSET;

                // Check if this position overlaps with any existing bubble
                positionFound = true;
                for (let existingBubble of bubbles) {
                    const dx = (x + BUBBLE_SIZE) - existingBubble.getCenterX();
                    const dy = (y + BUBBLE_SIZE) - existingBubble.getCenterY();
                    const distance = Math.sqrt(dx * dx + dy * dy);
                    const minDistance = BUBBLE_SIZE + existingBubble.radius;

                    if (distance < minDistance) {
                        positionFound = false;
                        break;
                    }
                }

                attempts++;
            }

            bubbles.push(new Bubble(category, x, y));
        });

        // Initialize spatial grid with cell size of 2 * bubble radius
        spatialGrid = new SpatialGrid(BUBBLE_SIZE * 2);

        // Start animation
        animate();

        // Set up visibility change listener for performance optimization
        document.addEventListener('visibilitychange', handleVisibilityChange);
    } catch (error) {
        console.error('Error initializing application:', error);
        showError('Unable to connect to the quote service. Please ensure the API is running.');
    }
}

// Cleanup function for proper resource management
function cleanup() {
    // Cancel animation
    if (animationFrameId !== null) {
        cancelAnimationFrame(animationFrameId);
        animationFrameId = null;
    }
    // Destroy all bubbles
    bubbles.forEach(bubble => bubble.destroy());
    bubbles = [];
    // Remove visibility change listener
    document.removeEventListener('visibilitychange', handleVisibilityChange);
}

// Start the application when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initialize);
} else {
    initialize();
}

// Clean up resources when page is unloaded
window.addEventListener('beforeunload', cleanup);
