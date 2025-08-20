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
            if (this.editor && this.editor.selectionManager) {
                this.editor.selectionManager.selectElement(element);
            }
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
        
        // Start coordinate tracking
        if (this.editor.viewportManager) {
            this.editor.viewportManager.startCoordinateTracking();
        }
        
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
        
        // Stop coordinate tracking
        if (this.editor.viewportManager) {
            this.editor.viewportManager.stopCoordinateTracking();
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
        this.cacheManager = new CacheManager(this);
        
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
        
        // Toolbar controls
        const selectTool = document.getElementById('select-tool');
        const panTool = document.getElementById('pan-tool');
        
        if (selectTool) selectTool.addEventListener('click', () => this.setTool('select'));
        if (panTool) panTool.addEventListener('click', () => this.setTool('pan'));
        
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
    
    setTool(toolName) {
        // Remove active class from all toolbar buttons
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        
        // Add active class to selected tool
        const selectedTool = document.getElementById(`${toolName}-tool`);
        if (selectedTool) {
            selectedTool.classList.add('active');
        }
        
        // Set viewport mode
        if (this.viewport) {
            this.viewport.classList.remove('pan-mode', 'select-mode');
            this.viewport.classList.add(`${toolName}-mode`);
        }
        
        // Update cursor
        this.updateCursor(toolName);
        
        console.log(`Tool changed to: ${toolName}`);
    }
    
    updateCursor(toolName) {
        if (!this.viewport) return;
        
        switch(toolName) {
            case 'select':
                this.viewport.style.cursor = 'default';
                break;
            case 'pan':
                this.viewport.style.cursor = 'grab';
                break;
            default:
                this.viewport.style.cursor = 'default';
        }
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
    async save() {
        if (!this.cacheManager.hasChanges()) {
            console.log('No changes to save');
            return;
        }
        
        try {
            // Show loading state
            const saveBtn = document.getElementById('save-btn');
            if (saveBtn) {
                const originalContent = saveBtn.innerHTML;
                saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...';
                saveBtn.disabled = true;
            }
            
            // Save to database via cache manager
            const success = await this.cacheManager.saveToDatabase();
            
            if (success) {
                // Clear cache after successful save
                this.cacheManager.clearCache();
                
                // Show success message
                this.showToast('تغییرات با موفقیت ذخیره شد', 'success');
                console.log('Page saved successfully');
            } else {
                // Show error message
                this.showToast('خطا در ذخیره تغییرات', 'error');
                console.error('Failed to save page');
            }
            
            // Restore button state
            if (saveBtn) {
                saveBtn.innerHTML = '<i class="fas fa-save"></i> <span>ذخیره</span>';
                saveBtn.disabled = false;
            }
            
        } catch (error) {
            console.error('Error during save:', error);
            this.showToast('خطای غیرمنتظره در ذخیره', 'error');
        }
    }

    cancel() {
        if (!this.cacheManager.hasChanges()) {
            console.log('No changes to cancel');
            this.showToast('هیچ تغییری برای لغو وجود ندارد', 'info');
            return;
        }
        
        // Show confirmation dialog
        if (confirm('آیا مطمئن هستید که می‌خواهید تغییرات لغو شوند؟')) {
            // Use cache manager's cancel method
            const cancelled = this.cacheManager.cancelChanges();
            
            if (cancelled) {
                this.showToast('تغییرات لغو شد', 'info');
                console.log('Changes cancelled, restored to original state');
                
                // Optionally reload the page to reset state
                // window.location.reload();
            }
        }
    }
    
    showToast(message, type = 'info') {
        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas fa-${this.getToastIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;
        
        // Add to page
        document.body.appendChild(toast);
        
        // Show with animation
        setTimeout(() => toast.classList.add('show'), 100);
        
        // Remove after delay
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => document.body.removeChild(toast), 300);
        }, 3000);
    }
    
    getToastIcon(type) {
        const icons = {
            success: 'check-circle',
            error: 'exclamation-circle',
            warning: 'exclamation-triangle',
            info: 'info-circle'
        };
        return icons[type] || 'info-circle';
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
    
    // Helper method to convert viewport coordinates to canvas coordinates
    viewportToCanvas(viewportX, viewportY) {
        if (this.viewportManager) {
            return this.viewportManager.viewportToCanvas(viewportX, viewportY);
        }
        return { x: viewportX, y: viewportY };
    }
    
    // Helper method to convert canvas coordinates to viewport coordinates
    canvasToViewport(canvasX, canvasY) {
        if (this.viewportManager) {
            return this.viewportManager.canvasToViewport(canvasX, canvasY);
        }
        return { x: canvasX, y: canvasY };
    }
}

