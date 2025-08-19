/**
 * Page Editor Main JavaScript Classes - Updated with Viewport Management
 */

/**
 * Element Manager - Handles element creation, modification, and deletion
 */
class ElementManager {
    constructor(editor) {
        this.editor = editor;
    }

    createElement(type, config) {
        const id = this.generateElementId();
        const element = {
            id: id,
            type: type,
            config: { ...config },
            domElement: this.createDOMElement(type, config, id)
        };

        this.editor.elements.set(id, element);
        
        // Add to page content container
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            pageContent.appendChild(element.domElement);
        } else {
            this.editor.canvas.appendChild(element.domElement);
        }
        
        // Setup element interactions
        this.setupElementInteractions(element);
        
        return element;
    }

    createDOMElement(type, config, id) {
        const element = document.createElement('div');
        element.className = `page-element element-${type}`;
        element.dataset.elementId = id;
        element.dataset.elementType = type;
        
        // Set position and size
        element.style.position = 'absolute';
        element.style.left = config.x + 'px';
        element.style.top = config.y + 'px';
        element.style.width = config.width + 'px';
        element.style.height = config.height + 'px';
        
        // Set content based on type
        this.setElementContent(element, type, config);
        
        return element;
    }

    setElementContent(element, type, config) {
        switch (type) {
            case 'text':
                element.innerHTML = `<div class="text-content" style="font-size: ${config.fontSize}px; color: ${config.color}; background-color: ${config.backgroundColor};">${config.content}</div>`;
                break;
            case 'image':
                element.innerHTML = `<img src="${config.src}" alt="${config.alt}" style="width: 100%; height: 100%; object-fit: cover;">`;
                break;
            case 'video':
                element.innerHTML = `<video src="${config.src}" ${config.autoplay ? 'autoplay' : ''} ${config.loop ? 'loop' : ''} controls style="width: 100%; height: 100%;"></video>`;
                break;
            case 'camera':
                element.innerHTML = `<div class="camera-placeholder"><i class="fas fa-camera"></i><span>${config.title}</span></div>`;
                break;
            case 'clock':
                element.innerHTML = `<div class="clock-widget" data-format="${config.format}" data-show-seconds="${config.showSeconds}"><div class="clock-time">00:00:00</div></div>`;
                this.initializeClock(element);
                break;
            case 'weather':
                element.innerHTML = `<div class="weather-widget" data-location="${config.location}"><div class="weather-info">Weather Widget</div></div>`;
                break;
        }
    }

    setupElementInteractions(element) {
        const domElement = element.domElement;
        
        // Click to select
        domElement.addEventListener('click', (e) => {
            e.stopPropagation();
            this.editor.selectionManager.selectElement(element);
        });
        
        // Drag functionality - need to account for viewport transformation
        domElement.addEventListener('mousedown', (e) => {
            if (e.button === 0) { // Left mouse button
                this.startDrag(element, e);
            }
        });
        
        // Make element resizable
        this.makeResizable(domElement);
    }

    startDrag(element, e) {
        e.preventDefault();
        e.stopPropagation();
        
        this.editor.isDragging = true;
        this.editor.draggedElement = element;
        
        // Convert viewport coordinates to canvas coordinates
        const rect = this.editor.viewport.getBoundingClientRect();
        const viewportX = e.clientX - rect.left;
        const viewportY = e.clientY - rect.top;
        const canvasCoords = this.editor.viewportToCanvas(viewportX, viewportY);
        
        this.editor.dragOffset = {
            x: canvasCoords.x - element.config.x,
            y: canvasCoords.y - element.config.y
        };
        
        // Add global mouse events
        document.addEventListener('mousemove', this.handleDragMove.bind(this));
        document.addEventListener('mouseup', this.handleDragEnd.bind(this));
        
        // Visual feedback
        element.domElement.style.zIndex = '1000';
        element.domElement.classList.add('dragging');
    }

    handleDragMove(e) {
        if (!this.editor.isDragging || !this.editor.draggedElement) return;
        
        const rect = this.editor.viewport.getBoundingClientRect();
        const viewportX = e.clientX - rect.left;
        const viewportY = e.clientY - rect.top;
        const canvasCoords = this.editor.viewportToCanvas(viewportX, viewportY);
        
        const newX = canvasCoords.x - this.editor.dragOffset.x;
        const newY = canvasCoords.y - this.editor.dragOffset.y;
        
        // Update element position
        this.updateElementPosition(this.editor.draggedElement, newX, newY);
    }

    handleDragEnd(e) {
        if (this.editor.draggedElement) {
            this.editor.draggedElement.domElement.style.zIndex = '';
            this.editor.draggedElement.domElement.classList.remove('dragging');
        }
        
        this.editor.isDragging = false;
        this.editor.draggedElement = null;
        
        // Remove global mouse events
        document.removeEventListener('mousemove', this.handleDragMove.bind(this));
        document.removeEventListener('mouseup', this.handleDragEnd.bind(this));
    }

    updateElementPosition(element, x, y) {
        element.config.x = Math.max(0, Math.min(x, this.editor.options.canvasWidth - element.config.width));
        element.config.y = Math.max(0, Math.min(y, this.editor.options.canvasHeight - element.config.height));
        
        element.domElement.style.left = element.config.x + 'px';
        element.domElement.style.top = element.config.y + 'px';
    }

    // Other methods remain similar...
    generateElementId() {
        return 'element_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    makeResizable(element) {
        // Basic resize implementation - can be enhanced
        const handles = ['nw', 'ne', 'sw', 'se', 'n', 's', 'e', 'w'];
        
        handles.forEach(handle => {
            const resizeHandle = document.createElement('div');
            resizeHandle.className = `resize-handle resize-${handle}`;
            element.appendChild(resizeHandle);
        });
    }

    initializeClock(element) {
        const updateClock = () => {
            const now = new Date();
            const timeString = now.toLocaleTimeString('fa-IR');
            const timeElement = element.querySelector('.clock-time');
            if (timeElement) {
                timeElement.textContent = timeString;
            }
        };
        
        updateClock();
        setInterval(updateClock, 1000);
    }

    deleteElement(elementId) {
        const element = this.editor.elements.get(elementId);
        if (element) {
            element.domElement.remove();
            this.editor.elements.delete(elementId);
            
            if (this.editor.selectedElement?.id === elementId) {
                this.editor.selectedElement = null;
                this.editor.selectionManager.clearSelection();
            }
            
            this.editor.updateElementsList();
        }
    }
}

