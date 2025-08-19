/**
 * Page Editor Main JavaScript Classes
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
        this.editor.canvas.appendChild(element.domElement);
        
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
        
        // Drag functionality
        domElement.addEventListener('mousedown', (e) => {
            if (e.button === 0) { // Left mouse button
                this.startDrag(element, e);
            }
        });
        
        // Make element resizable (basic implementation)
        this.makeResizable(domElement);
    }

    makeResizable(element) {
        // Add resize handles
        const handles = ['nw', 'ne', 'sw', 'se', 'n', 's', 'e', 'w'];
        
        handles.forEach(handle => {
            const resizeHandle = document.createElement('div');
            resizeHandle.className = `resize-handle resize-${handle}`;
            resizeHandle.addEventListener('mousedown', (e) => {
                e.stopPropagation();
                this.startResize(element, handle, e);
            });
            element.appendChild(resizeHandle);
        });
    }

    startDrag(element, e) {
        if (this.editor.isDragging || this.editor.isResizing) return;
        
        this.editor.isDragging = true;
        this.editor.selectedElement = element;
        
        const rect = element.domElement.getBoundingClientRect();
        const canvasRect = this.editor.canvas.getBoundingClientRect();
        
        this.editor.dragOffset = {
            x: e.clientX - rect.left,
            y: e.clientY - rect.top
        };
        
        const mouseMoveHandler = (e) => this.handleDrag(element, e);
        const mouseUpHandler = () => this.endDrag(mouseMoveHandler, mouseUpHandler);
        
        document.addEventListener('mousemove', mouseMoveHandler);
        document.addEventListener('mouseup', mouseUpHandler);
    }

    handleDrag(element, e) {
        const canvasRect = this.editor.canvas.getBoundingClientRect();
        
        let newX = e.clientX - canvasRect.left - this.editor.dragOffset.x;
        let newY = e.clientY - canvasRect.top - this.editor.dragOffset.y;
        
        // Snap to grid if enabled
        if (this.editor.options.snapToGrid) {
            newX = Math.round(newX / this.editor.options.gridSize) * this.editor.options.gridSize;
            newY = Math.round(newY / this.editor.options.gridSize) * this.editor.options.gridSize;
        }
        
        // Constrain to canvas bounds
        newX = Math.max(0, Math.min(newX, this.editor.options.canvasWidth - element.config.width));
        newY = Math.max(0, Math.min(newY, this.editor.options.canvasHeight - element.config.height));
        
        element.domElement.style.left = newX + 'px';
        element.domElement.style.top = newY + 'px';
        
        element.config.x = newX;
        element.config.y = newY;
    }

    endDrag(mouseMoveHandler, mouseUpHandler) {
        this.editor.isDragging = false;
        document.removeEventListener('mousemove', mouseMoveHandler);
        document.removeEventListener('mouseup', mouseUpHandler);
        
        // Add to cache for auto-save
        if (this.editor.selectedElement) {
            this.editor.cacheManager.addChange('element-move', {
                elementId: this.editor.selectedElement.id,
                x: this.editor.selectedElement.config.x,
                y: this.editor.selectedElement.config.y
            });
        }
    }

    startResize(element, handle, e) {
        if (this.editor.isDragging || this.editor.isResizing) return;
        
        this.editor.isResizing = true;
        e.stopPropagation();
        
        const startX = e.clientX;
        const startY = e.clientY;
        const startWidth = element.offsetWidth;
        const startHeight = element.offsetHeight;
        const startLeft = element.offsetLeft;
        const startTop = element.offsetTop;
        
        const mouseMoveHandler = (e) => {
            const deltaX = e.clientX - startX;
            const deltaY = e.clientY - startY;
            
            let newWidth = startWidth;
            let newHeight = startHeight;
            let newLeft = startLeft;
            let newTop = startTop;
            
            // Handle different resize directions
            if (handle.includes('e')) newWidth = startWidth + deltaX;
            if (handle.includes('w')) {
                newWidth = startWidth - deltaX;
                newLeft = startLeft + deltaX;
            }
            if (handle.includes('s')) newHeight = startHeight + deltaY;
            if (handle.includes('n')) {
                newHeight = startHeight - deltaY;
                newTop = startTop + deltaY;
            }
            
            // Apply minimum size constraints
            newWidth = Math.max(50, newWidth);
            newHeight = Math.max(30, newHeight);
            
            // Update element
            element.style.width = newWidth + 'px';
            element.style.height = newHeight + 'px';
            element.style.left = newLeft + 'px';
            element.style.top = newTop + 'px';
        };
        
        const mouseUpHandler = () => {
            this.editor.isResizing = false;
            document.removeEventListener('mousemove', mouseMoveHandler);
            document.removeEventListener('mouseup', mouseUpHandler);
            
            // Update element config
            const elementData = this.editor.elements.get(element.dataset.elementId);
            if (elementData) {
                elementData.config.width = element.offsetWidth;
                elementData.config.height = element.offsetHeight;
                elementData.config.x = element.offsetLeft;
                elementData.config.y = element.offsetTop;
                
                // Add to cache
                this.editor.cacheManager.addChange('element-resize', {
                    elementId: elementData.id,
                    width: elementData.config.width,
                    height: elementData.config.height,
                    x: elementData.config.x,
                    y: elementData.config.y
                });
            }
        };
        
        document.addEventListener('mousemove', mouseMoveHandler);
        document.addEventListener('mouseup', mouseUpHandler);
    }

    deleteElement(elementId) {
        const element = this.editor.elements.get(elementId);
        if (element) {
            element.domElement.remove();
            this.editor.elements.delete(elementId);
            
            if (this.editor.selectedElement && this.editor.selectedElement.id === elementId) {
                this.editor.selectionManager.clearSelection();
            }
            
            this.editor.cacheManager.addChange('element-delete', { elementId });
            this.editor.updateElementsList();
        }
    }

    updateElement(elementId, newConfig) {
        const element = this.editor.elements.get(elementId);
        if (element) {
            element.config = { ...element.config, ...newConfig };
            this.updateElementDOM(element);
            
            this.editor.cacheManager.addChange('element-update', {
                elementId: elementId,
                config: element.config
            });
        }
    }

    updateElementDOM(element) {
        const domElement = element.domElement;
        const config = element.config;
        
        // Update position and size
        domElement.style.left = config.x + 'px';
        domElement.style.top = config.y + 'px';
        domElement.style.width = config.width + 'px';
        domElement.style.height = config.height + 'px';
        
        // Update content
        this.setElementContent(domElement, element.type, config);
    }

    initializeClock(element) {
        const updateClock = () => {
            const clockElement = element.querySelector('.clock-time');
            const format = element.dataset.format;
            const showSeconds = element.dataset.showSeconds === 'true';
            
            const now = new Date();
            let timeString;
            
            if (format === '12h') {
                timeString = now.toLocaleTimeString('en-US', {
                    hour12: true,
                    hour: '2-digit',
                    minute: '2-digit',
                    second: showSeconds ? '2-digit' : undefined
                });
            } else {
                timeString = now.toLocaleTimeString('en-GB', {
                    hour12: false,
                    hour: '2-digit',
                    minute: '2-digit',
                    second: showSeconds ? '2-digit' : undefined
                });
            }
            
            clockElement.textContent = timeString;
        };
        
        updateClock();
        setInterval(updateClock, 1000);
    }

    generateElementId() {
        return 'element_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
}

/**
 * Selection Manager - Handles element selection and visual feedback
 */
