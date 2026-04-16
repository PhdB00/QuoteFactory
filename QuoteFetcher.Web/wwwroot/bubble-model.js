const BUBBLE_SPEED = 2;
export const BUBBLE_SIZE = 60;

const DEFAULT_VFX_CONFIG = {
    preset: 'arcade_punchy',
    seed: 1337,
    allowAudioOverlap: false,
    respawnDelayMs: 550,
    explosionDurationMs: 600,
    clickFeedbackDurationMs: 100
};

const PARTICLE_HARD_CAPS = {
    chunks: 16,
    sparks: 48
};
const PARTICLE_SPEED_SCALE = 3.4;

const VFX_PRESETS = {
    arcade_punchy: {
        flashStrength: 1,
        chunks: {
            minCount: 10,
            maxCount: 14,
            minSize: 10,
            maxSize: 19,
            minSpeed: 7,
            maxSpeed: 12,
            drag: 0.92,
            minLifeRatio: 0.82,
            maxLifeRatio: 1
        },
        sparks: {
            minCount: 30,
            maxCount: 40,
            minSize: 2,
            maxSize: 4,
            minSpeed: 13,
            maxSpeed: 23,
            drag: 0.86,
            minLifeRatio: 0.52,
            maxLifeRatio: 0.85
        },
        audio: {
            baseFrequency: 170,
            durationMs: 220,
            volume: 0.18
        }
    },
    arcade_heavy: {
        flashStrength: 0.85,
        chunks: {
            minCount: 10,
            maxCount: 13,
            minSize: 12,
            maxSize: 22,
            minSpeed: 5,
            maxSpeed: 9,
            drag: 0.95,
            minLifeRatio: 0.85,
            maxLifeRatio: 1
        },
        sparks: {
            minCount: 28,
            maxCount: 36,
            minSize: 2,
            maxSize: 4,
            minSpeed: 10,
            maxSpeed: 18,
            drag: 0.88,
            minLifeRatio: 0.5,
            maxLifeRatio: 0.8
        },
        audio: {
            baseFrequency: 145,
            durationMs: 240,
            volume: 0.2
        }
    },
    arcade_fizz: {
        flashStrength: 1.15,
        chunks: {
            minCount: 10,
            maxCount: 12,
            minSize: 8,
            maxSize: 16,
            minSpeed: 7,
            maxSpeed: 11,
            drag: 0.9,
            minLifeRatio: 0.72,
            maxLifeRatio: 0.92
        },
        sparks: {
            minCount: 36,
            maxCount: 48,
            minSize: 1,
            maxSize: 4,
            minSpeed: 15,
            maxSpeed: 26,
            drag: 0.84,
            minLifeRatio: 0.5,
            maxLifeRatio: 0.82
        },
        audio: {
            baseFrequency: 205,
            durationMs: 210,
            volume: 0.16
        }
    }
};

let explosionSequence = 0;

function clamp(value, min, max) {
    return Math.max(min, Math.min(max, value));
}

function lerp(start, end, t) {
    return start + (end - start) * t;
}

function smoothStep01(t) {
    return t * t * (3 - 2 * t);
}

function blendColor(colorA, colorB, t) {
    const blend = clamp(t, 0, 1);
    const r = Math.round(lerp(colorA.r, colorB.r, blend));
    const g = Math.round(lerp(colorA.g, colorB.g, blend));
    const b = Math.round(lerp(colorA.b, colorB.b, blend));
    return `rgb(${r}, ${g}, ${b})`;
}

function getWarmPaletteColor(progress) {
    const white = { r: 255, g: 245, b: 190 };
    const orange = { r: 255, g: 143, b: 46 };
    const red = { r: 227, g: 58, b: 33 };

    if (progress < 0.25) {
        return blendColor(white, orange, progress / 0.25);
    }

    if (progress < 0.72) {
        return blendColor(orange, red, (progress - 0.25) / 0.47);
    }

    return blendColor(red, { r: 150, g: 24, b: 18 }, (progress - 0.72) / 0.28);
}