/**
 * Page Editor Main Class - Updated with Viewport Management
 */
class PageEditor {
    constructor(pageId, options = {}) {
        this.pageId = pageId;
        this.options = {
            canvasWidth: 1920,
            canvasHeight: 1080,
            gridSize: 20,
            snapToGrid: true,
            autoSave: true,
            autoSaveInterval: 30000,
            ...options
        };

        this.canvas = null;
        this.viewport = null;
        this.viewportManager = null;
        this.elements = new Map();
        this.selectedElement = null;
        this.clipboard = null;
        this.isDragging = false;
        this.draggedElement = null;
        this.dragOffset = { x: 0, y: 0 };
        
        // Managers
        this.elementManager = new ElementManager(this);
        
        this.init();
    }

    async init() {
        try {
            this.setupCanvas();
            this.setupViewport();
            this.setupEventListeners();
            this.setupPageSettings();
            
            console.log('Page Editor initialized successfully');
        } catch (error) {
            console.error('Failed to initialize Page Editor:', error);
        }
    }

    setupCanvas() {
        // Find canvas viewport and container
        this.viewport = document.getElementById('canvas-viewport');
        this.canvas = document.getElementById('canvas-container');
        
        if (!this.viewport || !this.canvas) {
            console.error('Canvas viewport or container not found in DOM');
            return;
        }
        
        // Set canvas dimensions
        this.canvas.style.width = this.options.canvasWidth + 'px';
        this.canvas.style.height = this.options.canvasHeight + 'px';
        
        // Add page content container if it doesn't exist
        let pageContent = document.getElementById('page-content');
        if (!pageContent) {
            pageContent = document.createElement('div');
            pageContent.id = 'page-content';
            pageContent.className = 'page-content';
            this.canvas.appendChild(pageContent);
        }
        
        // Add grid overlay if it doesn't exist
        if (!this.canvas.querySelector('.canvas-grid')) {
            const grid = document.createElement('div');
            grid.className = 'canvas-grid';
            grid.style.cssText = `
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-image: 
                    linear-gradient(rgba(0,0,0,0.1) 1px, transparent 1px),
                    linear-gradient(90deg, rgba(0,0,0,0.1) 1px, transparent 1px);
                background-size: ${this.options.gridSize}px ${this.options.gridSize}px;
                pointer-events: none;
                z-index: 1;
            `;
            this.canvas.appendChild(grid);
        }
        
        // Update size inputs if they exist
        const widthInput = document.getElementById('page-width');
        const heightInput = document.getElementById('page-height');
        if (widthInput) widthInput.value = this.options.canvasWidth;
        if (heightInput) heightInput.value = this.options.canvasHeight;
    }