// Selection Manager
class SelectionManager {
    constructor(editor) {
        this.editor = editor;
        this.selectedElement = null;
    }

    selectElement(element) {
        // Clear previous selection
        this.clearSelection();
        
        // Set new selection
        this.selectedElement = element;
        this.editor.selectedElement = element;
        
        // Add visual feedback
        element.domElement.classList.add('selected');
        
        // Update properties panel
        this.updatePropertiesPanel(element);
        
        console.log('Element selected:', element);
    }

    clearSelection() {
        if (this.selectedElement) {
            this.selectedElement.domElement.classList.remove('selected');
        }
        
        this.selectedElement = null;
        this.editor.selectedElement = null;
        
        // Clear properties panel
        this.clearPropertiesPanel();
    }
    
    updatePropertiesPanel(element) {
        // Show properties content and hide no-selection
        const noSelection = document.getElementById('no-selection');
        const propertiesContent = document.getElementById('properties-content');
        
        if (noSelection) noSelection.style.display = 'none';
        if (propertiesContent) propertiesContent.style.display = 'block';
        
        // Update basic properties
        const positionX = document.getElementById('element-x');
        const positionY = document.getElementById('element-y');
        const elementWidth = document.getElementById('element-width');
        const elementHeight = document.getElementById('element-height');
        
        if (positionX) positionX.value = Math.round(element.config.x);
        if (positionY) positionY.value = Math.round(element.config.y);
        if (elementWidth) elementWidth.value = Math.round(element.config.width);
        if (elementHeight) elementHeight.value = Math.round(element.config.height);
        
        // Show/hide text properties based on element type
        const textProperties = document.getElementById('text-properties');
        if (textProperties) {
            textProperties.style.display = (element.type === 'text') ? 'block' : 'none';
        }
        
        // Update text properties if it's a text element
        if (element.type === 'text') {
            const textColor = document.getElementById('element-text-color');
            const fontSize = document.getElementById('element-font-size');
            const bgColor = document.getElementById('element-bg-color');
            
            if (textColor) textColor.value = element.config.color || '#000000';
            if (fontSize) fontSize.value = element.config.fontSize || 16;
            if (bgColor) bgColor.value = element.config.backgroundColor || '#ffffff';
        }
        
        // Setup property change listeners
        this.setupPropertyChangeListeners(element);
    }
    
    setupPropertyChangeListeners(element) {
        // Position and size listeners
        const positionX = document.getElementById('element-x');
        const positionY = document.getElementById('element-y');
        const elementWidth = document.getElementById('element-width');
        const elementHeight = document.getElementById('element-height');
        
        if (positionX) {
            positionX.oninput = () => {
                element.config.x = parseInt(positionX.value) || 0;
                this.updateElementVisualPosition(element);
            };
        }
        
        if (positionY) {
            positionY.oninput = () => {
                element.config.y = parseInt(positionY.value) || 0;
                this.updateElementVisualPosition(element);
            };
        }
        
        if (elementWidth) {
            elementWidth.oninput = () => {
                element.config.width = parseInt(elementWidth.value) || 100;
                this.updateElementVisualSize(element);
            };
        }
        
        if (elementHeight) {
            elementHeight.oninput = () => {
                element.config.height = parseInt(elementHeight.value) || 100;
                this.updateElementVisualSize(element);
            };
        }
    }
    
