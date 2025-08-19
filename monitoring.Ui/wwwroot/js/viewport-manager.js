/**
 * ViewportManager - Handles canvas zooming, panning, and viewport transformations
 */
class ViewportManager {
    constructor(viewport, canvas) {
        this.viewport = viewport;
        this.canvas = canvas;
        
        // Zoom settings
        this.minZoom = 0.1;
        this.maxZoom = 5;
        this.zoomStep = 0.1;
        this.currentZoom = 1;
        
        // Pan settings
        this.isPanning = false;
        this.panStartX = 0;
        this.panStartY = 0;
        this.panX = 0;
        this.panY = 0;
        
        // Canvas dimensions
        this.canvasWidth = 1920;
        this.canvasHeight = 1080;
        
        // Viewport dimensions
        this.viewportWidth = 0;
        this.viewportHeight = 0;
        
        this.init();
    }
    
    init() {
        this.updateViewportDimensions();
        this.calculateFitToViewport();
        this.setupEventListeners();
        this.updateTransform();
        
        // Listen for window resize
        window.addEventListener('resize', () => {
            this.updateViewportDimensions();
            this.calculateFitToViewport();
        });
    }
    
    updateViewportDimensions() {
        const rect = this.viewport.getBoundingClientRect();
        this.viewportWidth = rect.width;
        this.viewportHeight = rect.height;
    }
    
    calculateFitToViewport() {
        this.updateViewportDimensions();
        
        const scaleX = this.viewportWidth / this.canvasWidth;
        const scaleY = this.viewportHeight / this.canvasHeight;
        this.fitZoom = Math.min(scaleX, scaleY) * 0.9; // 90% to leave some margin
        
        // Set initial zoom to fit
        if (this.currentZoom === 1) {
            this.currentZoom = this.fitZoom;
            this.centerCanvas();
        }
    }
    
    setupEventListeners() {
        // Mouse wheel for zooming
        this.viewport.addEventListener('wheel', (e) => {
            e.preventDefault();
            
            const rect = this.viewport.getBoundingClientRect();
            const mouseX = e.clientX - rect.left;
            const mouseY = e.clientY - rect.top;
            
            if (e.deltaY < 0) {
                this.zoomIn(mouseX, mouseY);
            } else {
                this.zoomOut(mouseX, mouseY);
            }
        });
        
        // Pan with mouse drag
        this.viewport.addEventListener('mousedown', (e) => {
            // Only pan on background (not on elements)
            if (e.target === this.viewport || e.target === this.canvas) {
                this.startPan(e);
            }
        });
        
        document.addEventListener('mousemove', (e) => {
            if (this.isPanning) {
                this.updatePan(e);
            }
        });
        
        document.addEventListener('mouseup', () => {
            this.endPan();
        });
        
        // Pan with space key + drag
        document.addEventListener('keydown', (e) => {
            if (e.code === 'Space' && !this.isPanning) {
                e.preventDefault();
                this.viewport.classList.add('pan-mode');
            }
        });
        
        document.addEventListener('keyup', (e) => {
            if (e.code === 'Space') {
                this.viewport.classList.remove('pan-mode');
            }
        });
        
        // Handle space+drag
        this.viewport.addEventListener('mousedown', (e) => {
            if (e.code === 'Space' || this.viewport.classList.contains('pan-mode')) {
                this.startPan(e);
            }
        });
    }
    
    startPan(e) {
        this.isPanning = true;
        this.panStartX = e.clientX - this.panX;
        this.panStartY = e.clientY - this.panY;
        this.viewport.classList.add('panning');
        e.preventDefault();
    }
    
    updatePan(e) {
        if (!this.isPanning) return;
        
        const newPanX = e.clientX - this.panStartX;
        const newPanY = e.clientY - this.panStartY;
        
        // Constrain pan to keep canvas visible
        const scaledWidth = this.canvasWidth * this.currentZoom;
        const scaledHeight = this.canvasHeight * this.currentZoom;
        
        const maxPanX = Math.max(0, (scaledWidth - this.viewportWidth) / 2);
        const maxPanY = Math.max(0, (scaledHeight - this.viewportHeight) / 2);
        
        this.panX = Math.max(-maxPanX, Math.min(maxPanX, newPanX));
        this.panY = Math.max(-maxPanY, Math.min(maxPanY, newPanY));
        
        this.updateTransform();
    }
    