class SelectionManager {
    constructor(editor) {
        this.editor = editor;
        this.selectedElement = null;
        this.selectionBox = null;
    }

    selectElement(element) {
        this.clearSelection();
        
        this.selectedElement = element;
        this.editor.selectedElement = element;
        
        this.showSelectionBox(element);
        this.showElementProperties(element);
        
        // Update elements list
        this.updateElementsListSelection(element.id);
    }

    clearSelection() {
        if (this.selectionBox) {
            this.selectionBox.remove();
            this.selectionBox = null;
        }
        
        this.selectedElement = null;
        this.editor.selectedElement = null;
        
        // Clear elements list selection
        document.querySelectorAll('.element-item').forEach(item => {
            item.classList.remove('selected');
        });
        
        // Clear properties panel
        this.clearPropertiesPanel();
    }

    showSelectionBox(element) {
        this.selectionBox = document.createElement('div');
        this.selectionBox.className = 'selection-outline';
        this.selectionBox.style.position = 'absolute';
        this.selectionBox.style.left = element.config.x + 'px';
        this.selectionBox.style.top = element.config.y + 'px';
        this.selectionBox.style.width = element.config.width + 'px';
        this.selectionBox.style.height = element.config.height + 'px';
        this.selectionBox.style.border = '2px solid #007bff';
        this.selectionBox.style.pointerEvents = 'none';
        this.selectionBox.style.zIndex = '1000';
        
        this.editor.canvas.appendChild(this.selectionBox);
    }

    updateSelectionBox() {
        if (this.selectionBox && this.selectedElement) {
            this.selectionBox.style.left = this.selectedElement.config.x + 'px';
            this.selectionBox.style.top = this.selectedElement.config.y + 'px';
            this.selectionBox.style.width = this.selectedElement.config.width + 'px';
            this.selectionBox.style.height = this.selectedElement.config.height + 'px';
        }
    }

    showElementProperties(element) {
        const propertiesPanel = document.getElementById('properties-content');
        if (propertiesPanel) {
            propertiesPanel.innerHTML = this.generatePropertiesHTML(element);
            this.setupPropertiesEvents(element);
        }
    }

    generatePropertiesHTML(element) {
        const config = element.config;
        
        let html = `
            <div class="properties-section">
                <h6>موقعیت و اندازه</h6>
                <div class="row">
                    <div class="col-6">
                        <label class="form-label small">X</label>
                        <input type="number" class="form-control form-control-sm" id="prop-x" value="${config.x}">
                    </div>
                    <div class="col-6">
                        <label class="form-label small">Y</label>
                        <input type="number" class="form-control form-control-sm" id="prop-y" value="${config.y}">
                    </div>
                </div>
                <div class="row mt-2">
                    <div class="col-6">
                        <label class="form-label small">عرض</label>
                        <input type="number" class="form-control form-control-sm" id="prop-width" value="${config.width}">
                    </div>
                    <div class="col-6">
                        <label class="form-label small">ارتفاع</label>
                        <input type="number" class="form-control form-control-sm" id="prop-height" value="${config.height}">
                    </div>
                </div>
            </div>
        `;
        
        // Add specific properties based on element type
        switch (element.type) {
            case 'text':
                html += `
                    <div class="properties-section">
                        <h6>متن</h6>
                        <div class="mb-2">
                            <label class="form-label small">محتوا</label>
                            <textarea class="form-control form-control-sm" id="prop-content" rows="3">${config.content}</textarea>
                        </div>
                        <div class="row">
                            <div class="col-6">
                                <label class="form-label small">اندازه فونت</label>
                                <input type="number" class="form-control form-control-sm" id="prop-font-size" value="${config.fontSize}">
                            </div>
                            <div class="col-6">
                                <label class="form-label small">رنگ</label>
                                <input type="color" class="form-control form-control-color" id="prop-color" value="${config.color}">
                            </div>
                        </div>
                    </div>
                `;
                break;
            case 'image':
                html += `
                    <div class="properties-section">
                        <h6>تصویر</h6>
                        <div class="mb-2">
                            <label class="form-label small">آدرس تصویر</label>
                            <input type="text" class="form-control form-control-sm" id="prop-src" value="${config.src}">
                        </div>
                        <div class="mb-2">
                            <label class="form-label small">متن جایگزین</label>
                            <input type="text" class="form-control form-control-sm" id="prop-alt" value="${config.alt}">
                        </div>
                    </div>
                `;
                break;
            case 'camera':
                html += `
                    <div class="properties-section">
                        <h6>دوربین</h6>
                        <div class="mb-2">
                            <label class="form-label small">شناسه دوربین</label>
                            <input type="text" class="form-control form-control-sm" id="prop-camera-id" value="${config.cameraId}">
                        </div>
                        <div class="mb-2">
                            <label class="form-label small">عنوان</label>
                            <input type="text" class="form-control form-control-sm" id="prop-title" value="${config.title}">
                        </div>
                    </div>
                `;
                break;
        }
        
        html += `
            <div class="properties-section">
                <button type="button" class="btn btn-primary btn-sm w-100" id="apply-properties">
                    <i class="fas fa-check me-1"></i>
                    اعمال تغییرات
                </button>
            </div>
        `;
        
        return html;
    }

