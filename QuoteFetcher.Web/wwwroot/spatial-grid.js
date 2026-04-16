export class SpatialGrid {
    constructor(cellSize) {
        this.cellSize = cellSize;
        this.grid = new Map();
    }

    clear() {
        this.grid.clear();
    }

    getKey(x, y) {
        return `${x},${y}`;
    }

    getCellCoords(worldX, worldY) {
        return {
            x: Math.floor(worldX / this.cellSize),
            y: Math.floor(worldY / this.cellSize)
        };
    }

    insert(bubble) {
        const cell = this.getCellCoords(bubble.getCenterX(), bubble.getCenterY());
        const key = this.getKey(cell.x, cell.y);

        if (!this.grid.has(key)) {
            this.grid.set(key, []);
        }

        this.grid.get(key).push(bubble);
    }

    getNearby(bubble) {
        const cell = this.getCellCoords(bubble.getCenterX(), bubble.getCenterY());
        const nearby = [];

        for (let dx = -1; dx <= 1; dx++) {
            for (let dy = -1; dy <= 1; dy++) {
                const key = this.getKey(cell.x + dx, cell.y + dy);
                const cellBubbles = this.grid.get(key);
                if (cellBubbles) {
                    nearby.push(...cellBubbles);
                }
            }
        }

        return nearby;
    }
}
