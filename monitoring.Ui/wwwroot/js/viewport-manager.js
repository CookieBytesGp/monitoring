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
        
        // Coordinate tracking
        this.coordinateDisplay = document.getElementById('coordinate-display');
        this.coordX = document.getElementById('coord-x');
        this.coordY = document.getElementById('coord-y');
        this.isTrackingCoordinates = false;
        
        // Zoom controls dragging
        this.zoomControls = document.getElementById('zoom-controls');
        this.isZoomControlsDragging = false;
        this.zoomControlsOffset = { x: 0, y: 0 };
        
        this.init();
    }
    
    init() {
        this.updateViewportDimensions();
        this.calculateFitToViewport();
        this.setupEventListeners();
        this.setupZoomControlsDragging();
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
        this.fitZoom = Math.min(scaleX, scaleY) * 0.8; // 80% to leave margin
        
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
            
            // Update coordinates when dragging elements
            if (this.isTrackingCoordinates) {
                this.updateCoordinateDisplay(e);
            }
        });
        
        document.addEventListener('mouseup', () => {
            this.endPan();
            this.hideCoordinateDisplay();
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
        // Apply transform while preserving CSS centering
        const transform = `translate(-50%, -50%) translate(${this.panX}px, ${this.panY}px) scale(${this.currentZoom})`;
        this.canvas.style.transform = transform;
    }
    
    // Convert viewport coordinates to canvas coordinates
    viewportToCanvas(viewportX, viewportY) {
        // Get canvas position relative to viewport
        const rect = this.canvas.getBoundingClientRect();
        const viewportRect = this.viewport.getBoundingClientRect();
        
        // Calculate relative position within canvas
        const relativeX = viewportX - (rect.left - viewportRect.left);
        const relativeY = viewportY - (rect.top - viewportRect.top);
        
        // Convert to canvas coordinates accounting for scale
        const canvasX = relativeX / this.currentZoom;
        const canvasY = relativeY / this.currentZoom;
        
        return { x: canvasX, y: canvasY };
    }
    
    // Convert canvas coordinates to viewport coordinates
    canvasToViewport(canvasX, canvasY) {
        // Get canvas position relative to viewport
        const rect = this.canvas.getBoundingClientRect();
        const viewportRect = this.viewport.getBoundingClientRect();
        
        // Convert to viewport coordinates
        const viewportX = (canvasX * this.currentZoom) + (rect.left - viewportRect.left);
        const viewportY = (canvasY * this.currentZoom) + (rect.top - viewportRect.top);
        
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
    
    // Coordinate tracking methods
    startCoordinateTracking() {
        this.isTrackingCoordinates = true;
        this.showCoordinateDisplay();
    }
    
    stopCoordinateTracking() {
        this.isTrackingCoordinates = false;
        this.hideCoordinateDisplay();
    }
    
    updateCoordinateDisplay(event) {
        if (!this.coordinateDisplay) return;
        
        const rect = this.viewport.getBoundingClientRect();
        const viewportX = event.clientX - rect.left;
        const viewportY = event.clientY - rect.top;
        
        // Convert to canvas coordinates
        const canvasCoords = this.viewportToCanvas(viewportX, viewportY);
        
        // Update display elements
        if (this.coordX) this.coordX.textContent = Math.round(canvasCoords.x);
        if (this.coordY) this.coordY.textContent = Math.round(canvasCoords.y);
    }
    
    showCoordinateDisplay() {
        if (this.coordinateDisplay) {
            this.coordinateDisplay.classList.add('visible');
        }
    }
    
    hideCoordinateDisplay() {
        if (this.coordinateDisplay) {
            this.coordinateDisplay.classList.remove('visible');
        }
    }
    
    // Zoom controls dragging
    setupZoomControlsDragging() {
        if (!this.zoomControls) return;
        
        this.zoomControls.addEventListener('mousedown', (e) => {
            // Only start dragging if clicking on the drag handle (left area)
            const rect = this.zoomControls.getBoundingClientRect();
            const clickX = e.clientX - rect.left;
            
            if (clickX <= 20) { // Click on drag handle area
                e.preventDefault();
                this.startZoomControlsDrag(e);
            }
        });
        
        document.addEventListener('mousemove', (e) => {
            if (this.isZoomControlsDragging) {
                this.updateZoomControlsDrag(e);
            }
        });
        
        document.addEventListener('mouseup', () => {
            this.endZoomControlsDrag();
        });
    }
    
    startZoomControlsDrag(event) {
        this.isZoomControlsDragging = true;
        this.zoomControls.classList.add('dragging');
        
        const rect = this.zoomControls.getBoundingClientRect();
        this.zoomControlsOffset.x = event.clientX - rect.left;
        this.zoomControlsOffset.y = event.clientY - rect.top;
        
        document.body.style.userSelect = 'none';
    }
    
    updateZoomControlsDrag(event) {
        const x = event.clientX - this.zoomControlsOffset.x;
        const y = event.clientY - this.zoomControlsOffset.y;
        
        // Keep within viewport bounds
        const maxX = window.innerWidth - this.zoomControls.offsetWidth;
        const maxY = window.innerHeight - this.zoomControls.offsetHeight;
        
        const constrainedX = Math.max(0, Math.min(maxX, x));
        const constrainedY = Math.max(0, Math.min(maxY, y));
        
        this.zoomControls.style.left = constrainedX + 'px';
        this.zoomControls.style.top = constrainedY + 'px';
        this.zoomControls.style.right = 'auto';
        this.zoomControls.style.bottom = 'auto';
    }
    
    endZoomControlsDrag() {
        this.isZoomControlsDragging = false;
        this.zoomControls.classList.remove('dragging');
        document.body.style.userSelect = '';
    }
}
