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
        element.id = id;
        element.dataset.elementId = id;
        element.dataset.type = type; // اصلاح شده: type به جای elementType
        
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

    showElementProperties(element) {
        // نمایش panel تنظیمات المنت
        console.log('ElementManager.showElementProperties called for:', element.id);
        
        // پیدا کردن properties panel
        const propertiesPanel = document.getElementById('properties-panel');
        if (!propertiesPanel) {
            console.error('Properties panel not found');
            return;
        }
        
        // پاک کردن محتوای قبلی
        propertiesPanel.innerHTML = '';
        
        // ایجاد فرم تنظیمات بر اساس نوع المنت
        const elementType = element.dataset.type || 'unknown';
        const elementId = element.id;
        
        // عنوان panel
        const title = document.createElement('h6');
        title.className = 'mb-3';
        title.innerHTML = `<i class="fas fa-cog me-2"></i>تنظیمات ${this.getElementTypeLabel(elementType)}`;
        propertiesPanel.appendChild(title);
        
        // ایجاد تب‌ها
        const tabsContainer = document.createElement('div');
        tabsContainer.className = 'element-tabs mb-3';
        tabsContainer.innerHTML = `
            <ul class="nav nav-pills nav-fill" role="tablist">
                <li class="nav-item" role="presentation">
                    <button class="nav-link active" id="content-tab-${elementId}" 
                            data-bs-toggle="pill" data-bs-target="#content-panel-${elementId}" 
                            type="button" role="tab">
                        <i class="fas fa-edit me-1"></i>محتوا
                    </button>
                </li>
                <li class="nav-item" role="presentation">
                    <button class="nav-link" id="style-tab-${elementId}" 
                            data-bs-toggle="pill" data-bs-target="#style-panel-${elementId}" 
                            type="button" role="tab">
                        <i class="fas fa-paint-brush me-1"></i>استایل
                    </button>
                </li>
            </ul>
        `;
        propertiesPanel.appendChild(tabsContainer);
        
        // ایجاد محتوای تب‌ها
        const tabContent = document.createElement('div');
        tabContent.className = 'tab-content';
        tabContent.innerHTML = `
            <div class="tab-pane fade show active" id="content-panel-${elementId}" role="tabpanel">
                <form class="element-content-form" data-element-id="${elementId}"></form>
            </div>
            <div class="tab-pane fade" id="style-panel-${elementId}" role="tabpanel">
                <form class="element-style-form" data-element-id="${elementId}"></form>
            </div>
        `;
        propertiesPanel.appendChild(tabContent);
        
        // پیدا کردن فرم‌ها
        const contentForm = tabContent.querySelector('.element-content-form');
        const styleForm = tabContent.querySelector('.element-style-form');
        
        // اضافه کردن تنظیمات محتوا
        this.addContentProperties(contentForm, element, elementType);
        
        // اضافه کردن تنظیمات استایل
        this.addStyleProperties(styleForm, element, elementType);
        
        // اضافه کردن event listeners برای تغییرات
        this.attachPropertiesEventListeners(contentForm, element);
        this.attachPropertiesEventListeners(styleForm, element);
        
        // اضافه کردن tab functionality
        this.initializePropertyTabs(elementId);
    }
    
    initializePropertyTabs(elementId) {
        // Manual tab switching اضافه کردن
        const contentTab = document.getElementById(`content-tab-${elementId}`);
        const styleTab = document.getElementById(`style-tab-${elementId}`);
        const contentPanel = document.getElementById(`content-panel-${elementId}`);
        const stylePanel = document.getElementById(`style-panel-${elementId}`);
        
        if (contentTab && styleTab && contentPanel && stylePanel) {
            contentTab.addEventListener('click', () => {
                // Remove active from all tabs
                contentTab.classList.add('active');
                styleTab.classList.remove('active');
                
                // Show/hide panels
                contentPanel.classList.add('show', 'active');
                contentPanel.classList.remove('fade');
                stylePanel.classList.remove('show', 'active');
                stylePanel.classList.add('fade');
            });
            
            styleTab.addEventListener('click', () => {
                // Remove active from all tabs
                styleTab.classList.add('active');
                contentTab.classList.remove('active');
                
                // Show/hide panels
                stylePanel.classList.add('show', 'active');
                stylePanel.classList.remove('fade');
                contentPanel.classList.remove('show', 'active');
                contentPanel.classList.add('fade');
            });
        }
    }
    
    getElementTypeLabel(type) {
        const labels = {
            'text': 'متن',
            'image': 'تصویر', 
            'video': 'ویدیو',
            'camera': 'دوربین',
            'clock': 'ساعت',
            'weather': 'آب و هوا'
        };
        return labels[type] || type;
    }
    
    addContentProperties(form, element, elementType) {
        // تنظیمات محتوا بر اساس نوع المنت
        switch (elementType) {
            case 'text':
                this.addTextContentProperties(form, element);
                break;
            case 'image':
                this.addImageContentProperties(form, element);
                break;
            case 'video':
                this.addVideoContentProperties(form, element);
                break;
            case 'camera':
                this.addCameraContentProperties(form, element);
                break;
            case 'clock':
                this.addClockContentProperties(form, element);
                break;
            case 'weather':
                this.addWeatherContentProperties(form, element);
                break;
            default:
                // پیام پیش‌فرض برای انواع ناشناخته
                const defaultGroup = document.createElement('div');
                defaultGroup.className = 'text-center text-muted p-3';
                defaultGroup.innerHTML = `
                    <i class="fas fa-question-circle mb-2"></i>
                    <p class="small mb-0">نوع المنت ناشناخته: ${elementType}</p>
                `;
                form.appendChild(defaultGroup);
        }
    }
    
    addStyleProperties(form, element, elementType) {
        // تنظیمات موقعیت و اندازه (مشترک)
        const positionGroup = document.createElement('div');
        positionGroup.className = 'mb-3';
        positionGroup.innerHTML = `
            <label class="form-label">موقعیت و اندازه</label>
            <div class="row g-2">
                <div class="col-6">
                    <label class="form-label small">X:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="x" value="${parseInt(element.style.left) || 0}">
                </div>
                <div class="col-6">
                    <label class="form-label small">Y:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="y" value="${parseInt(element.style.top) || 0}">
                </div>
                <div class="col-6">
                    <label class="form-label small">عرض:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="width" value="${parseInt(element.style.width) || 100}">
                </div>
                <div class="col-6">
                    <label class="form-label small">ارتفاع:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="height" value="${parseInt(element.style.height) || 100}">
                </div>
            </div>
        `;
        form.appendChild(positionGroup);
        
        // تنظیمات رنگ‌ها (مشترک)
        const colorGroup = document.createElement('div');
        colorGroup.className = 'mb-3';
        colorGroup.innerHTML = `
            <label class="form-label">رنگ‌ها</label>
            <div class="row g-2">
                <div class="col-6">
                    <label class="form-label small">رنگ متن:</label>
                    <input type="color" class="form-control form-control-sm" 
                           name="color" value="${element.style.color || '#000000'}">
                </div>
                <div class="col-6">
                    <label class="form-label small">رنگ پس‌زمینه:</label>
                    <input type="color" class="form-control form-control-sm" 
                           name="backgroundColor" value="${element.style.backgroundColor || '#ffffff'}">
                </div>
            </div>
        `;
        form.appendChild(colorGroup);
        
        // تنظیمات فونت (فقط برای text)
        if (elementType === 'text') {
            const fontGroup = document.createElement('div');
            fontGroup.className = 'mb-3';
            fontGroup.innerHTML = `
                <label class="form-label">فونت</label>
                <div class="row g-2">
                    <div class="col-6">
                        <label class="form-label small">اندازه فونت:</label>
                        <input type="number" class="form-control form-control-sm" 
                               name="fontSize" value="${parseInt(element.style.fontSize) || 14}">
                    </div>
                    <div class="col-6">
                        <label class="form-label small">ضخامت فونت:</label>
                        <select class="form-select form-select-sm" name="fontWeight">
                            <option value="normal" ${element.style.fontWeight === 'normal' ? 'selected' : ''}>عادی</option>
                            <option value="bold" ${element.style.fontWeight === 'bold' ? 'selected' : ''}>ضخیم</option>
                        </select>
                    </div>
                </div>
            `;
            form.appendChild(fontGroup);
        }
        
        // تنظیمات border و shadow (مشترک)
        const decorationGroup = document.createElement('div');
        decorationGroup.className = 'mb-3';
        decorationGroup.innerHTML = `
            <label class="form-label">حاشیه و سایه</label>
            <div class="mb-2">
                <label class="form-label small">حاشیه:</label>
                <input type="text" class="form-control form-control-sm" 
                       name="border" value="${element.style.border || ''}" 
                       placeholder="1px solid #000">
            </div>
            <div class="mb-2">
                <label class="form-label small">گردی حاشیه:</label>
                <input type="number" class="form-control form-control-sm" 
                       name="borderRadius" value="${parseInt(element.style.borderRadius) || 0}">
            </div>
            <div class="mb-2">
                <label class="form-label small">سایه:</label>
                <input type="text" class="form-control form-control-sm" 
                       name="boxShadow" value="${element.style.boxShadow || ''}" 
                       placeholder="2px 2px 4px rgba(0,0,0,0.3)">
            </div>
        `;
        form.appendChild(decorationGroup);
    }
    
    // متدهای تنظیمات محتوا
    addTextContentProperties(form, element) {
        const textGroup = document.createElement('div');
        textGroup.className = 'mb-3';
        textGroup.innerHTML = `
            <label class="form-label">محتوای متن</label>
            <div class="mb-2">
                <textarea class="form-control" name="content" rows="4" 
                          placeholder="متن خود را اینجا بنویسید...">${element.textContent || ''}</textarea>
            </div>
        `;
        form.appendChild(textGroup);
    }
    
    addImageContentProperties(form, element) {
        const img = element.querySelector('img');
        const imageGroup = document.createElement('div');
        imageGroup.className = 'mb-3';
        imageGroup.innerHTML = `
            <label class="form-label">تنظیمات تصویر</label>
            <div class="mb-2">
                <label class="form-label small">آدرس تصویر:</label>
                <input type="url" class="form-control" 
                       name="src" value="${img?.src || ''}" 
                       placeholder="https://example.com/image.jpg">
            </div>
            <div class="mb-2">
                <label class="form-label small">متن جایگزین:</label>
                <input type="text" class="form-control" 
                       name="alt" value="${img?.alt || ''}" 
                       placeholder="توضیح تصویر">
            </div>
        `;
        form.appendChild(imageGroup);
    }
    
    addVideoContentProperties(form, element) {
        const video = element.querySelector('video');
        const videoGroup = document.createElement('div');
        videoGroup.className = 'mb-3';
        videoGroup.innerHTML = `
            <label class="form-label">تنظیمات ویدیو</label>
            <div class="mb-2">
                <label class="form-label small">آدرس ویدیو:</label>
                <input type="url" class="form-control" 
                       name="src" value="${video?.src || ''}" 
                       placeholder="https://example.com/video.mp4">
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="autoplay" 
                       ${video?.autoplay ? 'checked' : ''}>
                <label class="form-check-label">پخش خودکار</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="loop" 
                       ${video?.loop ? 'checked' : ''}>
                <label class="form-check-label">تکرار</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="controls" 
                       ${video?.controls ? 'checked' : ''}>
                <label class="form-check-label">نمایش کنترل‌ها</label>
            </div>
        `;
        form.appendChild(videoGroup);
    }
    
    addCameraContentProperties(form, element) {
        const cameraGroup = document.createElement('div');
        cameraGroup.className = 'mb-3';
        cameraGroup.innerHTML = `
            <label class="form-label">تنظیمات دوربین</label>
            <div class="mb-2">
                <label class="form-label small">عنوان دوربین:</label>
                <input type="text" class="form-control" 
                       name="title" value="${element.querySelector('span')?.textContent || 'دوربین'}" 
                       placeholder="نام دوربین">
            </div>
            <div class="mb-2">
                <label class="form-label small">آدرس استریم:</label>
                <input type="url" class="form-control" 
                       name="streamUrl" value="" 
                       placeholder="rtsp://camera-ip/stream">
            </div>
        `;
        form.appendChild(cameraGroup);
    }
    
    addClockContentProperties(form, element) {
        const clockGroup = document.createElement('div');
        clockGroup.className = 'mb-3';
        clockGroup.innerHTML = `
            <label class="form-label">تنظیمات ساعت</label>
            <div class="mb-2">
                <label class="form-label small">فرمت نمایش:</label>
                <select class="form-select" name="format">
                    <option value="24">24 ساعته</option>
                    <option value="12">12 ساعته</option>
                </select>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="showSeconds" checked>
                <label class="form-check-label">نمایش ثانیه</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="showDate">
                <label class="form-check-label">نمایش تاریخ</label>
            </div>
        `;
        form.appendChild(clockGroup);
    }
    
    addWeatherContentProperties(form, element) {
        const weatherGroup = document.createElement('div');
        weatherGroup.className = 'mb-3';
        weatherGroup.innerHTML = `
            <label class="form-label">تنظیمات آب و هوا</label>
            <div class="mb-2">
                <label class="form-label small">شهر:</label>
                <input type="text" class="form-control" 
                       name="location" value="تهران" 
                       placeholder="نام شهر">
            </div>
            <div class="mb-2">
                <label class="form-label small">واحد دما:</label>
                <select class="form-select" name="tempUnit">
                    <option value="celsius">سلسیوس</option>
                    <option value="fahrenheit">فارنهایت</option>
                </select>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="showIcon" checked>
                <label class="form-check-label">نمایش آیکون</label>
            </div>
        `;
        form.appendChild(weatherGroup);
    }
    
    addGeneralProperties(form, element) {
        // گروه موقعیت و اندازه
        const positionGroup = document.createElement('div');
        positionGroup.className = 'mb-3';
        positionGroup.innerHTML = `
            <label class="form-label">موقعیت و اندازه</label>
            <div class="row g-2">
                <div class="col-6">
                    <label class="form-label small">X:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="x" value="${parseInt(element.style.left) || 0}">
                </div>
                <div class="col-6">
                    <label class="form-label small">Y:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="y" value="${parseInt(element.style.top) || 0}">
                </div>
                <div class="col-6">
                    <label class="form-label small">عرض:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="width" value="${parseInt(element.style.width) || 100}">
                </div>
                <div class="col-6">
                    <label class="form-label small">ارتفاع:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="height" value="${parseInt(element.style.height) || 100}">
                </div>
            </div>
        `;
        form.appendChild(positionGroup);
        
        // گروه رنگ‌ها
        const colorGroup = document.createElement('div');
        colorGroup.className = 'mb-3';
        colorGroup.innerHTML = `
            <label class="form-label">رنگ‌ها</label>
            <div class="row g-2">
                <div class="col-6">
                    <label class="form-label small">رنگ متن:</label>
                    <input type="color" class="form-control form-control-sm" 
                           name="color" value="${element.style.color || '#000000'}">
                </div>
                <div class="col-6">
                    <label class="form-label small">رنگ پس‌زمینه:</label>
                    <input type="color" class="form-control form-control-sm" 
                           name="backgroundColor" value="${element.style.backgroundColor || '#ffffff'}">
                </div>
            </div>
        `;
        form.appendChild(colorGroup);
    }
    
    addTextProperties(form, element) {
        const textGroup = document.createElement('div');
        textGroup.className = 'mb-3';
        textGroup.innerHTML = `
            <label class="form-label">تنظیمات متن</label>
            <div class="mb-2">
                <label class="form-label small">متن:</label>
                <textarea class="form-control form-control-sm" name="content" rows="3">${element.textContent || ''}</textarea>
            </div>
            <div class="row g-2">
                <div class="col-6">
                    <label class="form-label small">اندازه فونت:</label>
                    <input type="number" class="form-control form-control-sm" 
                           name="fontSize" value="${parseInt(element.style.fontSize) || 14}">
                </div>
                <div class="col-6">
                    <label class="form-label small">ضخامت فونت:</label>
                    <select class="form-select form-select-sm" name="fontWeight">
                        <option value="normal" ${element.style.fontWeight === 'normal' ? 'selected' : ''}>عادی</option>
                        <option value="bold" ${element.style.fontWeight === 'bold' ? 'selected' : ''}>ضخیم</option>
                    </select>
                </div>
            </div>
        `;
        form.appendChild(textGroup);
    }
    
    addImageProperties(form, element) {
        const img = element.querySelector('img');
        const imageGroup = document.createElement('div');
        imageGroup.className = 'mb-3';
        imageGroup.innerHTML = `
            <label class="form-label">تنظیمات تصویر</label>
            <div class="mb-2">
                <label class="form-label small">آدرس تصویر:</label>
                <input type="url" class="form-control form-control-sm" 
                       name="src" value="${img?.src || ''}" placeholder="https://example.com/image.jpg">
            </div>
            <div class="mb-2">
                <label class="form-label small">متن جایگزین:</label>
                <input type="text" class="form-control form-control-sm" 
                       name="alt" value="${img?.alt || ''}" placeholder="توضیح تصویر">
            </div>
        `;
        form.appendChild(imageGroup);
    }
    
    addVideoProperties(form, element) {
        const video = element.querySelector('video');
        const videoGroup = document.createElement('div');
        videoGroup.className = 'mb-3';
        videoGroup.innerHTML = `
            <label class="form-label">تنظیمات ویدیو</label>
            <div class="mb-2">
                <label class="form-label small">آدرس ویدیو:</label>
                <input type="url" class="form-control form-control-sm" 
                       name="src" value="${video?.src || ''}" placeholder="https://example.com/video.mp4">
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="autoplay" 
                       ${video?.autoplay ? 'checked' : ''}>
                <label class="form-check-label small">پخش خودکار</label>
            </div>
            <div class="form-check mb-2">
                <input class="form-check-input" type="checkbox" name="loop" 
                       ${video?.loop ? 'checked' : ''}>
                <label class="form-check-label small">تکرار</label>
            </div>
        `;
        form.appendChild(videoGroup);
    }
    
    addCameraProperties(form, element) {
        const cameraGroup = document.createElement('div');
        cameraGroup.className = 'mb-3';
        cameraGroup.innerHTML = `
            <label class="form-label">تنظیمات دوربین</label>
            <div class="mb-2">
                <label class="form-label small">عنوان:</label>
                <input type="text" class="form-control form-control-sm" 
                       name="title" value="${element.querySelector('span')?.textContent || 'دوربین'}" 
                       placeholder="نام دوربین">
            </div>
        `;
        form.appendChild(cameraGroup);
    }
    
    addClockProperties(form, element) {
        const clockGroup = document.createElement('div');
        clockGroup.className = 'mb-3';
        clockGroup.innerHTML = `
            <label class="form-label">تنظیمات ساعت</label>
            <div class="mb-2">
                <label class="form-label small">فرمت:</label>
                <select class="form-select form-select-sm" name="format">
                    <option value="24">24 ساعته</option>
                    <option value="12">12 ساعته</option>
                </select>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" name="showSeconds" checked>
                <label class="form-check-label small">نمایش ثانیه</label>
            </div>
        `;
        form.appendChild(clockGroup);
    }
    
    addWeatherProperties(form, element) {
        const weatherGroup = document.createElement('div');
        weatherGroup.className = 'mb-3';
        weatherGroup.innerHTML = `
            <label class="form-label">تنظیمات آب و هوا</label>
            <div class="mb-2">
                <label class="form-label small">شهر:</label>
                <input type="text" class="form-control form-control-sm" 
                       name="location" value="تهران" placeholder="نام شهر">
            </div>
        `;
        form.appendChild(weatherGroup);
    }
    
    attachPropertiesEventListeners(form, element) {
        // اضافه کردن event listeners برای تغییرات فوری
        form.addEventListener('input', (e) => {
            this.updateElementProperty(element, e.target.name, e.target.value, e.target.type);
        });
        
        form.addEventListener('change', (e) => {
            this.updateElementProperty(element, e.target.name, e.target.value, e.target.type);
        });
    }
    
    updateElementProperty(element, propertyName, value, inputType) {
        switch (propertyName) {
            case 'x':
                element.style.left = value + 'px';
                break;
            case 'y':
                element.style.top = value + 'px';
                break;
            case 'width':
                element.style.width = value + 'px';
                break;
            case 'height':
                element.style.height = value + 'px';
                break;
            case 'color':
                element.style.color = value;
                break;
            case 'backgroundColor':
                element.style.backgroundColor = value;
                break;
            case 'fontSize':
                element.style.fontSize = value + 'px';
                break;
            case 'fontWeight':
                element.style.fontWeight = value;
                break;
            case 'content':
                if (element.dataset.type === 'text') {
                    element.textContent = value;
                }
                break;
            case 'src':
                const mediaElement = element.querySelector('img, video');
                if (mediaElement) {
                    mediaElement.src = value;
                }
                break;
            case 'alt':
                const img = element.querySelector('img');
                if (img) {
                    img.alt = value;
                }
                break;
            case 'autoplay':
                const video = element.querySelector('video');
                if (video) {
                    video.autoplay = inputType === 'checkbox' ? e.target.checked : value === 'true';
                }
                break;
            case 'loop':
                const videoLoop = element.querySelector('video');
                if (videoLoop) {
                    videoLoop.loop = inputType === 'checkbox' ? e.target.checked : value === 'true';
                }
                break;
            case 'title':
                const titleSpan = element.querySelector('span');
                if (titleSpan) {
                    titleSpan.textContent = value;
                }
                break;
        }
        
        // آپدیت cache
        if (this.editor && this.editor.cacheManager) {
            this.editor.cacheManager.addChange('property-change', {
                elementId: element.id,
                property: propertyName,
                value: value,
                timestamp: Date.now()
            });
        }
        
        console.log(`Property updated: ${propertyName} = ${value} for element ${element.id}`);
    }

    hideElementProperties() {
        // مخفی کردن panel تنظیمات المنت
        console.log('ElementManager.hideElementProperties called');
        
        const propertiesPanel = document.getElementById('properties-panel');
        if (propertiesPanel) {
            propertiesPanel.innerHTML = `
                <div class="text-center text-muted p-4">
                    <i class="fas fa-mouse-pointer fa-2x mb-2"></i>
                    <p class="small mb-0">برای مشاهده تنظیمات، روی یک المنت راست کلیک کنید</p>
                </div>
            `;
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
        
        // Drag state
        this.isDragging = false;
        this.draggedElement = null;
        this.dragElement = null;
        this.dragStartX = 0;
        this.dragStartY = 0;
        this.dragOffset = { x: 0, y: 0 };
        
        // Managers
        this.elementManager = new ElementManager(this);
        this.cacheManager = new CacheManager(this);
        
        // Bind methods for event listeners
        this.handleMouseMove = this.handleMouseMove.bind(this);
        this.handleMouseUp = this.handleMouseUp.bind(this);
        
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
        // Canvas events - using event delegation for better element interaction
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            // Left click: move elements or pan viewport
            pageContent.addEventListener('click', (e) => this.handleCanvasClick(e));
            // Right click: select element and show properties
            pageContent.addEventListener('contextmenu', (e) => this.handleContextMenu(e));
            // Mouse down for dragging
            pageContent.addEventListener('mousedown', (e) => this.handleMouseDown(e));
        }
        
        // Header controls  
        const saveBtn = document.getElementById('save-btn');
        const cancelBtn = document.getElementById('cancel-btn');
        const applySizeBtn = document.getElementById('apply-size-btn');
        
        if (saveBtn) saveBtn.addEventListener('click', () => this.save());
        if (cancelBtn) cancelBtn.addEventListener('click', () => this.cancel());
        if (applySizeBtn) applySizeBtn.addEventListener('click', () => this.applyCanvasSize());
        
        // Mouse-based interaction (removed toolbar)
        // Left click: pan/move elements
        // Right click: select element and show properties in sidebar
        
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
        // Left click functionality: select for quick actions
        const clickedElement = e.target.closest('.page-element');
        
        if (!clickedElement) {
            // کلیک روی فضای خالی - انتخاب را پاک کن
            this.clearSelection();
        }
        // توجه: drag functionality در handleMouseDown پیاده‌سازی شده
    }
    
    /**
     * Handle mouse down - start dragging
     */
    handleMouseDown(e) {
        // فقط برای کلیک چپ
        if (e.button !== 0) return;
        
        const clickedElement = e.target.closest('.page-element');
        
        if (clickedElement) {
            // آماده‌سازی برای جابجایی المنت
            this.prepareElementMove(clickedElement, e);
        }
    }
    
    /**
     * آماده‌سازی برای جابجایی المنت
     */
    prepareElementMove(element, e) {
        // انتخاب المنت
        this.selectElementForMove(element);
        
        // ذخیره موقعیت شروع برای drag
        this.dragStartX = e.clientX;
        this.dragStartY = e.clientY;
        this.isDragging = true;
        this.dragElement = element;
        
        // اضافه کردن event listeners برای drag
        document.addEventListener('mousemove', this.handleMouseMove);
        document.addEventListener('mouseup', this.handleMouseUp);
        
        e.preventDefault();
        e.stopPropagation();
    }
    
    /**
     * انتخاب المنت برای جابجایی (بدون نمایش properties)
     */
    selectElementForMove(element) {
        // پاک کردن انتخاب قبلی
        this.clearSelection();
        
        // اضافه کردن کلاس dragging
        element.classList.add('dragging');
        
        // ذخیره المنت انتخاب شده
        this.selectedElement = element;
    }
    
    /**
     * handle mouse move for dragging
     */
    handleMouseMove(e) {
        if (this.isDragging && this.dragElement) {
            const deltaX = e.clientX - this.dragStartX;
            const deltaY = e.clientY - this.dragStartY;
            
            // محاسبه موقعیت جدید
            const rect = this.dragElement.getBoundingClientRect();
            const newLeft = rect.left + deltaX;
            const newTop = rect.top + deltaY;
            
            // اعمال موقعیت جدید
            this.dragElement.style.left = newLeft + 'px';
            this.dragElement.style.top = newTop + 'px';
            
            // آپدیت موقعیت شروع برای حرکت بعدی
            this.dragStartX = e.clientX;
            this.dragStartY = e.clientY;
            
            // آپدیت cache
            if (this.cacheManager) {
                this.cacheManager.addChange('element-move', {
                    elementId: this.dragElement.id,
                    position: { x: newLeft, y: newTop },
                    timestamp: Date.now()
                });
            }
        }
    }
    
    /**
     * Handle mouse up - end dragging
     */
    handleMouseUp(e) {
        if (this.isDragging) {
            this.isDragging = false;
            
            if (this.dragElement) {
                this.dragElement.classList.remove('dragging');
                this.dragElement = null;
            }
            
            // حذف event listeners
            document.removeEventListener('mousemove', this.handleMouseMove);
            document.removeEventListener('mouseup', this.handleMouseUp);
            
            console.log('Dragging ended');
        }
    }

    handleContextMenu(e) {
        e.preventDefault();
        
        // اگر روی یک المنت کلیک شده، آن را انتخاب کن و سایدبار تنظیمات را نمایش بده
        const clickedElement = e.target.closest('.page-element');
        if (clickedElement) {
            this.selectElement(clickedElement);
            this.showElementProperties(clickedElement);
        } else {
            // اگر روی فضای خالی کلیک شده، انتخاب را پاک کن
            this.clearSelection();
        }
    }
    
    /**
     * انتخاب المنت و نمایش تنظیمات در سایدبار
     */
    selectElement(element) {
        // پاک کردن انتخاب قبلی
        this.clearSelection();
        
        // اضافه کردن کلاس selected
        element.classList.add('selected');
        
        // ذخیره المنت انتخاب شده
        this.selectedElement = element;
        
        console.log('Element selected:', element.id);
    }
    
    /**
     * نمایش تنظیمات المنت در سایدبار
     */
    showElementProperties(element) {
        // تغییر سایدبار به تب Properties
        this.switchSidebarTab('properties');
        
        // نمایش تنظیمات المنت در سایدبار
        // این قسمت باید با کد سایدبار هماهنگ شود
        if (this.elementManager) {
            this.elementManager.showElementProperties(element);
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
        
        // مخفی کردن properties panel
        if (this.elementManager) {
            this.elementManager.hideElementProperties();
        }
    }

    showPageSettings() {
        // Show page settings modal with cache information
        const modal = document.getElementById('pageSettingsModal');
        if (modal) {
            // Update cache info before showing modal
            this.updateCacheInfoDisplay();
            
            const bootstrapModal = new bootstrap.Modal(modal);
            bootstrapModal.show();
        }
    }
    
    updateCacheInfoDisplay() {
        // Update cache statistics in the modal
        const cacheInfo = document.getElementById('cache-info');
        if (cacheInfo) {
            const stats = this.cacheManager.getCacheStats();
            const changeInfo = this.cacheManager.getChangeInfo();
            
            cacheInfo.innerHTML = `
                <div class="cache-stats">
                    <h6>وضعیت Cache:</h6>
                    <ul class="list-unstyled">
                        <li><strong>المنت‌ها:</strong> ${stats.elementCount} عدد</li>
                        <li><strong>اندازه:</strong> ${stats.sizeKB} KB</li>
                        <li><strong>سن Cache:</strong> ${stats.ageMinutes} دقیقه</li>
                        <li><strong>تغییرات ذخیره نشده:</strong> ${changeInfo.changeCount} عدد</li>
                        <li><strong>آخرین ذخیره:</strong> ${stats.lastSaveTime ? new Date(stats.lastSaveTime).toLocaleTimeString('fa-IR') : 'هرگز'}</li>
                        <li><strong>وضعیت:</strong> ${changeInfo.hasChanges ? '<span class="text-warning">نیاز به ذخیره</span>' : '<span class="text-success">ذخیره شده</span>'}</li>
                    </ul>
                    <div class="mt-2">
                        <button class="btn btn-sm btn-outline-primary" onclick="pageEditor.cacheManager.forceSave()">
                            <i class="fas fa-save"></i> ذخیره فوری
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="pageEditor.cacheManager.clearCache()">
                            <i class="fas fa-trash"></i> پاک کردن Cache
                        </button>
                    </div>
                </div>
            `;
        }
    }

    // Placeholder methods for core functionality
    async save() {
        if (!this.cacheManager.hasChanges()) {
            console.log('No changes to save');
            this.showToast('هیچ تغییری برای ذخیره وجود ندارد', 'info');
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
            
            // Force save to cache first
            this.cacheManager.forceSave();
            
            // Save to database via cache manager
            const success = await this.cacheManager.saveToDatabase();
            
            if (success) {
                // Clear cache after successful save
                this.cacheManager.clearCache();
                
                // Show success message with stats
                const stats = this.cacheManager.getCacheStats();
                this.showToast(`تغییرات با موفقیت ذخیره شد (${stats.elementCount} المنت)`, 'success');
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
            
            // Restore button state on error
            const saveBtn = document.getElementById('save-btn');
            if (saveBtn) {
                saveBtn.innerHTML = '<i class="fas fa-save"></i> <span>ذخیره</span>';
                saveBtn.disabled = false;
            }
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
        
        // Auto-save settings
        this.autoSaveInterval = 30000; // 30 seconds
        this.autoSaveTimer = null;
        this.lastSaveTime = 0;
        this.saveDelay = 2000; // 2 seconds delay after last change
        this.saveTimeout = null;
        
        // Change tracking
        this.changeBuffer = [];
        this.isProcessingChanges = false;
        
        this.init();
    }
    
    init() {
        // Store original state on page load
        this.storeOriginalState();
        
        // Set up change detection methods
        this.setupManualChangeTracking();
        this.setupMutationObserver();
        
        // Set up auto-save timer
        this.setupAutoSave();
        
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
    
    setupManualChangeTracking() {
        // This method will be called by element operations
        // No additional setup needed - just enable the addChange method
    }
    
    setupMutationObserver() {
        // Listen for DOM changes with debouncing
        let mutationTimer = null;
        
        const observer = new MutationObserver((mutations) => {
            // Clear previous timer
            if (mutationTimer) {
                clearTimeout(mutationTimer);
            }
            
            // Debounce mutations to avoid excessive saving
            mutationTimer = setTimeout(() => {
                // Only save if there are meaningful changes
                if (this.hasMeaningfulChanges(mutations)) {
                    this.addChange('dom-mutation', {
                        mutations: mutations.length,
                        timestamp: Date.now()
                    });
                }
            }, 500); // 500ms debounce
        });
        
        const pageContent = document.getElementById('page-content');
        if (pageContent) {
            observer.observe(pageContent, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['style', 'class', 'data-element-id'], // Only watch specific attributes
                attributeOldValue: true
            });
        }
    }
    
    hasMeaningfulChanges(mutations) {
        // Filter out insignificant mutations
        return mutations.some(mutation => {
            // Ignore text changes in temporary elements
            if (mutation.type === 'characterData') {
                return false;
            }
            
            // Ignore style changes for resize handles
            if (mutation.type === 'attributes' && 
                mutation.target.classList.contains('resize-handle')) {
                return false;
            }
            
            // Ignore temporary class changes
            if (mutation.type === 'attributes' && 
                mutation.attributeName === 'class' &&
                (mutation.target.classList.contains('dragging') || 
                 mutation.target.classList.contains('resizing'))) {
                return false;
            }
            
            return true;
        });
    }
    
    setupAutoSave() {
        // Set up periodic auto-save
        this.autoSaveTimer = setInterval(() => {
            if (this.hasUnsavedChanges && !this.isProcessingChanges) {
                console.log('Auto-saving due to timer...');
                this.saveToCache();
                this.processPendingChanges();
            }
        }, this.autoSaveInterval);
    }
    
    setupUnloadWarning() {
        window.addEventListener('beforeunload', (e) => {
            // Clean up timers
            if (this.autoSaveTimer) {
                clearInterval(this.autoSaveTimer);
            }
            if (this.saveTimeout) {
                clearTimeout(this.saveTimeout);
            }
            
            if (this.hasUnsavedChanges) {
                const message = 'شما تغییرات ذخیره نشده‌ای دارید. آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟';
                e.preventDefault();
                e.returnValue = message;
                return message;
            }
        });
    }
    
    // Manual change tracking method
    addChange(changeType, changeData) {
        const change = {
            type: changeType,
            data: changeData,
            timestamp: Date.now()
        };
        
        this.changeBuffer.push(change);
        this.markAsChanged();
        
        // Debounced save
        this.debouncedSave();
        
        console.log(`Change tracked: ${changeType}`, changeData);
    }
    
    debouncedSave() {
        // Clear previous timeout
        if (this.saveTimeout) {
            clearTimeout(this.saveTimeout);
        }
        
        // Set new timeout
        this.saveTimeout = setTimeout(() => {
            if (!this.isProcessingChanges) {
                this.saveToCache();
                this.processPendingChanges();
            }
        }, this.saveDelay);
    }
    
    processPendingChanges() {
        if (this.changeBuffer.length === 0) return;
        
        this.isProcessingChanges = true;
        
        try {
            // Group changes by type for optimization
            const groupedChanges = this.groupChangesByType(this.changeBuffer);
            console.log('Processing changes:', groupedChanges);
            
            // Clear processed changes
            this.changeBuffer = [];
            
            // Update last save time
            this.lastSaveTime = Date.now();
            
        } catch (error) {
            console.error('Error processing changes:', error);
        } finally {
            this.isProcessingChanges = false;
        }
    }
    
    groupChangesByType(changes) {
        const grouped = {};
        changes.forEach(change => {
            if (!grouped[change.type]) {
                grouped[change.type] = [];
            }
            grouped[change.type].push(change);
        });
        return grouped;
    }
    
    markAsChanged() {
        this.hasUnsavedChanges = true;
        this.updateSaveButtonState();
    }
    
    markAsSaved() {
        this.hasUnsavedChanges = false;
        this.updateSaveButtonState();
        
        // Clear change buffer
        this.changeBuffer = [];
    }
    
    updateSaveButtonState() {
        const saveBtn = document.getElementById('save-btn');
        if (saveBtn) {
            if (this.hasUnsavedChanges) {
                saveBtn.classList.add('has-changes');
                saveBtn.innerHTML = '<i class="fas fa-save"></i> ذخیره تغییرات';
                
                // Show change count if available
                const changeCount = this.changeBuffer.length;
                if (changeCount > 0) {
                    saveBtn.innerHTML = `<i class="fas fa-save"></i> ذخیره تغییرات (${changeCount})`;
                }
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
                pageId: this.pageId,
                changeCount: this.changeBuffer.length,
                lastSaveTime: this.lastSaveTime
            };
            
            localStorage.setItem(this.cacheKey, JSON.stringify(cacheData));
            console.log('Elements saved to cache:', elementsData.length, 'elements', 
                       this.changeBuffer.length, 'pending changes');
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
                    // Check cache age (don't load very old cache)
                    const cacheAge = Date.now() - (cacheData.timestamp || 0);
                    const maxCacheAge = 24 * 60 * 60 * 1000; // 24 hours
                    
                    if (cacheAge > maxCacheAge) {
                        console.log('Cache too old, clearing...');
                        this.clearCache();
                        return false;
                    }
                    
                    this.deserializeElements(cacheData.elements);
                    this.markAsChanged(); // Mark as having unsaved changes
                    
                    console.log('Loaded elements from cache:', cacheData.elements.length, 'elements');
                    console.log('Cache age:', Math.round(cacheAge / 1000 / 60), 'minutes');
                    
                    return true;
                }
            }
        } catch (error) {
            console.error('Failed to load from cache:', error);
            // Clear corrupted cache
            this.clearCache();
        }
        return false;
    }
    
    // Get cache statistics
    getCacheStats() {
        try {
            const cached = localStorage.getItem(this.cacheKey);
            if (cached) {
                const cacheData = JSON.parse(cached);
                const cacheAge = Date.now() - (cacheData.timestamp || 0);
                
                return {
                    exists: true,
                    elementCount: cacheData.elements?.length || 0,
                    ageMinutes: Math.round(cacheAge / 1000 / 60),
                    sizeKB: Math.round(cached.length / 1024),
                    lastSaveTime: cacheData.lastSaveTime || 0,
                    changeCount: cacheData.changeCount || 0,
                    version: cacheData.version || 'unknown'
                };
            }
        } catch (error) {
            console.error('Error getting cache stats:', error);
        }
        
        return {
            exists: false,
            elementCount: 0,
            ageMinutes: 0,
            sizeKB: 0,
            lastSaveTime: 0,
            changeCount: 0,
            version: 'none'
        };
    }
    
    // Force save (bypass debouncing)
    forceSave() {
        if (this.saveTimeout) {
            clearTimeout(this.saveTimeout);
            this.saveTimeout = null;
        }
        
        this.saveToCache();
        this.processPendingChanges();
        
        console.log('Forced save completed');
    }
    
    serializeElements() {
        const elements = [];
        this.editor.elements.forEach((element, id) => {
            // Get complete element data including styles and content
            const domElement = element.domElement;
            const computedStyle = window.getComputedStyle(domElement);
            
            // Extract configuration based on element type
            const config = this.extractElementConfig(element, domElement);
            
            const elementData = {
                id: id,
                type: element.type,
                config: config,
                position: {
                    x: element.config.x || 0,
                    y: element.config.y || 0,
                    width: element.config.width || 100,
                    height: element.config.height || 100
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
    
    extractElementConfig(element, domElement) {
        const baseConfig = {
            x: element.config.x || 0,
            y: element.config.y || 0,
            width: element.config.width || 100,
            height: element.config.height || 100
        };

        // Add type-specific configuration
        switch (element.type) {
            case 'text':
                return {
                    ...baseConfig,
                    content: element.config.content || domElement.textContent || '',
                    fontSize: element.config.fontSize || '14px',
                    color: element.config.color || '#000000',
                    backgroundColor: element.config.backgroundColor || 'transparent'
                };
                
            case 'image':
                const img = domElement.querySelector('img');
                return {
                    ...baseConfig,
                    src: element.config.src || (img ? img.src : ''),
                    alt: element.config.alt || (img ? img.alt : '')
                };
                
            case 'video':
                const video = domElement.querySelector('video');
                return {
                    ...baseConfig,
                    src: element.config.src || (video ? video.src : ''),
                    autoplay: element.config.autoplay || (video ? video.autoplay : false),
                    loop: element.config.loop || (video ? video.loop : false)
                };
                
            case 'camera':
                return {
                    ...baseConfig,
                    title: element.config.title || 'دوربین'
                };
                
            case 'clock':
                return {
                    ...baseConfig,
                    format: element.config.format || '24',
                    showSeconds: element.config.showSeconds || true
                };
                
            case 'weather':
                return {
                    ...baseConfig,
                    location: element.config.location || 'تهران'
                };
                
            default:
                return baseConfig;
        }
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
            // Transform elements to match expected API structure
            const transformedElements = cachedData.elements.map(element => ({
                id: element.id,
                type: element.type,
                config: element.config || {},
                position: element.position || { x: 0, y: 0, width: 100, height: 100 },
                styles: element.styles || {},
                content: element.content || { innerHTML: '', textContent: '', attributes: {} },
                timestamp: element.timestamp || Date.now()
            }));

            const response = await fetch(`/Page/${this.pageId}/elements`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    pageId: this.pageId,
                    elements: transformedElements
                })
            });
            
            if (response.ok) {
                this.markAsSaved();
                console.log('Elements saved to database successfully');
                return true;
            } else {
                const errorText = await response.text();
                console.error('Failed to save to database:', response.statusText, errorText);
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
        return this.hasUnsavedChanges || this.changeBuffer.length > 0;
    }
    
    // Get detailed change information
    getChangeInfo() {
        return {
            hasChanges: this.hasChanges(),
            changeCount: this.changeBuffer.length,
            lastChangeTime: this.changeBuffer.length > 0 ? 
                Math.max(...this.changeBuffer.map(c => c.timestamp)) : 0,
            timeSinceLastSave: Date.now() - this.lastSaveTime
        };
    }
    
    // Clean up resources
    destroy() {
        // Clear timers
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
            this.autoSaveTimer = null;
        }
        
        if (this.saveTimeout) {
            clearTimeout(this.saveTimeout);
            this.saveTimeout = null;
        }
        
        // Clear change buffer
        this.changeBuffer = [];
        
        console.log('CacheManager destroyed');
    }
}