    setupPropertiesEvents(element) {
        // Apply properties button
        document.getElementById('apply-properties').addEventListener('click', () => {
            this.applyProperties(element);
        });
        
        // Real-time updates for position and size
        ['prop-x', 'prop-y', 'prop-width', 'prop-height'].forEach(id => {
            const input = document.getElementById(id);
            if (input) {
                input.addEventListener('input', () => {
                    this.updateElementFromProperties(element);
                });
            }
        });
    }

    updateElementFromProperties(element) {
        const x = parseInt(document.getElementById('prop-x').value) || 0;
        const y = parseInt(document.getElementById('prop-y').value) || 0;
        const width = parseInt(document.getElementById('prop-width').value) || 50;
        const height = parseInt(document.getElementById('prop-height').value) || 30;
        
        element.config.x = x;
        element.config.y = y;
        element.config.width = width;
        element.config.height = height;
        
        // Update DOM element
        element.domElement.style.left = x + 'px';
        element.domElement.style.top = y + 'px';
        element.domElement.style.width = width + 'px';
        element.domElement.style.height = height + 'px';
        
        // Update selection box
        this.updateSelectionBox();
    }

    applyProperties(element) {
        const newConfig = { ...element.config };
        
        // Common properties
        newConfig.x = parseInt(document.getElementById('prop-x').value) || 0;
        newConfig.y = parseInt(document.getElementById('prop-y').value) || 0;
        newConfig.width = parseInt(document.getElementById('prop-width').value) || 50;
        newConfig.height = parseInt(document.getElementById('prop-height').value) || 30;
        
        // Type-specific properties
        switch (element.type) {
            case 'text':
                const content = document.getElementById('prop-content');
                const fontSize = document.getElementById('prop-font-size');
                const color = document.getElementById('prop-color');
                
                if (content) newConfig.content = content.value;
                if (fontSize) newConfig.fontSize = parseInt(fontSize.value) || 16;
                if (color) newConfig.color = color.value;
                break;
                
            case 'image':
                const src = document.getElementById('prop-src');
                const alt = document.getElementById('prop-alt');
                
                if (src) newConfig.src = src.value;
                if (alt) newConfig.alt = alt.value;
                break;
                
            case 'camera':
                const cameraId = document.getElementById('prop-camera-id');
                const title = document.getElementById('prop-title');
                
                if (cameraId) newConfig.cameraId = cameraId.value;
                if (title) newConfig.title = title.value;
                break;
        }
        
        // Update element
        this.editor.elementManager.updateElement(element.id, newConfig);
        this.updateSelectionBox();
        
        // Show success message
        this.editor.showSaveIndicator('saving');
    }

    updateElementsListSelection(elementId) {
        document.querySelectorAll('.element-item').forEach(item => {
            item.classList.remove('selected');
            if (item.dataset.elementId === elementId) {
                item.classList.add('selected');
            }
        });
    }

}

/**
 * Drag and Drop Manager - Handles drag and drop from sidebar
 */
class DragDropManager {
    constructor(editor) {
        this.editor = editor;
        this.draggedElementType = null;
        this.setupSidebarDragDrop();
    }

    setupSidebarDragDrop() {
        // Setup drag for sidebar elements
        const setupElementDrag = (element) => {
            element.addEventListener('dragstart', (e) => {
                this.draggedElementType = element.dataset.elementType;
                e.dataTransfer.effectAllowed = 'copy';
                e.dataTransfer.setData('text/plain', this.draggedElementType);
                
                // Add visual feedback
                element.classList.add('dragging');
            });
            
            element.addEventListener('dragend', (e) => {
                element.classList.remove('dragging');
                this.draggedElementType = null;
            });
        };

        // Setup for existing elements
        document.querySelectorAll('.element-item[draggable="true"]').forEach(setupElementDrag);
        
        // Setup for dynamically added elements (using delegation)
        document.addEventListener('dragstart', (e) => {
            if (e.target.matches('.element-item[draggable="true"]')) {
                setupElementDrag(e.target);
            }
        });
    }

    setupCanvasDropZone() {
        const canvas = this.editor.canvas;
        
        canvas.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
            
            // Show drop indicator
            canvas.classList.add('drag-over');
        });
        
        canvas.addEventListener('dragleave', (e) => {
            if (!canvas.contains(e.relatedTarget)) {
                canvas.classList.remove('drag-over');
            }
        });
        
        canvas.addEventListener('drop', (e) => {
            e.preventDefault();
            canvas.classList.remove('drag-over');
            
            const elementType = e.dataTransfer.getData('text/plain') || this.draggedElementType;
            
            if (elementType) {
                const canvasRect = canvas.getBoundingClientRect();
                const x = e.clientX - canvasRect.left;
                const y = e.clientY - canvasRect.top;
                
                const config = this.editor.getDefaultElementConfig(elementType);
                config.x = Math.max(0, x - config.width / 2);
                config.y = Math.max(0, y - config.height / 2);
                
                // Snap to grid if enabled
                if (this.editor.options.snapToGrid) {
                    config.x = Math.round(config.x / this.editor.options.gridSize) * this.editor.options.gridSize;
                    config.y = Math.round(config.y / this.editor.options.gridSize) * this.editor.options.gridSize;
                }
                
                const element = this.editor.elementManager.createElement(elementType, config);
                this.editor.history.addCommand(new AddElementCommand(element));
                this.editor.updateElementsList();
                
                // Select the new element
                this.editor.selectionManager.selectElement(element);
            }
        });
    }
}

/**
 * Toolbar Manager - Handles toolbar interactions
 */
class ToolbarManager {
    constructor(editor) {
        this.editor = editor;
        this.currentTool = 'select';
        this.setupToolbar();
    }