    updateElementVisualPosition(element) {
        element.domElement.style.left = element.config.x + 'px';
        element.domElement.style.top = element.config.y + 'px';
    }
    
    updateElementVisualSize(element) {
        element.domElement.style.width = element.config.width + 'px';
        element.domElement.style.height = element.config.height + 'px';
    }
    
    showTypeSpecificProperties(element) {
        // Hide all type-specific panels first
        const panels = document.querySelectorAll('.type-specific-properties');
        panels.forEach(panel => panel.style.display = 'none');
        
        // Show relevant panel
        const typePanel = document.querySelector(`#${element.type}-properties`);
        if (typePanel) {
            typePanel.style.display = 'block';
            
            // Populate type-specific fields
            this.populateTypeSpecificFields(element, typePanel);
        }
    }
    
    populateTypeSpecificFields(element, panel) {
        const config = element.config;
        
        switch (element.type) {
            case 'text':
                const textContent = panel.querySelector('#text-content');
                const fontSize = panel.querySelector('#text-font-size');
                const textColor = panel.querySelector('#text-color');
                const bgColor = panel.querySelector('#text-bg-color');
                
                if (textContent) textContent.value = config.content || '';
                if (fontSize) fontSize.value = config.fontSize || 16;
                if (textColor) textColor.value = config.color || '#000000';
                if (bgColor) bgColor.value = config.backgroundColor || '#ffffff';
                break;
                
            case 'image':
                const imageSrc = panel.querySelector('#image-src');
                const imageAlt = panel.querySelector('#image-alt');
                
                if (imageSrc) imageSrc.value = config.src || '';
                if (imageAlt) imageAlt.value = config.alt || '';
                break;
        }
    }
    
    clearPropertiesPanel() {
        // Hide properties content and show no-selection
        const noSelection = document.getElementById('no-selection');
        const propertiesContent = document.getElementById('properties-content');
        
        if (noSelection) noSelection.style.display = 'block';
        if (propertiesContent) propertiesContent.style.display = 'none';
        
        // Clear all input values
        const inputs = document.querySelectorAll('#properties-panel input, #properties-panel textarea, #properties-panel select');
        inputs.forEach(input => {
            if (input.type === 'checkbox') {
                input.checked = false;
            } else {
                input.value = '';
            }
        });
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
                // Try different data transfer formats
                let data = null;
                const jsonData = e.dataTransfer.getData('application/json');
                const textData = e.dataTransfer.getData('text/plain');
                
                if (jsonData) {
                    data = JSON.parse(jsonData);
                } else if (textData) {
                    try {
                        data = JSON.parse(textData);
                    } catch {
                        // If not JSON, treat as plain text
                        data = { type: 'text', content: textData };
                    }
                }
                
                if (data && pageEditor) {
                    // Check if it has elementType (from tools list) or type (from other sources)
                    const elementType = data.elementType || data.type;
                    
                    if (elementType) {
                        // Convert drop coordinates to canvas coordinates
                        const rect = viewport.getBoundingClientRect();
                        const viewportX = e.clientX - rect.left;
                        const viewportY = e.clientY - rect.top;
                        const canvasCoords = pageEditor.viewportToCanvas(viewportX, viewportY);
                        
                        // Create element at drop position
                        const config = pageEditor.getDefaultElementConfig(elementType);
                        config.x = canvasCoords.x - (config.width / 2);
                        config.y = canvasCoords.y - (config.height / 2);
                        
                        const element = pageEditor.elementManager.createElement(elementType, config);
                        if (pageEditor.selectionManager) {
                            pageEditor.selectionManager.selectElement(element);
                        }
                        
                        console.log('Element created:', elementType, config);
                    }
                }
            } catch (error) {
                console.error('Error handling drop:', error);
            }
        });
    }
}

