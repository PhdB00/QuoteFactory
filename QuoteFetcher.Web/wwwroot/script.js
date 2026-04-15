// Configuration
// Use HTTP for API to avoid mixed content issues
const API_BASE_URL = window.API_BASE_URL || 'http://localhost:5074';
const BUBBLE_SPEED = 2;
const BUBBLE_SIZE = 60; // Approximate radius for collision detection
const ERROR_DISPLAY_DURATION = 5000; // 5 seconds

// State
let bubbles = [];
let animationFrameId = null;

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
        div.style.left = `${this.x}px`;
        div.style.top = `${this.y}px`;
        div.addEventListener('click', () => this.onClick());
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
                showError(`Failed to fetch quote: ${response.status} ${response.statusText}`);
                return;
            }
            const data = await response.json();
            // API returns { icon_url: string, Value: string }
            const quoteText = data.Value || data.value || data.quote || data.text || JSON.stringify(data);
            displayCrawl(quoteText);
        } catch (error) {
            console.error('Error fetching quote:', error);
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

        // Update DOM
        this.element.style.left = `${this.x}px`;
        this.element.style.top = `${this.y}px`;
    }

    getCenterX() {
        return this.x + this.radius;
    }

    getCenterY() {
        return this.y + this.radius;
    }
}

// Collision detection between two bubbles
function checkCollision(bubble1, bubble2) {
    const dx = bubble1.getCenterX() - bubble2.getCenterX();
    const dy = bubble1.getCenterY() - bubble2.getCenterY();
    const distance = Math.sqrt(dx * dx + dy * dy);
    const minDistance = bubble1.radius + bubble2.radius;

    return distance < minDistance;
}

// Resolve collision between two bubbles
function resolveCollision(bubble1, bubble2) {
    const dx = bubble2.getCenterX() - bubble1.getCenterX();
    const dy = bubble2.getCenterY() - bubble1.getCenterY();
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance === 0) return; // Prevent division by zero

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

// Animation loop
function animate() {
    // Update all bubbles
    bubbles.forEach(bubble => bubble.update());

    // Check collisions between all bubble pairs
    for (let i = 0; i < bubbles.length; i++) {
        for (let j = i + 1; j < bubbles.length; j++) {
            if (checkCollision(bubbles[i], bubbles[j])) {
                resolveCollision(bubbles[i], bubbles[j]);
            }
        }
    }

    animationFrameId = requestAnimationFrame(animate);
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

    // Auto-hide after duration
    setTimeout(() => {
        errorDiv.classList.add('hidden');
    }, ERROR_DISPLAY_DURATION);
}

// Fetch categories and create bubbles
async function initialize() {
    try {
        const response = await fetch(`${API_BASE_URL}/quote_category`);
        if (!response.ok) {
            showError(`Failed to load categories: ${response.status} ${response.statusText}`);
            console.error('Failed to fetch categories:', response.statusText);
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
        categoryNames.forEach((category, index) => {
            const x = Math.random() * (window.innerWidth - 200) + 50;
            const y = Math.random() * (window.innerHeight - 100) + 50;
            bubbles.push(new Bubble(category, x, y));
        });

        // Start animation
        animate();
    } catch (error) {
        console.error('Error initializing application:', error);
        showError('Unable to connect to the quote service. Please ensure the API is running at ' + API_BASE_URL);
    }
}

// Start the application when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initialize);
} else {
    initialize();
}