    setupToolbar() {
        // Tool selection
        document.getElementById('select-tool').addEventListener('click', () => {
            this.setTool('select');
        });
        
        document.getElementById('pan-tool').addEventListener('click', () => {
            this.setTool('pan');
        });
        
        // Actions
        document.getElementById('undo-btn').addEventListener('click', () => {
            this.editor.history.undo();
        });
        
        document.getElementById('redo-btn').addEventListener('click', () => {
            this.editor.history.redo();
        });
        
        document.getElementById('copy-btn').addEventListener('click', () => {
            this.editor.copy();
        });
        
        document.getElementById('paste-btn').addEventListener('click', () => {
            this.editor.paste();
        });
        
        // Grid and snap
        document.getElementById('grid-toggle').addEventListener('click', () => {
            this.toggleGrid();
        });
        
        document.getElementById('snap-toggle').addEventListener('click', () => {
            this.toggleSnap();
        });
        
        // Zoom controls
        document.getElementById('zoom-in').addEventListener('click', () => {
            this.zoomIn();
        });
        
        document.getElementById('zoom-out').addEventListener('click', () => {
            this.zoomOut();
        });
        
        document.getElementById('zoom-fit').addEventListener('click', () => {
            this.zoomToFit();
        });
    }

    setTool(toolName) {
        this.currentTool = toolName;
        
        // Update button states
        document.querySelectorAll('.toolbar-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        
        document.getElementById(`${toolName}-tool`).classList.add('active');
        
        // Update cursor
        this.updateCanvasCursor();
    }

    updateCanvasCursor() {
        const cursor = this.currentTool === 'pan' ? 'grab' : 'default';
        this.editor.canvas.style.cursor = cursor;
    }

    toggleGrid() {
        const grid = this.editor.canvas.querySelector('.canvas-grid');
        const btn = document.getElementById('grid-toggle');
        
        if (grid.style.display === 'none') {
            grid.style.display = 'block';
            btn.classList.add('active');
        } else {
            grid.style.display = 'none';
            btn.classList.remove('active');
        }
    }

    toggleSnap() {
        const btn = document.getElementById('snap-toggle');
        
        this.editor.options.snapToGrid = !this.editor.options.snapToGrid;
        
        if (this.editor.options.snapToGrid) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    }

    zoomIn() {
        this.zoom(1.2);
    }

    zoomOut() {
        this.zoom(0.8);
    }

    zoom(factor) {
        const workspace = document.querySelector('.editor-workspace');
        const currentTransform = workspace.style.transform;
        const currentScale = this.getCurrentScale(currentTransform);
        
        const newScale = Math.min(Math.max(currentScale * factor, 0.1), 5);
        
        workspace.style.transform = `scale(${newScale})`;
        
        // Update zoom level display
        document.getElementById('zoom-level').textContent = Math.round(newScale * 100) + '%';
    }

    zoomToFit() {
        const workspace = document.querySelector('.editor-workspace');
        const container = workspace.parentElement;
        
        const containerRect = container.getBoundingClientRect();
        const canvasRect = this.editor.canvas.getBoundingClientRect();
        
        const scaleX = (containerRect.width - 40) / this.editor.options.canvasWidth;
        const scaleY = (containerRect.height - 40) / this.editor.options.canvasHeight;
        
        const scale = Math.min(scaleX, scaleY, 1);
        
        workspace.style.transform = `scale(${scale})`;
        document.getElementById('zoom-level').textContent = Math.round(scale * 100) + '%';
    }

    getCurrentScale(transform) {
        if (!transform || transform === 'none') return 1;
        
        const match = transform.match(/scale\(([^)]+)\)/);
        return match ? parseFloat(match[1]) : 1;
    }
}

/**
 * Cache Manager - Handles auto-save and draft management
 */
class CacheManager {
    constructor(editor) {
        this.editor = editor;
        this.changes = new Map();
        this.lastSaveTime = Date.now();
        this.saveInProgress = false;
    }

    addChange(type, data) {
        const changeId = `${type}_${Date.now()}_${Math.random()}`;
        this.changes.set(changeId, {
            type: type,
            data: data,
            timestamp: Date.now()
        });
        
        // Auto-save if enabled
        if (this.editor.options.autoSave && !this.saveInProgress) {
            this.debouncedSave();
        }
    }

    hasUnsavedChanges() {
        return this.changes.size > 0;
    }

    async saveDraft() {
        if (this.saveInProgress || this.changes.size === 0) return;
        
        this.saveInProgress = true;
        
        try {
            const changesData = Array.from(this.changes.values());
            
            const response = await fetch(`/api/page-edit/${this.editor.pageId}/draft`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    changes: changesData,
                    timestamp: Date.now()
                })
            });
            
            if (response.ok) {
                console.log('Draft saved successfully');
                this.lastSaveTime = Date.now();
            }
        } catch (error) {
            console.error('Failed to save draft:', error);
        } finally {
            this.saveInProgress = false;
        }
    }

    async publishChanges() {
        if (this.saveInProgress) return false;
        
        this.saveInProgress = true;
        
        try {
            const elementsData = this.editor.getAllElements().map(element => ({
                id: element.id,
                type: element.type,
                config: element.config
            }));
            
            const response = await fetch(`/api/page-edit/${this.editor.pageId}/publish`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    elements: elementsData,
                    canvasSize: this.editor.getCanvasSize(),
                    timestamp: Date.now()
                })
            });
            
            if (response.ok) {
                this.changes.clear();
                this.lastSaveTime = Date.now();
                return true;
            }
            
            return false;
        } catch (error) {
            console.error('Failed to publish changes:', error);
            return false;
        } finally {
            this.saveInProgress = false;
        }
    }

    loadDraftChanges(draftData) {
        if (draftData && draftData.changes) {
            draftData.changes.forEach(change => {
                this.applyChange(change);
            });
        }
    }

    applyChange(change) {
        switch (change.type) {
            case 'element-move':
            case 'element-resize':
            case 'element-update':
                const element = this.editor.elements.get(change.data.elementId);
                if (element) {
                    Object.assign(element.config, change.data);
                    this.editor.elementManager.updateElementDOM(element);
                }
                break;
                
            case 'element-delete':
                this.editor.elementManager.deleteElement(change.data.elementId);
                break;
                
            case 'canvas-resize':
                this.editor.options.canvasWidth = change.data.width;
                this.editor.options.canvasHeight = change.data.height;
                this.editor.canvas.style.width = change.data.width + 'px';
                this.editor.canvas.style.height = change.data.height + 'px';
                break;
        }
    }

    debouncedSave = this.debounce(() => {
        this.saveDraft();
    }, 2000);

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