/**
 * Cache Manager - Handles auto-save and page state caching
 */
class CacheManager {
    constructor(editor) {
        this.editor = editor;
        this.pageId = this.editor.pageId;
        this.cacheKey = `page_elements_${this.pageId}`;
        this.originalCacheKey = `page_original_${this.pageId}`;
        this.hasUnsavedChanges = false;
        
        this.init();
    }
    
    init() {
        // Store original state on page load
        this.storeOriginalState();
        
        // Set up auto-save on changes
        this.setupChangeDetection();
        
        // Set up page unload warning
        this.setupUnloadWarning();
    }
    
    storeOriginalState() {
        // Store original page state to compare later
        const originalState = this.serializeElements();
        localStorage.setItem(this.originalCacheKey, JSON.stringify({
            elements: originalState,
            timestamp: Date.now()
        }));
    }
    
    setupChangeDetection() {
        // Listen for element changes
        const observer = new MutationObserver(() => {
            this.markAsChanged();
            this.saveToCache();
        });
        
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            observer.observe(pageContent, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeOldValue: true
            });
        }
    }
    
    setupUnloadWarning() {
        window.addEventListener('beforeunload', (e) => {
            if (this.hasUnsavedChanges) {
                const message = 'شما تغییرات ذخیره نشده‌ای دارید. آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟';
                e.preventDefault();
                e.returnValue = message;
                return message;
            }
        });
    }
    
    markAsChanged() {
        this.hasUnsavedChanges = true;
        this.updateSaveButtonState();
    }
    
    markAsSaved() {
        this.hasUnsavedChanges = false;
        this.updateSaveButtonState();
    }
    
    updateSaveButtonState() {
        const saveBtn = document.getElementById('save-btn');
        if (saveBtn) {
            if (this.hasUnsavedChanges) {
                saveBtn.classList.add('has-changes');
                saveBtn.innerHTML = '<i class="fas fa-save"></i> ذخیره تغییرات';
            } else {
                saveBtn.classList.remove('has-changes');
                saveBtn.innerHTML = '<i class="fas fa-check"></i> ذخیره شده';
            }
        }
    }
    
    saveToCache() {
        try {
            const elementsData = this.serializeElements();
            const cacheData = {
                elements: elementsData,
                timestamp: Date.now(),
                version: '1.0',
                pageId: this.pageId
            };
            
            localStorage.setItem(this.cacheKey, JSON.stringify(cacheData));
            console.log('Elements saved to cache:', elementsData.length, 'elements');
        } catch (error) {
            console.error('Failed to save to cache:', error);
        }
    }
    
    loadFromCache() {
        try {
            const cached = localStorage.getItem(this.cacheKey);
            if (cached) {
                const cacheData = JSON.parse(cached);
                
                // Check if cache is valid and for same page
                if (cacheData.pageId === this.pageId && cacheData.elements) {
                    this.deserializeElements(cacheData.elements);
                    this.markAsChanged(); // Mark as having unsaved changes
                    console.log('Loaded elements from cache:', cacheData.elements.length, 'elements');
                    return true;
                }
            }
        } catch (error) {
            console.error('Failed to load from cache:', error);
        }
        return false;
    }
    
    serializeElements() {
        const elements = [];
        this.editor.elements.forEach((element, id) => {
            // Get complete element data including styles and content
            const domElement = element.domElement;
            const computedStyle = window.getComputedStyle(domElement);
            
            const elementData = {
                id: id,
                type: element.type,
                config: { ...element.config },
                position: {
                    x: element.config.x,
                    y: element.config.y,
                    width: element.config.width,
                    height: element.config.height
                },
                styles: {
                    backgroundColor: computedStyle.backgroundColor,
                    color: computedStyle.color,
                    fontSize: computedStyle.fontSize,
                    fontFamily: computedStyle.fontFamily,
                    fontWeight: computedStyle.fontWeight,
                    textAlign: computedStyle.textAlign,
                    border: computedStyle.border,
                    borderRadius: computedStyle.borderRadius,
                    boxShadow: computedStyle.boxShadow,
                    opacity: computedStyle.opacity,
                    zIndex: computedStyle.zIndex
                },
                content: {
                    innerHTML: domElement.innerHTML,
                    textContent: domElement.textContent,
                    attributes: this.getElementAttributes(domElement)
                },
                timestamp: Date.now()
            };
            
            elements.push(elementData);
        });
        
        return elements;
    }
    
    getElementAttributes(domElement) {
        const attrs = {};
        for (let attr of domElement.attributes) {
            attrs[attr.name] = attr.value;
        }
        return attrs;
    }
    
    deserializeElements(elements) {
        // Clear existing elements
        this.editor.elements.clear();
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            pageContent.innerHTML = '';
        }
        
        // Recreate elements with full data
        elements.forEach(elementData => {
            const element = this.editor.elementManager.createElement(
                elementData.type, 
                elementData.config
            );
            
            // Restore position and size
            if (elementData.position) {
                element.config.x = elementData.position.x;
                element.config.y = elementData.position.y;
                element.config.width = elementData.position.width;
                element.config.height = elementData.position.height;
            }
            
            // Restore content
            if (elementData.content) {
                element.domElement.innerHTML = elementData.content.innerHTML;
            }
            
            // Restore styles
            if (elementData.styles) {
                Object.assign(element.domElement.style, elementData.styles);
            }
            
            // Restore attributes
            if (elementData.content.attributes) {
                Object.entries(elementData.content.attributes).forEach(([name, value]) => {
                    if (name !== 'style') { // Don't override style attribute
                        element.domElement.setAttribute(name, value);
                    }
                });
            }
            
            element.id = elementData.id; // Preserve original ID
        });
    }
    
    // Get cached data for saving to database
    getCachedDataForSave() {
        try {
            const cached = localStorage.getItem(this.cacheKey);
            if (cached) {
                const cacheData = JSON.parse(cached);
                return {
                    pageId: this.pageId,
                    elements: cacheData.elements,
                    timestamp: cacheData.timestamp,
                    totalElements: cacheData.elements.length
                };
            }
        } catch (error) {
            console.error('Failed to get cached data:', error);
        }
        return null;
    }
    
    // Save elements to database
    async saveToDatabase() {
        const cachedData = this.getCachedDataForSave();
        if (!cachedData) {
            console.warn('No cached data to save');
            return false;
        }
        
        try {
            const response = await fetch(`/Page/${this.pageId}/elements`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    pageId: this.pageId,
                    elements: cachedData.elements
                })
            });
            
            if (response.ok) {
                this.markAsSaved();
                console.log('Elements saved to database successfully');
                return true;
            } else {
                console.error('Failed to save to database:', response.statusText);
                return false;
            }
        } catch (error) {
            console.error('Error saving to database:', error);
            return false;
        }
    }
    
    clearCache() {
        localStorage.removeItem(this.cacheKey);
        localStorage.removeItem(this.originalCacheKey);
        this.hasUnsavedChanges = false;
        console.log('Cache cleared');
    }
    
    // Cancel changes and restore original state
    cancelChanges() {
        const confirmCancel = confirm('آیا مطمئن هستید که می‌خواهید تمام تغییرات را لغو کنید؟ این عمل قابل بازگشت نیست.');
        
        if (confirmCancel) {
            try {
                const original = localStorage.getItem(this.originalCacheKey);
                if (original) {
                    const originalData = JSON.parse(original);
                    this.deserializeElements(originalData.elements);
                }
                this.clearCache();
                this.markAsSaved();
                return true;
            } catch (error) {
                console.error('Failed to restore original state:', error);
                return false;
            }
        }
        return false;
    }
    
    hasCache() {
        return localStorage.getItem(this.cacheKey) !== null;
    }
    
    hasChanges() {
        return this.hasUnsavedChanges;
    }
}