    setupViewport() {
        if (!this.viewport || !this.canvas) return;
        
        // Initialize viewport manager
        this.viewportManager = new ViewportManager(this.viewport, this.canvas);
        this.viewportManager.setCanvasDimensions(this.options.canvasWidth, this.options.canvasHeight);
        
        // Set zoom change callback to update UI
        this.viewportManager.setZoomChangeCallback((zoomPercentage) => {
            const zoomLevelEl = document.getElementById('zoom-level');
            if (zoomLevelEl) {
                zoomLevelEl.textContent = zoomPercentage + '%';
            }
        });
    }

    setupEventListeners() {
        // Canvas events
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            pageContent.addEventListener('click', (e) => this.handleCanvasClick(e));
            pageContent.addEventListener('contextmenu', (e) => this.handleContextMenu(e));
        }
        
        // Header controls
        const saveBtn = document.getElementById('save-btn');
        const cancelBtn = document.getElementById('cancel-btn');
        const applySizeBtn = document.getElementById('apply-size-btn');
        
        if (saveBtn) saveBtn.addEventListener('click', () => this.save());
        if (cancelBtn) cancelBtn.addEventListener('click', () => this.cancel());
        if (applySizeBtn) applySizeBtn.addEventListener('click', () => this.applyCanvasSize());
        
        // Zoom controls
        const zoomInBtn = document.getElementById('zoom-in');
        const zoomOutBtn = document.getElementById('zoom-out');
        const zoomFitBtn = document.getElementById('zoom-fit');
        const actualSizeBtn = document.getElementById('actual-size');
        
        if (zoomInBtn) zoomInBtn.addEventListener('click', () => this.viewportManager.zoomIn());
        if (zoomOutBtn) zoomOutBtn.addEventListener('click', () => this.viewportManager.zoomOut());
        if (zoomFitBtn) zoomFitBtn.addEventListener('click', () => this.viewportManager.fitToViewport());
        if (actualSizeBtn) actualSizeBtn.addEventListener('click', () => this.viewportManager.actualSize());
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyDown(e));
        
        // Sidebar tabs
        document.querySelectorAll('.sidebar-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                const tabName = tab.dataset.tab;
                this.switchSidebarTab(tabName);
            });
        });
    }

    handleCanvasClick(e) {
        // Clear selection if clicking on empty canvas
        if (e.target === e.currentTarget) {
            this.clearSelection();
        }
    }

    handleContextMenu(e) {
        e.preventDefault();
        // Show context menu (implementation can be added later)
    }

    handleKeyDown(e) {
        // Handle keyboard shortcuts
        if (e.ctrlKey) {
            switch (e.key) {
                case 's':
                    e.preventDefault();
                    this.save();
                    break;
                case 'c':
                    e.preventDefault();
                    this.copy();
                    break;
                case 'v':
                    e.preventDefault();
                    this.paste();
                    break;
                case 'z':
                    e.preventDefault();
                    if (e.shiftKey) {
                        this.redo();
                    } else {
                        this.undo();
                    }
                    break;
            }
        }
        
        if (e.key === 'Delete' && this.selectedElement) {
            this.deleteSelectedElement();
        }
    }

    setupPageSettings() {
        // Page settings implementation
        const pageSettingsBtn = document.getElementById('page-settings-btn');
        if (pageSettingsBtn) {
            pageSettingsBtn.addEventListener('click', () => this.showPageSettings());
        }
    }

    // Apply canvas size changes from header inputs
    applyCanvasSize() {
        const widthInput = document.getElementById('page-width');
        const heightInput = document.getElementById('page-height');
        
        if (widthInput && heightInput) {
            const newWidth = parseInt(widthInput.value);
            const newHeight = parseInt(heightInput.value);
            
            if (newWidth > 0 && newHeight > 0) {
                this.options.canvasWidth = newWidth;
                this.options.canvasHeight = newHeight;
                
                // Update canvas size
                this.canvas.style.width = newWidth + 'px';
                this.canvas.style.height = newHeight + 'px';
                
                // Update viewport manager
                if (this.viewportManager) {
                    this.viewportManager.setCanvasDimensions(newWidth, newHeight);
                }
                
                console.log(`Canvas size updated to ${newWidth}x${newHeight}`);
            }
        }
    }

    // Convert viewport coordinates to canvas coordinates
    viewportToCanvas(viewportX, viewportY) {
        if (this.viewportManager) {
            return this.viewportManager.viewportToCanvas(viewportX, viewportY);
        }
        return { x: viewportX, y: viewportY };
    }

    // Convert canvas coordinates to viewport coordinates
    canvasToViewport(canvasX, canvasY) {
        if (this.viewportManager) {
            return this.viewportManager.canvasToViewport(canvasX, canvasY);
        }
        return { x: canvasX, y: canvasY };
    }

    switchSidebarTab(tabName) {
        // Switch sidebar tabs
        document.querySelectorAll('.sidebar-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        document.querySelectorAll('.sidebar-panel').forEach(panel => {
            panel.classList.remove('active');
        });
        
        document.querySelector(`[data-tab="${tabName}"]`).classList.add('active');
        document.getElementById(`${tabName}-panel`).classList.add('active');
    }

    clearSelection() {
        this.selectedElement = null;
        document.querySelectorAll('.page-element.selected').forEach(el => {
            el.classList.remove('selected');
        });
    }

    showPageSettings() {
        // Show page settings modal
        const modal = document.getElementById('pageSettingsModal');
        if (modal) {
            const bootstrapModal = new bootstrap.Modal(modal);
            bootstrapModal.show();
        }
    }

    // Placeholder methods for core functionality
    save() {
        console.log('Save functionality - to be implemented');
    }

    cancel() {
        console.log('Cancel functionality - to be implemented');
    }

    copy() {
        console.log('Copy functionality - to be implemented');
    }

    paste() {
        console.log('Paste functionality - to be implemented');
    }

    undo() {
        console.log('Undo functionality - to be implemented');
    }

    redo() {
        console.log('Redo functionality - to be implemented');
    }

    deleteSelectedElement() {
        if (this.selectedElement) {
            this.elementManager.deleteElement(this.selectedElement.id);
        }
    }

    // Create element from sidebar
    createElementFromSidebar(type) {
        const defaultConfigs = this.getDefaultElementConfig(type);
        const element = this.elementManager.createElement(type, defaultConfigs);
        return element;
    }

    getDefaultElementConfig(type) {
        const configs = {
            text: {
                width: 200,
                height: 50,
                x: 100,
                y: 100,
                content: 'متن نمونه',
                fontSize: 16,
                color: '#333333',
                backgroundColor: 'transparent'
            },
            image: {
                width: 200,
                height: 150,
                x: 100,
                y: 100,
                src: 'https://via.placeholder.com/200x150',
                alt: 'تصویر نمونه'
            },
            camera: {
                width: 320,
                height: 240,
                x: 100,
                y: 100,
                title: 'دوربین'
            },
            clock: {
                width: 150,
                height: 60,
                x: 100,
                y: 100,
                format: '24',
                showSeconds: true
            },
            weather: {
                width: 200,
                height: 100,
                x: 100,
                y: 100,
                location: 'Tehran'
            }
        };
        
        return configs[type] || {};
    }
}