    endPan() {
        this.isPanning = false;
        this.viewport.classList.remove('panning');
    }
    
    zoomIn(mouseX = null, mouseY = null) {
        const newZoom = Math.min(this.maxZoom, this.currentZoom + this.zoomStep);
        this.setZoom(newZoom, mouseX, mouseY);
    }
    
    zoomOut(mouseX = null, mouseY = null) {
        const newZoom = Math.max(this.minZoom, this.currentZoom - this.zoomStep);
        this.setZoom(newZoom, mouseX, mouseY);
    }
    
    setZoom(zoom, mouseX = null, mouseY = null) {
        if (zoom === this.currentZoom) return;
        
        // If mouse position is provided, zoom to that point
        if (mouseX !== null && mouseY !== null) {
            // Calculate the point on canvas before zoom
            const canvasX = (mouseX - this.panX) / this.currentZoom;
            const canvasY = (mouseY - this.panY) / this.currentZoom;
            
            // Update zoom
            this.currentZoom = zoom;
            
            // Calculate new pan to keep the same point under mouse
            this.panX = mouseX - canvasX * this.currentZoom;
            this.panY = mouseY - canvasY * this.currentZoom;
            
            // Constrain pan
            this.constrainPan();
        } else {
            this.currentZoom = zoom;
        }
        
        this.updateTransform();
        this.onZoomChange();
    }
    
    fitToViewport() {
        this.calculateFitToViewport();
        this.currentZoom = this.fitZoom;
        this.centerCanvas();
        this.updateTransform();
        this.onZoomChange();
    }
    
    actualSize() {
        this.currentZoom = 1;
        this.centerCanvas();
        this.updateTransform();
        this.onZoomChange();
    }
    
    centerCanvas() {
        const scaledWidth = this.canvasWidth * this.currentZoom;
        const scaledHeight = this.canvasHeight * this.currentZoom;
        
        this.panX = (this.viewportWidth - scaledWidth) / 2;
        this.panY = (this.viewportHeight - scaledHeight) / 2;
        
        this.constrainPan();
    }
    
    constrainPan() {
        const scaledWidth = this.canvasWidth * this.currentZoom;
        const scaledHeight = this.canvasHeight * this.currentZoom;
        
        const maxPanX = Math.max(0, (scaledWidth - this.viewportWidth) / 2);
        const maxPanY = Math.max(0, (scaledHeight - this.viewportHeight) / 2);
        
        this.panX = Math.max(-maxPanX, Math.min(maxPanX, this.panX));
        this.panY = Math.max(-maxPanY, Math.min(maxPanY, this.panY));
    }
    
    updateTransform() {
        const transform = `translate(${this.panX}px, ${this.panY}px) scale(${this.currentZoom})`;
        this.canvas.style.transform = transform;
    }
    
    // Convert viewport coordinates to canvas coordinates
    viewportToCanvas(viewportX, viewportY) {
        const canvasX = (viewportX - this.panX) / this.currentZoom;
        const canvasY = (viewportY - this.panY) / this.currentZoom;
        return { x: canvasX, y: canvasY };
    }
    
    // Convert canvas coordinates to viewport coordinates
    canvasToViewport(canvasX, canvasY) {
        const viewportX = canvasX * this.currentZoom + this.panX;
        const viewportY = canvasY * this.currentZoom + this.panY;
        return { x: viewportX, y: viewportY };
    }
    
    // Get current zoom percentage
    getZoomPercentage() {
        return Math.round(this.currentZoom * 100);
    }
    
    // Set canvas dimensions
    setCanvasDimensions(width, height) {
        this.canvasWidth = width;
        this.canvasHeight = height;
        this.canvas.style.width = width + 'px';
        this.canvas.style.height = height + 'px';
        this.calculateFitToViewport();
        this.updateTransform();
    }
    
    // Callback for zoom changes (to update UI)
    onZoomChange() {
        // Override this method to handle zoom change events
        if (this.onZoomChangeCallback) {
            this.onZoomChangeCallback(this.getZoomPercentage());
        }
    }
    
    // Set zoom change callback
    setZoomChangeCallback(callback) {
        this.onZoomChangeCallback = callback;
    }
}