/**
 * Editor History - Handles undo/redo functionality
 */
class EditorHistory {
    constructor() {
        this.commands = [];
        this.currentIndex = -1;
        this.maxCommands = 50;
    }

    addCommand(command) {
        // Remove any commands after current index
        this.commands = this.commands.slice(0, this.currentIndex + 1);
        
        // Add new command
        this.commands.push(command);
        this.currentIndex++;
        
        // Limit history size
        if (this.commands.length > this.maxCommands) {
            this.commands.shift();
            this.currentIndex--;
        }
        
        // Execute the command
        command.execute();
        
        this.updateToolbarButtons();
    }

    undo() {
        if (this.currentIndex >= 0) {
            const command = this.commands[this.currentIndex];
            command.undo();
            this.currentIndex--;
            this.updateToolbarButtons();
        }
    }

    redo() {
        if (this.currentIndex < this.commands.length - 1) {
            this.currentIndex++;
            const command = this.commands[this.currentIndex];
            command.execute();
            this.updateToolbarButtons();
        }
    }

    canUndo() {
        return this.currentIndex >= 0;
    }

    canRedo() {
        return this.currentIndex < this.commands.length - 1;
    }

    updateToolbarButtons() {
        const undoBtn = document.getElementById('undo-btn');
        const redoBtn = document.getElementById('redo-btn');
        
        if (undoBtn) {
            undoBtn.disabled = !this.canUndo();
        }
        
        if (redoBtn) {
            redoBtn.disabled = !this.canRedo();
        }
    }

    markSaved() {
        // Mark current state as saved
        this.lastSavedIndex = this.currentIndex;
    }

    hasUnsavedChanges() {
        return this.lastSavedIndex !== this.currentIndex;
    }
}

/**
 * Command Classes for History
 */
class Command {
    execute() {
        throw new Error('Execute method must be implemented');
    }
    
    undo() {
        throw new Error('Undo method must be implemented');
    }
}

class AddElementCommand extends Command {
    constructor(element) {
        super();
        this.element = element;
        this.editor = window.pageEditor;
    }
    
    execute() {
        // Element is already created, just ensure it's in the elements map
        if (!this.editor.elements.has(this.element.id)) {
            this.editor.elements.set(this.element.id, this.element);
            this.editor.canvas.appendChild(this.element.domElement);
        }
    }
    
    undo() {
        this.editor.elementManager.deleteElement(this.element.id);
    }
}

class DeleteElementCommand extends Command {
    constructor(element) {
        super();
        this.element = element;
        this.editor = window.pageEditor;
    }
    
    execute() {
        this.editor.elementManager.deleteElement(this.element.id);
    }
    
    undo() {
        this.editor.elements.set(this.element.id, this.element);
        this.editor.canvas.appendChild(this.element.domElement);
        this.editor.elementManager.setupElementInteractions(this.element);
    }
}

class PageEditor {
    constructor(pageId, options = {}) {
        this.pageId = pageId;
        this.options = {
            canvasWidth: 1920,
            canvasHeight: 1080,
            gridSize: 20,
            snapToGrid: true,
            autoSave: true,
            autoSaveInterval: 30000, // 30 seconds
            ...options
        };

        this.canvas = null;
        this.viewport = null;
        this.viewportManager = null;
        this.elements = new Map();
        this.selectedElement = null;
        this.clipboard = null;
        this.history = new EditorHistory();
        this.isDragging = false;
        this.isResizing = false;
        this.dragOffset = { x: 0, y: 0 };
        
        // Managers
        this.elementManager = new ElementManager(this);
        this.dragDropManager = new DragDropManager(this);
        this.selectionManager = new SelectionManager(this);
        this.toolbarManager = new ToolbarManager(this);
        this.cacheManager = new CacheManager(this);
        
        this.init();
    }

