/**
 * Page Editor Main JavaScript Classes
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
            autoSaveInterval: 30000, // 30 seconds
            ...options
        };

        this.canvas = null;
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
            this.setupEventListeners();
            this.loadPage();
            this.startAutoSave();
            
            console.log('Page Editor initialized successfully');
        } catch (error) {
            console.error('Failed to initialize Page Editor:', error);
        }
    }

    setupCanvas() {
        this.canvas = document.createElement('div');
        this.canvas.className = 'page-canvas';
        this.canvas.style.width = this.options.canvasWidth + 'px';
        this.canvas.style.height = this.options.canvasHeight + 'px';
        
        // Add grid overlay
        const grid = document.createElement('div');
        grid.className = 'canvas-grid';
        this.canvas.appendChild(grid);
        
        // Add to editor content
        const editorContent = document.querySelector('.editor-content');
        editorContent.appendChild(this.canvas);
        
        // Update size inputs
        document.getElementById('page-width').value = this.options.canvasWidth;
        document.getElementById('page-height').value = this.options.canvasHeight;
    }

    setupEventListeners() {
        // Canvas events
        this.canvas.addEventListener('click', (e) => this.handleCanvasClick(e));
        this.canvas.addEventListener('contextmenu', (e) => this.handleContextMenu(e));
        
        // Header controls
        document.getElementById('save-btn').addEventListener('click', () => this.save());
        document.getElementById('cancel-btn').addEventListener('click', () => this.cancel());
        document.getElementById('apply-size-btn').addEventListener('click', () => this.updateCanvasSize());
        
        // Sidebar tabs
        document.querySelectorAll('.sidebar-tab').forEach(tab => {
            tab.addEventListener('click', (e) => this.switchSidebarTab(e.target.dataset.tab));
        });
        
        // Add element buttons
        document.querySelectorAll('.add-element-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.addElement(e.target.dataset.type));
        });
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyboard(e));
        
        // Window events
        window.addEventListener('beforeunload', () => this.beforeUnload());
    }

    handleCanvasClick(e) {
        if (e.target === this.canvas) {
            this.selectionManager.clearSelection();
        }
    }

    handleContextMenu(e) {
        e.preventDefault();
        // Show context menu logic here
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
            }
        }
        
        if (e.key === 'Delete' && this.selectedElement) {
            this.elementManager.deleteElement(this.selectedElement.id);
        }
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
        document.getElementById(`${tabName}-panel`).classList.add('active');
        document.querySelector(`[data-tab="${tabName}"]`).classList.add('active');
    }

    addElement(type) {
        const defaultConfig = this.getDefaultElementConfig(type);
        const element = this.elementManager.createElement(type, defaultConfig);
        this.history.addCommand(new AddElementCommand(element));
        this.updateElementsList();
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
                backgroundColor: 'transparent'
            },
            image: {
                width: 300,
                height: 200,
                x: 100,
                y: 100,
                src: '',
                alt: 'تصویر'
            },
            video: {
                width: 400,
                height: 300,
                x: 100,
                y: 100,
                src: '',
                autoplay: false,
                loop: false
            },
            camera: {
                width: 320,
                height: 240,
                x: 100,
                y: 100,
                cameraId: '',
                title: 'دوربین'
            },
            clock: {
                width: 200,
                height: 100,
                x: 100,
                y: 100,
                format: '24h',
                showSeconds: true
            },
            weather: {
                width: 250,
                height: 150,
                x: 100,
                y: 100,
                location: 'Tehran',
                showForecast: true
            }
        };
        
        return configs[type] || {};
    }

    updateCanvasSize() {
        const width = parseInt(document.getElementById('page-width').value);
        const height = parseInt(document.getElementById('page-height').value);
        
        if (width > 0 && height > 0) {
            this.options.canvasWidth = width;
            this.options.canvasHeight = height;
            
            this.canvas.style.width = width + 'px';
            this.canvas.style.height = height + 'px';
            
            this.cacheManager.addChange('canvas-resize', { width, height });
            this.showSaveIndicator('saving');
        }
    }

    async loadPage() {
        try {
            this.showSaveIndicator('loading');
            
            const response = await fetch(`/api/page-edit/${this.pageId}/edit-session`);
            const data = await response.json();
            
            if (data.success) {
                // Load page elements
                if (data.page && data.page.elements) {
                    data.page.elements.forEach(elementData => {
                        this.elementManager.createElement(elementData.type, elementData);
                    });
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
        if (confirm('آیا مطمئن هستید؟ تغییرات ذخیره نشده از بین خواهد رفت.')) {
            window.location.href = '/Page';
        }
    }

    copy() {
        if (this.selectedElement) {
            this.clipboard = { ...this.selectedElement.config };
            console.log('Element copied to clipboard');
        }
    }

    paste() {
        if (this.clipboard) {
            const config = {
                ...this.clipboard,
                x: this.clipboard.x + 20,
                y: this.clipboard.y + 20
            };
            
            const element = this.elementManager.createElement(this.clipboard.type, config);
            this.history.addCommand(new AddElementCommand(element));
            this.updateElementsList();
        }
    }

    updateElementsList() {
        const container = document.getElementById('elements-container');
        const noElements = container.querySelector('.no-elements');
        
        if (this.elements.size === 0) {
            if (noElements) noElements.style.display = 'block';
            return;
        }
        
        if (noElements) noElements.style.display = 'none';
        
        // Clear existing elements
        container.querySelectorAll('.element-item').forEach(item => item.remove());
        
        // Add current elements
        this.elements.forEach((element, id) => {
            const elementItem = this.createElementListItem(element);
            container.appendChild(elementItem);
        });
    }

    createElementListItem(element) {
        const item = document.createElement('div');
        item.className = 'element-item';
        item.dataset.elementId = element.id;
        
        const typeIcons = {
            text: 'fas fa-font',
            image: 'fas fa-image',
            video: 'fas fa-video',
            camera: 'fas fa-camera',
            clock: 'fas fa-clock',
            weather: 'fas fa-cloud-sun'
        };
        
        item.innerHTML = `
            <div class="element-info">
                <div class="element-icon">
                    <i class="${typeIcons[element.type] || 'fas fa-square'}"></i>
                </div>
                <div class="element-details">
                    <h6>${element.config.title || element.type}</h6>
                    <small>${element.config.width}×${element.config.height}</small>
                </div>
            </div>
            <div class="element-actions">
                <button class="btn btn-outline-primary btn-sm" onclick="pageEditor.selectElement('${element.id}')">
                    <i class="fas fa-mouse-pointer"></i>
                </button>
                <button class="btn btn-outline-danger btn-sm" onclick="pageEditor.elementManager.deleteElement('${element.id}')">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
        
        item.addEventListener('click', () => this.selectElement(element.id));
        
        return item;
    }

    selectElement(elementId) {
        const element = this.elements.get(elementId);
        if (element) {
            this.selectionManager.selectElement(element);
            this.switchSidebarTab('properties');
        }
    }

    showSaveIndicator(status) {
        const indicator = document.getElementById('save-indicator');
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

    // Public API
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