// Selection Manager placeholder
class SelectionManager {
    constructor(editor) {
        this.editor = editor;
    }

    selectElement(element) {
        this.editor.clearSelection();
        this.editor.selectedElement = element;
        element.domElement.classList.add('selected');
    }

    clearSelection() {
        this.editor.clearSelection();
    }
}

// Global instance
let pageEditor = null;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Get page ID from URL or data attribute
    const pageId = document.body.dataset.pageId || getPageIdFromUrl();
    
    if (pageId) {
        pageEditor = new PageEditor(pageId);
    } else {
        console.error('Page ID not found');
    }
    
    // Setup drag and drop for sidebar elements
    setupSidebarDragDrop();
});

function getPageIdFromUrl() {
    const pathParts = window.location.pathname.split('/');
    return pathParts[pathParts.length - 1];
}

function setupSidebarDragDrop() {
    // Setup drag and drop for sidebar elements
    document.querySelectorAll('.element-item').forEach(item => {
        item.addEventListener('dragstart', (e) => {
            const elementType = item.dataset.elementType;
            e.dataTransfer.setData('application/json', JSON.stringify({
                type: elementType
            }));
        });
    });
    
    // Setup drop zone
    const viewport = document.getElementById('canvas-viewport');
    if (viewport) {
        viewport.addEventListener('dragover', (e) => {
            e.preventDefault();
            viewport.classList.add('drag-over');
        });
        
        viewport.addEventListener('dragleave', () => {
            viewport.classList.remove('drag-over');
        });
        
        viewport.addEventListener('drop', (e) => {
            e.preventDefault();
            viewport.classList.remove('drag-over');
            
            try {
                const data = JSON.parse(e.dataTransfer.getData('application/json'));
                if (data.type && pageEditor) {
                    // Convert drop coordinates to canvas coordinates
                    const rect = viewport.getBoundingClientRect();
                    const viewportX = e.clientX - rect.left;
                    const viewportY = e.clientY - rect.top;
                    const canvasCoords = pageEditor.viewportToCanvas(viewportX, viewportY);
                    
                    // Create element at drop position
                    const config = pageEditor.getDefaultElementConfig(data.type);
                    config.x = canvasCoords.x - (config.width / 2);
                    config.y = canvasCoords.y - (config.height / 2);
                    
                    pageEditor.elementManager.createElement(data.type, config);
                }
            } catch (error) {
                console.error('Error handling drop:', error);
            }
        });
    }
}