    async init() {
        try {
            this.setupCanvas();
            this.setupViewport();
            this.setupEventListeners();
            this.setupPageSettings();
            this.loadPage();
            this.startAutoSave();
            
            // Setup drag and drop for canvas
            this.dragDropManager.setupCanvasDropZone();
            
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
        this.canvas.addEventListener('click', (e) => this.handleCanvasClick(e));
        this.canvas.addEventListener('contextmenu', (e) => this.handleContextMenu(e));
        
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
        
        // Window events
        window.addEventListener('beforeunload', (e) => this.handleBeforeUnload(e));
        
        // Sidebar tabs
        document.querySelectorAll('.sidebar-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                const tabName = tab.dataset.tab;
                this.switchSidebarTab(tabName);
            });
        });
        
        // Page settings button
        const pageSettingsBtn = document.getElementById('page-settings-btn');
        if (pageSettingsBtn) {
            pageSettingsBtn.addEventListener('click', () => this.showPageSettings());
        }
    }
    }

    setupPageSettings() {
        // Page settings modal events
        const pageSettingsBtn = document.getElementById('page-settings-btn');
        const savePageSettingsBtn = document.getElementById('save-page-settings');
        
        if (pageSettingsBtn) {
            pageSettingsBtn.addEventListener('click', () => {
                const modal = new bootstrap.Modal(document.getElementById('pageSettingsModal'));
                modal.show();
            });
        }
        
        if (savePageSettingsBtn) {
            savePageSettingsBtn.addEventListener('click', () => this.savePageSettings());
        }
        
        // Background image preview
        const bgImageInput = document.getElementById('background-image');
        if (bgImageInput) {
            bgImageInput.addEventListener('change', (e) => this.previewBackgroundImage(e));
        }
        
        // Audio volume slider
        const volumeSlider = document.getElementById('audio-volume');
        const volumeValue = document.getElementById('volume-value');
        if (volumeSlider && volumeValue) {
            volumeSlider.addEventListener('input', (e) => {
                volumeValue.textContent = e.target.value + '%';
            });
        }
        
        // Background audio preview
        const bgAudioInput = document.getElementById('background-audio');
        if (bgAudioInput) {
            bgAudioInput.addEventListener('change', (e) => this.previewBackgroundAudio(e));
        }
    }

    handleCanvasClick(e) {
        if (e.target === this.canvas || e.target.classList.contains('canvas-grid')) {
            this.selectionManager.clearSelection();
        }
    }

    handleContextMenu(e) {
        e.preventDefault();
        // Show context menu logic here
        this.showContextMenu(e.clientX, e.clientY);
    }

    showContextMenu(x, y) {
        const contextMenu = document.getElementById('context-menu');
        if (contextMenu) {
            contextMenu.style.display = 'block';
            contextMenu.style.left = x + 'px';
            contextMenu.style.top = y + 'px';
            
            // Hide on next click
            setTimeout(() => {
                document.addEventListener('click', () => {
                    contextMenu.style.display = 'none';
                }, { once: true });
            }, 0);
        }
    }

    handleKeyboard(e) {
        if (e.ctrlKey) {
            switch (e.key) {
                case 's':
                    e.preventDefault();
                    this.save();
                    break;
                case 'z':
                    e.preventDefault();
                    this.history.undo();
                    break;
                case 'y':
                    e.preventDefault();
                    this.history.redo();
                    break;
                case 'c':
                    e.preventDefault();
                    this.copy();
                    break;
                case 'v':
                    e.preventDefault();
                    this.paste();
                    break;
                case 'd':
                    e.preventDefault();
                    this.duplicateElement();
                    break;
            }
        }
        
        if (e.key === 'Delete' && this.selectedElement) {
            this.elementManager.deleteElement(this.selectedElement.id);
        }
        
        // Arrow keys for nudging
        if (this.selectedElement && ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
            e.preventDefault();
            this.nudgeSelectedElement(e.key);
        }
    }

    nudgeSelectedElement(direction) {
        if (!this.selectedElement) return;
        
        const nudgeAmount = e.shiftKey ? 10 : 1;
        const element = this.selectedElement;
        
        switch (direction) {
            case 'ArrowUp':
                element.config.y = Math.max(0, element.config.y - nudgeAmount);
                break;
            case 'ArrowDown':
                element.config.y = Math.min(this.options.canvasHeight - element.config.height, element.config.y + nudgeAmount);
                break;
            case 'ArrowLeft':
                element.config.x = Math.max(0, element.config.x - nudgeAmount);
                break;
            case 'ArrowRight':
                element.config.x = Math.min(this.options.canvasWidth - element.config.width, element.config.x + nudgeAmount);
                break;
        }
        
        this.elementManager.updateElementDOM(element);
        this.selectionManager.updateSelectionBox();
        this.cacheManager.addChange('element-move', {
            elementId: element.id,
            x: element.config.x,
            y: element.config.y
        });
    }

    switchSidebarTab(tabName) {
        // Hide all panels
        document.querySelectorAll('.sidebar-panel').forEach(panel => {
            panel.classList.remove('active');
        });
        
        // Hide all tabs
        document.querySelectorAll('.sidebar-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        
        // Show selected panel and tab
        const panel = document.getElementById(`${tabName}-panel`);
        const tab = document.querySelector(`[data-tab="${tabName}"]`);
        
        if (panel) panel.classList.add('active');
        if (tab) tab.classList.add('active');
    }

    duplicateElement() {
        if (this.selectedElement) {
            const config = {
                ...this.selectedElement.config,
                x: this.selectedElement.config.x + 20,
                y: this.selectedElement.config.y + 20
            };
            
            const element = this.elementManager.createElement(this.selectedElement.type, config);
            this.history.addCommand(new AddElementCommand(element));
            this.updateElementsList();
            this.selectionManager.selectElement(element);
        }
    }

    previewBackgroundImage(event) {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                const preview = document.querySelector('.background-preview');
                if (preview) {
                    preview.style.backgroundImage = `url(${e.target.result})`;
                    preview.innerHTML = '';
                }
            };
            reader.readAsDataURL(file);
        }
    }

    previewBackgroundAudio(event) {
        const file = event.target.files[0];
        if (file) {
            const audioPreview = document.getElementById('audio-preview');
            const audioControls = document.querySelector('.audio-controls');
            
            if (audioPreview && audioControls) {
                const url = URL.createObjectURL(file);
                audioPreview.src = url;
                audioControls.style.display = 'block';
            }
        }
    }

    async savePageSettings() {
        const formData = new FormData();
        
        // Background image
        const bgImage = document.getElementById('background-image').files[0];
        if (bgImage) {
            formData.append('backgroundImage', bgImage);
        }
        
        // Background audio
        const bgAudio = document.getElementById('background-audio').files[0];
        if (bgAudio) {
            formData.append('backgroundAudio', bgAudio);
        }
        
        // Audio settings
        const audioVolume = document.getElementById('audio-volume').value;
        const audioLoop = document.getElementById('audio-loop').checked;
        const audioAutoplay = document.getElementById('audio-autoplay').checked;
        
        formData.append('audioVolume', audioVolume);
        formData.append('audioLoop', audioLoop);
        formData.append('audioAutoplay', audioAutoplay);
        
        try {
            const response = await fetch(`/api/page/${this.pageId}/settings`, {
                method: 'POST',
                body: formData
            });
            
            if (response.ok) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('pageSettingsModal'));
                modal.hide();
                this.showSaveIndicator('saved');
            } else {
                this.showSaveIndicator('error');
            }
        } catch (error) {
            console.error('Failed to save page settings:', error);
            this.showSaveIndicator('error');
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
        const widthInput = document.getElementById('page-width');
        const heightInput = document.getElementById('page-height');
        
        if (widthInput && heightInput) {
            const width = parseInt(widthInput.value);
            const height = parseInt(heightInput.value);
            
            if (width > 0 && height > 0) {
                this.options.canvasWidth = width;
                this.options.canvasHeight = height;
                
                this.canvas.style.width = width + 'px';
                this.canvas.style.height = height + 'px';
                
                this.cacheManager.addChange('canvas-resize', { width, height });
                this.showSaveIndicator('saving');
            }
        }
    }

    async loadPage() {
        try {
            this.showSaveIndicator('loading');
            
            const response = await fetch(`/api/page/${this.pageId}/edit-session`);
            const data = await response.json();
            
            if (data.success) {
                // Load page elements
                if (data.page && data.page.elements) {
                    data.page.elements.forEach(elementData => {
                        this.elementManager.createElement(elementData.type, elementData);
                    });
                }
                
                // Load page settings
                if (data.page) {
                    this.loadPageSettings(data.page);
                }
                
                // Load draft changes
                if (data.hasDraft && data.draftChanges) {
                    this.cacheManager.loadDraftChanges(data.draftChanges);
                }
                
                this.updateElementsList();
                this.showSaveIndicator('saved');
            }
        } catch (error) {
            console.error('Failed to load page:', error);
            this.showSaveIndicator('error');
        }
    }

    loadPageSettings(pageData) {
        // Load canvas size
        if (pageData.width && pageData.height) {
            this.options.canvasWidth = pageData.width;
            this.options.canvasHeight = pageData.height;
            this.setupCanvas();
        }
        
        // Load background image
        if (pageData.backgroundImage) {
            this.canvas.style.backgroundImage = `url(${pageData.backgroundImage})`;
            this.canvas.style.backgroundSize = 'cover';
            this.canvas.style.backgroundPosition = 'center';
        }
        
        // Load other settings
        if (pageData.settings) {
            this.applyPageSettings(pageData.settings);
        }
    }

    applyPageSettings(settings) {
        // Apply audio settings, colors, etc.
        if (settings.backgroundColor) {
            this.canvas.style.backgroundColor = settings.backgroundColor;
        }
        
        if (settings.backgroundAudio) {
            this.setupBackgroundAudio(settings.backgroundAudio, settings.audioSettings);
        }
    }

    setupBackgroundAudio(audioUrl, audioSettings = {}) {
        // Remove existing audio if any
        const existingAudio = document.getElementById('page-background-audio');
        if (existingAudio) {
            existingAudio.remove();
        }
        
        // Create new audio element
        const audio = document.createElement('audio');
        audio.id = 'page-background-audio';
        audio.src = audioUrl;
        audio.loop = audioSettings.loop || false;
        audio.volume = (audioSettings.volume || 50) / 100;
        
        if (audioSettings.autoplay) {
            audio.autoplay = true;
        }
        
        document.body.appendChild(audio);
    }

    async save() {
        try {
            this.showSaveIndicator('saving');
            
            const success = await this.cacheManager.publishChanges();
            
            if (success) {
                this.showSaveIndicator('saved');
                this.history.markSaved();
            } else {
                this.showSaveIndicator('error');
            }
        } catch (error) {
            console.error('Failed to save page:', error);
            this.showSaveIndicator('error');
        }
    }

    cancel() {
        if (this.cacheManager.hasUnsavedChanges() || this.history.hasUnsavedChanges()) {
            if (confirm('آیا مطمئن هستید؟ تغییرات ذخیره نشده از بین خواهد رفت.')) {
                window.location.href = '/Page';
            }
        } else {
            window.location.href = '/Page';
        }
    }

    copy() {
        if (this.selectedElement) {
            this.clipboard = { 
                type: this.selectedElement.type,
                config: { ...this.selectedElement.config }
            };
            console.log('Element copied to clipboard');
            
            // Visual feedback
            this.showToast('المنت کپی شد', 'success');
        }
    }

    paste() {
        if (this.clipboard) {
            const config = {
                ...this.clipboard.config,
                x: this.clipboard.config.x + 20,
                y: this.clipboard.config.y + 20
            };
            
            const element = this.elementManager.createElement(this.clipboard.type, config);
            this.history.addCommand(new AddElementCommand(element));
            this.updateElementsList();
            this.selectionManager.selectElement(element);
            
            // Visual feedback
            this.showToast('المنت چسبانده شد', 'success');
        }
    }

    showToast(message, type = 'info') {
        // Simple toast notification
        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            background: ${type === 'success' ? '#d1e7dd' : type === 'error' ? '#f8d7da' : '#cff4fc'};
            color: ${type === 'success' ? '#0f5132' : type === 'error' ? '#721c24' : '#055160'};
            padding: 12px 20px;
            border-radius: 8px;
            border: 1px solid ${type === 'success' ? '#badbcc' : type === 'error' ? '#f5c2c7' : '#b6effb'};
            z-index: 10000;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
        `;
        
        document.body.appendChild(toast);
        
        // Animate in
        setTimeout(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateX(0)';
        }, 10);
        
        // Animate out and remove
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    updateElementsList() {
        const container = document.getElementById('elements-container');
        if (!container) return;
        
        const noElements = container.querySelector('.no-elements');
        
        if (this.elements.size === 0) {
            if (noElements) noElements.style.display = 'block';
            return;
        }
        
        if (noElements) noElements.style.display = 'none';
        
        // Clear existing elements
        container.querySelectorAll('.element-item:not(.no-elements)').forEach(item => item.remove());
        
        // Add current elements
        this.elements.forEach((element, id) => {
            const elementItem = this.createElementListItem(element);
            container.appendChild(elementItem);
        });
    }

    createElementListItem(element) {
        const item = document.createElement('div');
        item.className = 'element-list-item';
        item.dataset.elementId = element.id;
        
        const typeIcons = {
            text: 'fas fa-font',
            image: 'fas fa-image',
            video: 'fas fa-video',
            camera: 'fas fa-camera',
            clock: 'fas fa-clock',
            weather: 'fas fa-cloud-sun',
            chart: 'fas fa-chart-bar',
            gauge: 'fas fa-tachometer-alt',
            button: 'fas fa-square',
            slider: 'fas fa-sliders-h',
            switch: 'fas fa-toggle-on',
            input: 'fas fa-keyboard',
            label: 'fas fa-tag',
            shape: 'fas fa-shapes'
        };
        
        const displayName = this.getElementDisplayName(element);
        
        item.innerHTML = `
            <div class="element-info">
                <div class="element-icon">
                    <i class="${typeIcons[element.type] || 'fas fa-square'}"></i>
                </div>
                <div class="element-details">
                    <div class="element-name">${displayName}</div>
                    <div class="element-size">${element.config.width}×${element.config.height}</div>
                </div>
            </div>
            <div class="element-actions">
                <button class="btn btn-outline-primary btn-sm" onclick="pageEditor.selectElement('${element.id}')" title="انتخاب">
                    <i class="fas fa-mouse-pointer"></i>
                </button>
                <button class="btn btn-outline-secondary btn-sm" onclick="pageEditor.duplicateElementById('${element.id}')" title="کپی">
                    <i class="fas fa-copy"></i>
                </button>
                <button class="btn btn-outline-danger btn-sm" onclick="pageEditor.deleteElementById('${element.id}')" title="حذف">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
        
        // Click to select
        item.querySelector('.element-info').addEventListener('click', () => {
            this.selectElement(element.id);
        });
        
        return item;
    }

    getElementDisplayName(element) {
        if (element.config.title) return element.config.title;
        if (element.config.content) return element.config.content.substring(0, 20) + '...';
        if (element.config.text) return element.config.text.substring(0, 20) + '...';
        
        // Fallback to type names in Persian
        const typeNames = {
            text: 'متن',
            image: 'تصویر', 
            video: 'ویدیو',
            camera: 'دوربین',
            clock: 'ساعت',
            weather: 'آب و هوا',
            chart: 'نمودار',
            gauge: 'گیج',
            button: 'دکمه',
            slider: 'اسلایدر',
            switch: 'کلید',
            input: 'ورودی',
            label: 'برچسب',
            shape: 'شکل'
        };
        
        return typeNames[element.type] || element.type;
    }

    selectElement(elementId) {
        const element = this.elements.get(elementId);
        if (element) {
            this.selectionManager.selectElement(element);
            this.switchSidebarTab('properties');
        }
    }

    duplicateElementById(elementId) {
        const element = this.elements.get(elementId);
        if (element) {
            this.selectionManager.selectElement(element);
            this.duplicateElement();
        }
    }

    deleteElementById(elementId) {
        if (confirm('آیا از حذف این المنت مطمئن هستید؟')) {
            this.elementManager.deleteElement(elementId);
        }
    }

    showSaveIndicator(status) {
        const indicator = document.getElementById('save-indicator');
        if (!indicator) return;
        
        const statusConfig = {
            saving: { class: 'saving', icon: 'fas fa-spinner fa-spin', text: 'در حال ذخیره...' },
            saved: { class: 'saved', icon: 'fas fa-check', text: 'ذخیره شده' },
            error: { class: 'error', icon: 'fas fa-exclamation-triangle', text: 'خطا در ذخیره' },
            loading: { class: 'saving', icon: 'fas fa-spinner fa-spin', text: 'در حال بارگذاری...' }
        };
        
        const config = statusConfig[status];
        if (config) {
            indicator.className = `save-indicator ${config.class}`;
            indicator.innerHTML = `<i class="${config.icon} me-1"></i>${config.text}`;
        }
    }

    startAutoSave() {
        if (this.options.autoSave) {
            setInterval(() => {
                if (this.cacheManager.hasUnsavedChanges()) {
                    this.cacheManager.saveDraft();
                }
            }, this.options.autoSaveInterval);
        }
    }

    beforeUnload() {
        if (this.cacheManager.hasUnsavedChanges()) {
            this.cacheManager.saveDraft();
        }
    }

    // Public API methods
    getElement(id) {
        return this.elements.get(id);
    }

    getAllElements() {
        return Array.from(this.elements.values());
    }

    getCanvasSize() {
        return {
            width: this.options.canvasWidth,
            height: this.options.canvasHeight
        };
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
                color: '#000000',
                backgroundColor: 'transparent',
                fontFamily: 'Iran Sans',
                textAlign: 'right'
            },
            image: {
                width: 300,
                height: 200,
                x: 100,
                y: 100,
                src: '',
                alt: 'تصویر',
                objectFit: 'cover'
            },
            video: {
                width: 400,
                height: 300,
                x: 100,
                y: 100,
                src: '',
                autoplay: false,
                loop: false,
                controls: true,
                muted: false
            },
            camera: {
                width: 320,
                height: 240,
                x: 100,
                y: 100,
                cameraId: '',
                title: 'دوربین',
                showControls: true,
                autoStart: true
            },
            clock: {
                width: 200,
                height: 100,
                x: 100,
                y: 100,
                format: '24h',
                showSeconds: true,
                showDate: false,
                timeZone: 'Asia/Tehran'
            },
            weather: {
                width: 250,
                height: 150,
                x: 100,
                y: 100,
                location: 'Tehran',
                showForecast: true,
                units: 'metric',
                updateInterval: 600000 // 10 minutes
            },
            chart: {
                width: 400,
                height: 300,
                x: 100,
                y: 100,
                chartType: 'line',
                dataSource: '',
                title: 'نمودار',
                showLegend: true
            },
            gauge: {
                width: 200,
                height: 200,
                x: 100,
                y: 100,
                min: 0,
                max: 100,
                value: 50,
                title: 'گیج',
                unit: '%'
            },
            button: {
                width: 120,
                height: 40,
                x: 100,
                y: 100,
                text: 'دکمه',
                action: 'alert',
                backgroundColor: '#007bff',
                textColor: '#ffffff'
            },
            slider: {
                width: 200,
                height: 30,
                x: 100,
                y: 100,
                min: 0,
                max: 100,
                value: 50,
                step: 1
            },
            switch: {
                width: 60,
                height: 30,
                x: 100,
                y: 100,
                value: false,
                onText: 'روشن',
                offText: 'خاموش'
            },
            input: {
                width: 200,
                height: 35,
                x: 100,
                y: 100,
                placeholder: 'متن را وارد کنید',
                inputType: 'text',
                value: ''
            },
            label: {
                width: 100,
                height: 25,
                x: 100,
                y: 100,
                text: 'برچسب',
                fontSize: 14,
                color: '#495057'
            },
            shape: {
                width: 100,
                height: 100,
                x: 100,
                y: 100,
                shapeType: 'rectangle',
                fillColor: '#f8f9fa',
                borderColor: '#dee2e6',
                borderWidth: 1
            }
        };
        
        return configs[type] || {};
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
});

function getPageIdFromUrl() {
    const pathParts = window.location.pathname.split('/');
    const editIndex = pathParts.indexOf('Edit');
    return editIndex !== -1 && pathParts[editIndex + 1] ? pathParts[editIndex + 1] : null;
}
