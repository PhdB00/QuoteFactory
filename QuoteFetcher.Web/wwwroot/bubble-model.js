const BUBBLE_SPEED = 2;
export const BUBBLE_SIZE = 60;
const CLICK_FEEDBACK_DURATION_MS = 300;
const EXPLOSION_DURATION_MS = 900;
const EXPLOSION_FRAGMENT_COUNT = 18;

export class Bubble {
    constructor({ id, category, x, y, onBubbleClick, onBubbleExploded }) {
        this.id = id;
        this.category = category;
        this.x = x;
        this.y = y;
        this.vx = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.vy = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.onBubbleClick = onBubbleClick;
        this.onBubbleExploded = onBubbleExploded;
        this.state = 'active';
        this.explosionTimeoutId = null;
        this.explosionElement = null;
        this.element = this.createElement();
        this.radius = this.element.offsetWidth / 2;
    }

    createElement() {
        const div = document.createElement('div');
        div.className = 'bubble';
        div.textContent = this.category;
        div.style.transform = `translate3d(${this.x}px, ${this.y}px, 0)`;
        div.style.willChange = 'transform';

        this.boundClickHandler = () => {
            this.handleClick().catch((error) => {
                console.error('Error handling bubble click:', error);
            });
        };

        div.addEventListener('click', this.boundClickHandler);
        document.getElementById('bubble-container').appendChild(div);
        return div;
    }

    async handleClick() {
        if (!this.isActive()) {
            return;
        }

        this.state = 'clicked';
        this.element.classList.add('clicked');
        setTimeout(() => {
            if (this.element) {
                this.element.classList.remove('clicked');
            }

            this.startExplosion();
        }, CLICK_FEEDBACK_DURATION_MS);

        if (this.onBubbleClick) {
            Promise.resolve(this.onBubbleClick(this)).catch((error) => {
                console.error('Error processing bubble click callback:', error);
            });
        }
    }

    startExplosion() {
        if (this.state !== 'clicked' || !this.element) {
            return;
        }

        this.state = 'exploding';
        this.element.classList.add('exploding');

        const explosionElement = document.createElement('div');
        explosionElement.className = 'bubble-explosion';
        explosionElement.style.transform = `translate3d(${this.getCenterX()}px, ${this.getCenterY()}px, 0)`;

        const maxDistance = Math.max(window.innerWidth, window.innerHeight) * 1.25;
        for (let index = 0; index < EXPLOSION_FRAGMENT_COUNT; index++) {
            const fragment = document.createElement('span');
            fragment.className = 'bubble-fragment';

            const angle = Math.random() * Math.PI * 2;
            const distance = maxDistance * (0.6 + Math.random() * 0.5);
            const fragmentSize = 4 + Math.random() * 6;

            fragment.style.setProperty('--fragment-x', `${Math.cos(angle) * distance}px`);
            fragment.style.setProperty('--fragment-y', `${Math.sin(angle) * distance}px`);
            fragment.style.setProperty('--fragment-rotation', `${(Math.random() - 0.5) * 1080}deg`);
            fragment.style.width = `${fragmentSize}px`;
            fragment.style.height = `${fragmentSize}px`;
            fragment.style.animationDuration = `${EXPLOSION_DURATION_MS + Math.random() * 250}ms`;
            explosionElement.appendChild(fragment);
        }

        const container = document.getElementById('bubble-container');
        container.appendChild(explosionElement);
        this.explosionElement = explosionElement;

        this.element.remove();

        this.explosionTimeoutId = setTimeout(() => {
            if (explosionElement.parentNode) {
                explosionElement.parentNode.removeChild(explosionElement);
            }
            this.explosionElement = null;
            this.explosionTimeoutId = null;

            this.state = 'destroyed';
            if (this.onBubbleExploded) {
                this.onBubbleExploded(this);
            }
        }, EXPLOSION_DURATION_MS + 300);
    }

    isActive() {
        return this.state === 'active';
    }

    update() {
        if (!this.isActive()) {
            return;
        }

        this.x += this.vx;
        this.y += this.vy;

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

        this.element.style.transform = `translate3d(${this.x}px, ${this.y}px, 0)`;
    }

    getCenterX() {
        return this.x + this.radius;
    }

    getCenterY() {
        return this.y + this.radius;
    }

    destroy() {
        this.state = 'destroyed';

        if (this.explosionTimeoutId !== null) {
            clearTimeout(this.explosionTimeoutId);
            this.explosionTimeoutId = null;
        }

        if (this.boundClickHandler) {
            this.element.removeEventListener('click', this.boundClickHandler);
        }

        if (this.element.parentNode) {
            this.element.parentNode.removeChild(this.element);
        }

        if (this.explosionElement && this.explosionElement.parentNode) {
            this.explosionElement.parentNode.removeChild(this.explosionElement);
            this.explosionElement = null;
        }
    }
}

function getDistance(bubbleA, bubbleB) {
    const dx = bubbleA.getCenterX() - bubbleB.getCenterX();
    const dy = bubbleA.getCenterY() - bubbleB.getCenterY();
    return Math.sqrt(dx * dx + dy * dy);
}

export function checkCollision(bubble1, bubble2) {
    const distance = getDistance(bubble1, bubble2);
    const minDistance = bubble1.radius + bubble2.radius;
    return distance < minDistance;
}

export function resolveCollision(bubble1, bubble2) {
    const dx = bubble2.getCenterX() - bubble1.getCenterX();
    const dy = bubble2.getCenterY() - bubble1.getCenterY();
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance === 0 || !isFinite(distance)) {
        return;
    }

    const nx = dx / distance;
    const ny = dy / distance;

    const dvx = bubble2.vx - bubble1.vx;
    const dvy = bubble2.vy - bubble1.vy;
    const dotProduct = dvx * nx + dvy * ny;

    if (dotProduct > 0) {
        return;
    }

    const impulse = dotProduct;
    bubble1.vx += impulse * nx;
    bubble1.vy += impulse * ny;
    bubble2.vx -= impulse * nx;
    bubble2.vy -= impulse * ny;

    const overlap = bubble1.radius + bubble2.radius - distance;
    const separationX = (overlap / 2) * nx;
    const separationY = (overlap / 2) * ny;

    bubble1.x -= separationX;
    bubble1.y -= separationY;
    bubble2.x += separationX;
    bubble2.y += separationY;
}