function hashSeed(baseSeed, bubbleId, sequence) {
    let hash = baseSeed >>> 0;
    hash ^= (bubbleId + 0x9e3779b9 + (hash << 6) + (hash >>> 2)) >>> 0;
    hash ^= (sequence + 0x85ebca6b + (hash << 7) + (hash >>> 3)) >>> 0;
    return hash >>> 0;
}

function createSeededRandom(seed) {
    let value = seed >>> 0;
    return () => {
        value = (value + 0x6d2b79f5) | 0;
        let t = Math.imul(value ^ (value >>> 15), 1 | value);
        t ^= t + Math.imul(t ^ (t >>> 7), 61 | t);
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}

function randomRange(random, min, max) {
    return min + (max - min) * random();
}

function randomInteger(random, min, max) {
    return Math.floor(randomRange(random, min, max + 1));
}

function createParticleElement(type, shape) {
    const particle = document.createElement('span');
    particle.className = `bubble-fragment bubble-particle bubble-particle-${type} bubble-particle-${shape}`;
    return particle;
}

function createLayerParticles(layerName, layerConfig, hardCap, durationMs, random) {
    const particles = [];
    const count = clamp(randomInteger(random, layerConfig.minCount, layerConfig.maxCount), 0, hardCap);

    for (let index = 0; index < count; index++) {
        const angle = random() * Math.PI * 2;
        const speed = randomRange(random, layerConfig.minSpeed, layerConfig.maxSpeed);
        const spreadNudge = (random() - 0.5) * 0.2;
        const vx = Math.cos(angle + spreadNudge) * speed;
        const vy = Math.sin(angle + spreadNudge) * speed;
        const lifeRatio = randomRange(random, layerConfig.minLifeRatio, layerConfig.maxLifeRatio);
        const lifeMs = durationMs * lifeRatio;
        const size = randomRange(random, layerConfig.minSize, layerConfig.maxSize);
        const curveStrength = (random() - 0.5) * (layerName === 'chunks' ? 0.7 : 1.2);
        const curveDirection = random() > 0.5 ? 1 : -1;
        const shape = layerName === 'chunks'
            ? (random() > 0.35 ? 'circle' : 'streak')
            : (random() > 0.7 ? 'streak' : 'point');

        particles.push({
            type: layerName,
            x: Math.cos(angle) * randomRange(random, 4, 12),
            y: Math.sin(angle) * randomRange(random, 4, 12),
            vx: vx * PARTICLE_SPEED_SCALE,
            vy: vy * PARTICLE_SPEED_SCALE,
            drag: layerConfig.drag,
            size,
            lifeMs,
            elapsedMs: 0,
            rotationDeg: randomRange(random, -35, 35),
            spinDegPerFrame: randomRange(random, -16, 16),
            curveStrength,
            curveDirection,
            element: createParticleElement(layerName, shape),
            shape
        });
    }

    return particles;
}

function configureParticleAppearance(particle) {
    let width = particle.size;
    let height = particle.size;

    if (particle.shape === 'streak') {
        width = particle.size * 1.7;
        height = Math.max(2, particle.size * 0.4);
    }

    particle.element.style.width = `${width}px`;
    particle.element.style.height = `${height}px`;
    particle.originOffsetX = width / 2;
    particle.originOffsetY = height / 2;
}

class ExplosionAudioManager {
    constructor(vfxConfig) {
        this.allowOverlap = Boolean(vfxConfig.allowAudioOverlap);
        this.audioPreset = vfxConfig.presetConfig.audio;
        this.activeSource = null;
        this.audioContext = null;
        this.activeUntilMs = 0;
    }

    getAudioContext() {
        if (this.audioContext) {
            return this.audioContext;
        }

        const AudioContextClass = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextClass) {
            return null;
        }

        this.audioContext = new AudioContextClass();
        return this.audioContext;
    }

    dispatchAudioEvent(action, metadata = null) {
        const detail = metadata ? { action, ...metadata } : { action };
        document.dispatchEvent(new CustomEvent('bubble-vfx-audio', {
            detail
        }));
    }

    prime() {
        const context = this.getAudioContext();
        if (!context) {
            this.dispatchAudioEvent('unsupported');
            return Promise.resolve(false);
        }

        if (context.state === 'running') {
            this.dispatchAudioEvent('primed');
            return Promise.resolve(true);
        }

        return context.resume().then(() => {
            if (context.state === 'running') {
                this.dispatchAudioEvent('primed');
                return true;
            }

            this.dispatchAudioEvent('blocked', {
                phase: 'prime',
                state: context.state
            });
            return false;
        }).catch((error) => {
            this.dispatchAudioEvent('blocked', {
                phase: 'prime',
                reason: error?.name || 'resume_failed'
            });
            return false;
        });
    }

    play() {
        const nowMs = performance.now();
        const durationMs = this.audioPreset.durationMs;

        if (!this.allowOverlap && this.activeSource && nowMs < this.activeUntilMs) {
            this.dispatchAudioEvent('dropped');
            return false;
        }

        const context = this.getAudioContext();
        if (!context) {
            this.dispatchAudioEvent('unsupported');
            return false;
        }

        if (context.state !== 'running') {
            this.dispatchAudioEvent('blocked', {
                phase: 'play',
                state: context.state
            });
            return false;
        }

        const oscillator = context.createOscillator();
        const gainNode = context.createGain();

        try {
            oscillator.type = 'triangle';
            oscillator.frequency.setValueAtTime(this.audioPreset.baseFrequency, context.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(this.audioPreset.baseFrequency * 0.58, context.currentTime + (durationMs / 1000));

            gainNode.gain.setValueAtTime(0.001, context.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(this.audioPreset.volume, context.currentTime + 0.015);
            gainNode.gain.exponentialRampToValueAtTime(0.001, context.currentTime + (durationMs / 1000));

            oscillator.connect(gainNode);
            gainNode.connect(context.destination);

            oscillator.onended = () => {
                if (this.activeSource === oscillator) {
                    this.activeSource = null;
                }
            };

            oscillator.start();
            oscillator.stop(context.currentTime + (durationMs / 1000));
        } catch (error) {
            this.dispatchAudioEvent('error', {
                phase: 'play',
                reason: error?.name || 'play_failed'
            });
            return false;
        }

        this.activeSource = oscillator;
        this.activeUntilMs = nowMs + durationMs;
        this.dispatchAudioEvent('played');
        return true;
    }
}

function createExplosionEngine({
    container,
    centerX,
    centerY,
    vfxConfig,
    seed
}) {
    const root = document.createElement('div');
    root.className = 'bubble-explosion';
    root.style.transform = `translate3d(${centerX}px, ${centerY}px, 0)`;

    const flash = document.createElement('span');
    flash.className = 'bubble-explosion-flash';
    flash.style.setProperty('--flash-strength', vfxConfig.presetConfig.flashStrength.toFixed(2));
    root.appendChild(flash);

    const random = createSeededRandom(seed);
    const chunks = createLayerParticles('chunks', vfxConfig.presetConfig.chunks, PARTICLE_HARD_CAPS.chunks, vfxConfig.explosionDurationMs, random);
    const sparks = createLayerParticles('sparks', vfxConfig.presetConfig.sparks, PARTICLE_HARD_CAPS.sparks, vfxConfig.explosionDurationMs, random);
    const particles = chunks.concat(sparks);

    for (const particle of particles) {
        configureParticleAppearance(particle);
        root.appendChild(particle.element);
    }

    container.appendChild(root);

    let frameId = null;
    let lastTick = null;
    let elapsedMs = 0;

    const step = (timestamp) => {
        if (lastTick === null) {
            lastTick = timestamp;
        }

        const deltaMs = clamp(timestamp - lastTick, 0, 34);
        lastTick = timestamp;
        elapsedMs += deltaMs;

        const globalProgress = clamp(elapsedMs / vfxConfig.explosionDurationMs, 0, 1);

        for (const particle of particles) {
            particle.elapsedMs += deltaMs;
            const localProgress = clamp(particle.elapsedMs / particle.lifeMs, 0, 1);
            if (localProgress >= 1) {
                particle.element.style.opacity = '0';
                continue;
            }

            const frameScale = deltaMs / 16.666;
            const dragScale = Math.pow(particle.drag, frameScale);
            const pathEase = 1 - (1 - localProgress) * (1 - localProgress);

            particle.vx *= dragScale;
            particle.vy *= dragScale;

            const curveX = particle.curveStrength * particle.curveDirection * (1 - pathEase) * frameScale;
            const curveY = particle.curveStrength * -particle.curveDirection * (1 - pathEase) * frameScale;

            particle.x += (particle.vx + curveX) * frameScale;
            particle.y += (particle.vy + curveY) * frameScale;
            particle.rotationDeg += particle.spinDegPerFrame * frameScale;

            const color = getWarmPaletteColor(localProgress);
            const fadeStart = particle.type === 'chunks' ? 0.68 : 0.5;
            const fadeProgress = localProgress <= fadeStart ? 0 : (localProgress - fadeStart) / (1 - fadeStart);
            const opacity = clamp(1 - smoothStep01(fadeProgress), 0, 1);
            const scale = particle.type === 'chunks'
                ? lerp(1.06, 0.7, pathEase)
                : lerp(1, 0.45, pathEase);

            particle.element.style.backgroundColor = color;
            particle.element.style.opacity = opacity.toFixed(3);
            particle.element.style.transform = `translate3d(${(particle.x - particle.originOffsetX).toFixed(2)}px, ${(particle.y - particle.originOffsetY).toFixed(2)}px, 0) rotate(${particle.rotationDeg.toFixed(2)}deg) scale(${scale.toFixed(3)})`;
        }

        const ignitionProgress = clamp(elapsedMs / 100, 0, 1);
        const flashOpacity = clamp(1 - smoothStep01(ignitionProgress), 0, 1);
        flash.style.opacity = flashOpacity.toFixed(3);

        if (globalProgress >= 1) {
            return;
        }

        frameId = requestAnimationFrame(step);
    };

    frameId = requestAnimationFrame(step);

    return {
        destroy() {
            if (frameId !== null) {
                cancelAnimationFrame(frameId);
                frameId = null;
            }

            if (root.parentNode) {
                root.parentNode.removeChild(root);
            }
        },
        root,
        chunksCount: chunks.length,
        sparksCount: sparks.length
    };
}

export function resolveBubbleVfxConfig(config) {
    const requestedPreset = config?.preset || DEFAULT_VFX_CONFIG.preset;
    const presetKey = Object.prototype.hasOwnProperty.call(VFX_PRESETS, requestedPreset)
        ? requestedPreset
        : DEFAULT_VFX_CONFIG.preset;
    const presetConfig = VFX_PRESETS[presetKey];

    const explosionDurationMs = clamp(
        config?.explosionDurationMs ?? DEFAULT_VFX_CONFIG.explosionDurationMs,
        300,
        1200
    );

    const respawnDelayMs = clamp(
        config?.respawnDelayMs ?? DEFAULT_VFX_CONFIG.respawnDelayMs,
        0,
        explosionDurationMs
    );

    return {
        preset: presetKey,
        presetConfig,
        seed: Number.isFinite(config?.seed) ? Number(config.seed) : DEFAULT_VFX_CONFIG.seed,
        allowAudioOverlap: Boolean(config?.allowAudioOverlap ?? DEFAULT_VFX_CONFIG.allowAudioOverlap),
        respawnDelayMs,
        explosionDurationMs,
        clickFeedbackDurationMs: clamp(
            config?.clickFeedbackDurationMs ?? DEFAULT_VFX_CONFIG.clickFeedbackDurationMs,
            0,
            300
        )
    };
}

export function createExplosionAudioManager(vfxConfig) {
    return new ExplosionAudioManager(vfxConfig);
}

export class Bubble {
    constructor({
        id,
        category,
        x,
        y,
        onBubbleClick,
        onBubbleRespawnRequested,
        onBubbleExploded,
        bubbleVfxConfig,
        explosionAudioManager
    }) {
        this.id = id;
        this.category = category;
        this.x = x;
        this.y = y;
        this.vx = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.vy = (Math.random() - 0.5) * BUBBLE_SPEED;
        this.onBubbleClick = onBubbleClick;
        this.onBubbleRespawnRequested = onBubbleRespawnRequested;
        this.onBubbleExploded = onBubbleExploded;
        this.vfxConfig = resolveBubbleVfxConfig(bubbleVfxConfig || DEFAULT_VFX_CONFIG);
        this.explosionAudioManager = explosionAudioManager || new ExplosionAudioManager(this.vfxConfig);
        this.state = 'active';
        this.clickTimeoutId = null;
        this.respawnTimeoutId = null;
        this.explosionTimeoutId = null;
        this.explosionEngine = null;
        this.hasRequestedRespawn = false;
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

        this.explosionAudioManager.prime().catch(() => {
            // Prime failures are captured via telemetry events.
        });

        this.state = 'clicked';
        this.element.classList.add('clicked');
        this.clickTimeoutId = setTimeout(() => {
            this.clickTimeoutId = null;
            if (this.element) {
                this.element.classList.remove('clicked');
            }

            this.startExplosion();
        }, this.vfxConfig.clickFeedbackDurationMs);

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

        const container = document.getElementById('bubble-container');
        const sequence = explosionSequence++;
        const seed = hashSeed(this.vfxConfig.seed, this.id, sequence);

        this.explosionEngine = createExplosionEngine({
            container,
            centerX: this.getCenterX(),
            centerY: this.getCenterY(),
            vfxConfig: this.vfxConfig,
            seed
        });

        if (this.explosionEngine && this.explosionEngine.root) {
            this.explosionEngine.root.dataset.chunksCount = String(this.explosionEngine.chunksCount);
            this.explosionEngine.root.dataset.sparksCount = String(this.explosionEngine.sparksCount);
            this.explosionEngine.root.dataset.seed = String(seed);
        }

        this.explosionAudioManager.play();

        this.element.remove();

        this.respawnTimeoutId = setTimeout(() => {
            this.respawnTimeoutId = null;
            if (this.state !== 'exploding' || this.hasRequestedRespawn) {
                return;
            }

            this.hasRequestedRespawn = true;
            if (this.onBubbleRespawnRequested) {
                this.onBubbleRespawnRequested(this);
            }
        }, this.vfxConfig.respawnDelayMs);

        this.explosionTimeoutId = setTimeout(() => {
            this.explosionTimeoutId = null;
            this.state = 'destroyed';

            if (this.explosionEngine) {
                this.explosionEngine.destroy();
                this.explosionEngine = null;
            }

            if (this.onBubbleExploded) {
                this.onBubbleExploded(this);
            }
        }, this.vfxConfig.explosionDurationMs);
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

        if (this.clickTimeoutId !== null) {
            clearTimeout(this.clickTimeoutId);
            this.clickTimeoutId = null;
        }

        if (this.respawnTimeoutId !== null) {
            clearTimeout(this.respawnTimeoutId);
            this.respawnTimeoutId = null;
        }

        if (this.explosionTimeoutId !== null) {
            clearTimeout(this.explosionTimeoutId);
            this.explosionTimeoutId = null;
        }

        if (this.boundClickHandler && this.element) {
            this.element.removeEventListener('click', this.boundClickHandler);
        }

        if (this.element && this.element.parentNode) {
            this.element.parentNode.removeChild(this.element);
        }

        if (this.explosionEngine) {
            this.explosionEngine.destroy();
            this.explosionEngine = null;
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
