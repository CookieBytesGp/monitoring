/**
 * Element Manager Class
 */
class ElementManager {
    constructor(editor) {
        this.editor = editor;
        this.nextId = 1;
    }

    createElement(type, config) {
        const id = `element_${this.nextId++}`;
        const element = new PageElement(id, type, config);
        
        this.editor.elements.set(id, element);
        this.renderElement(element);
        
        this.editor.cacheManager.addChange('add-element', {
            id,
            type,
            config
        });
        
        return element;
    }

    renderElement(element) {
        const elementDiv = document.createElement('div');
        elementDiv.className = `page-element element-${element.type}`;
        elementDiv.id = element.id;
        elementDiv.dataset.elementId = element.id;
        
        this.applyElementStyles(elementDiv, element);
        this.setElementContent(elementDiv, element);
        this.addResizeHandles(elementDiv);
        
        // Add event listeners
        elementDiv.addEventListener('click', (e) => {
            e.stopPropagation();
            this.editor.selectionManager.selectElement(element);
        });
        
        elementDiv.addEventListener('mousedown', (e) => {
            this.editor.dragDropManager.startDrag(e, element);
        });
        
        this.editor.canvas.appendChild(elementDiv);
    }

    applyElementStyles(elementDiv, element) {
        const config = element.config;
        
        elementDiv.style.left = config.x + 'px';
        elementDiv.style.top = config.y + 'px';
        elementDiv.style.width = config.width + 'px';
        elementDiv.style.height = config.height + 'px';
        
        if (config.backgroundColor) {
            elementDiv.style.backgroundColor = config.backgroundColor;
        }
        
        if (config.borderColor && config.borderWidth > 0) {
            elementDiv.style.border = `${config.borderWidth}px solid ${config.borderColor}`;
        }
        
        if (config.borderRadius) {
            elementDiv.style.borderRadius = config.borderRadius + 'px';
        }
        
        if (config.opacity !== undefined) {
            elementDiv.style.opacity = config.opacity / 100;
        }
    }

    setElementContent(elementDiv, element) {
        switch (element.type) {
            case 'text':
                this.setTextContent(elementDiv, element);
                break;
            case 'image':
                this.setImageContent(elementDiv, element);
                break;
            case 'video':
                this.setVideoContent(elementDiv, element);
                break;
            case 'camera':
                this.setCameraContent(elementDiv, element);
                break;
            case 'clock':
                this.setClockContent(elementDiv, element);
                break;
            case 'weather':
                this.setWeatherContent(elementDiv, element);
                break;
        }
    }

    setTextContent(elementDiv, element) {
        const config = element.config;
        elementDiv.textContent = config.content || 'متن نمونه';
        
        if (config.fontSize) {
            elementDiv.style.fontSize = config.fontSize + 'px';
        }
        
        if (config.color) {
            elementDiv.style.color = config.color;
        }
        
        if (config.fontFamily) {
            elementDiv.style.fontFamily = config.fontFamily;
        }
        
        if (config.fontWeight) {
            elementDiv.style.fontWeight = config.fontWeight;
        }
        
        if (config.textAlign) {
            elementDiv.style.textAlign = config.textAlign;
        }
    }

    setImageContent(elementDiv, element) {
        const config = element.config;
        
        if (config.src) {
            const img = document.createElement('img');
            img.src = config.src;
            img.alt = config.alt || 'تصویر';
            img.style.width = '100%';
            img.style.height = '100%';
            img.style.objectFit = 'cover';
            elementDiv.appendChild(img);
        } else {
            elementDiv.innerHTML = `
                <div style="display: flex; align-items: center; justify-content: center; height: 100%; background: #f8f9fa; color: #6c757d;">
                    <i class="fas fa-image fa-2x"></i>
                </div>
            `;
        }
    }

    setVideoContent(elementDiv, element) {
        const config = element.config;
        
        if (config.src) {
            const video = document.createElement('video');
            video.src = config.src;
            video.controls = true;
            video.style.width = '100%';
            video.style.height = '100%';
            
            if (config.autoplay) video.autoplay = true;
            if (config.loop) video.loop = true;
            
            elementDiv.appendChild(video);
        } else {
            elementDiv.innerHTML = `
                <div style="display: flex; align-items: center; justify-content: center; height: 100%; background: #000; color: white;">
                    <i class="fas fa-video fa-2x"></i>
                </div>
            `;
        }
    }

    setCameraContent(elementDiv, element) {
        const config = element.config;
        elementDiv.innerHTML = `
            <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; background: #1a1a1a; color: white; text-align: center;">
                <i class="fas fa-camera fa-2x mb-2"></i>
                <div>${config.title || 'دوربین'}</div>
                ${config.cameraId ? `<small>ID: ${config.cameraId}</small>` : ''}
            </div>
        `;
    }

    setClockContent(elementDiv, element) {
        const config = element.config;
        const now = new Date();
        const timeFormat = config.format === '12h' ? 'en-US' : 'fa-IR';
        const options = {
            hour: '2-digit',
            minute: '2-digit',
            hour12: config.format === '12h'
        };
        
        if (config.showSeconds) {
            options.second = '2-digit';
        }
        
        const timeString = now.toLocaleTimeString(timeFormat, options);
        const dateString = now.toLocaleDateString('fa-IR');
        
        elementDiv.innerHTML = `
            <div style="text-align: center; font-family: 'Courier New', monospace;">
                <div style="font-size: 1.5em; font-weight: bold;">${timeString}</div>
                <div style="font-size: 0.8em; margin-top: 5px;">${dateString}</div>
            </div>
        `;
        
        // Update every second
        if (!elementDiv._clockInterval) {
            elementDiv._clockInterval = setInterval(() => {
                this.setClockContent(elementDiv, element);
            }, 1000);
        }
    }

    setWeatherContent(elementDiv, element) {
        const config = element.config;
        elementDiv.innerHTML = `
            <div style="text-align: center; color: white; padding: 10px;">
                <div style="display: flex; align-items: center; justify-content: center; margin-bottom: 10px;">
                    <i class="fas fa-sun fa-2x"></i>
                    <span style="font-size: 1.5em; margin-right: 10px;">25°</span>
                </div>
                <div style="font-size: 0.9em;">${config.location || 'تهران'}</div>
                ${config.showForecast ? '<div style="font-size: 0.7em; margin-top: 5px;">آفتابی</div>' : ''}
            </div>
        `;
    }

    addResizeHandles(elementDiv) {
        const handles = ['nw', 'ne', 'sw', 'se', 'n', 's', 'w', 'e'];
        
        handles.forEach(position => {
            const handle = document.createElement('div');
            handle.className = `resize-handle ${position}`;
            handle.addEventListener('mousedown', (e) => {
                e.stopPropagation();
                this.editor.dragDropManager.startResize(e, elementDiv, position);
            });
            elementDiv.appendChild(handle);
        });
    }

    updateElement(elementId, newConfig) {
        const element = this.editor.elements.get(elementId);
        if (!element) return;
        
        const oldConfig = { ...element.config };
        element.config = { ...element.config, ...newConfig };
        
        const elementDiv = document.getElementById(elementId);
        if (elementDiv) {
            this.applyElementStyles(elementDiv, element);
            this.setElementContent(elementDiv, element);
        }
        
        this.editor.cacheManager.addChange('update-element', {
            id: elementId,
            oldConfig,
            newConfig: element.config
        });
        
        this.editor.history.addCommand(new UpdateElementCommand(elementId, oldConfig, element.config));
    }

    deleteElement(elementId) {
        const element = this.editor.elements.get(elementId);
        if (!element) return;
        
        const elementDiv = document.getElementById(elementId);
        if (elementDiv) {
            // Clear any intervals
            if (elementDiv._clockInterval) {
                clearInterval(elementDiv._clockInterval);
            }
            elementDiv.remove();
        }
        
        this.editor.elements.delete(elementId);
        
        if (this.editor.selectedElement && this.editor.selectedElement.id === elementId) {
            this.editor.selectionManager.clearSelection();
        }
        
        this.editor.cacheManager.addChange('delete-element', {
            id: elementId,
            element: element
        });
        
        this.editor.history.addCommand(new DeleteElementCommand(element));
        this.editor.updateElementsList();
    }

    moveElement(elementId, newX, newY) {
        const element = this.editor.elements.get(elementId);
        if (!element) return;
        
        const oldPosition = { x: element.config.x, y: element.config.y };
        element.config.x = newX;
        element.config.y = newY;
        
        const elementDiv = document.getElementById(elementId);
        if (elementDiv) {
            elementDiv.style.left = newX + 'px';
            elementDiv.style.top = newY + 'px';
        }
        
        this.editor.cacheManager.addChange('move-element', {
            id: elementId,
            oldPosition,
            newPosition: { x: newX, y: newY }
        });
    }

    resizeElement(elementId, newWidth, newHeight) {
        const element = this.editor.elements.get(elementId);
        if (!element) return;
        
        const oldSize = { width: element.config.width, height: element.config.height };
        element.config.width = newWidth;
        element.config.height = newHeight;
        
        const elementDiv = document.getElementById(elementId);
        if (elementDiv) {
            elementDiv.style.width = newWidth + 'px';
            elementDiv.style.height = newHeight + 'px';
        }
        
        this.editor.cacheManager.addChange('resize-element', {
            id: elementId,
            oldSize,
            newSize: { width: newWidth, height: newHeight }
        });
    }

    /**
     * نمایش تنظیمات المنت در سایدبار
     */
    showElementProperties(elementDiv) {
        const elementId = elementDiv.id;
        const element = this.editor.elements.get(elementId);
        
        if (!element) {
            console.warn('Element not found:', elementId);
            return;
        }

        // نمایش properties content و مخفی کردن no-selection
        const noSelection = document.getElementById('no-selection');
        const propertiesContent = document.getElementById('properties-content');
        
        if (noSelection) noSelection.style.display = 'none';
        if (propertiesContent) propertiesContent.style.display = 'block';

        // پر کردن فرم‌ها با مقادیر المنت
        this.populatePropertiesForm(element);

        console.log('Properties shown for element:', elementId);
    }

    /**
     * پر کردن فرم تنظیمات با مقادیر المنت
     */
    populatePropertiesForm(element) {
        // موقعیت و اندازه
        const xInput = document.getElementById('element-x');
        const yInput = document.getElementById('element-y');
        const widthInput = document.getElementById('element-width');
        const heightInput = document.getElementById('element-height');

        if (xInput) xInput.value = element.config.x || 0;
        if (yInput) yInput.value = element.config.y || 0;
        if (widthInput) widthInput.value = element.config.width || 100;
        if (heightInput) heightInput.value = element.config.height || 100;

        // رنگ پس‌زمینه
        const bgColorInput = document.getElementById('element-bg-color');
        const bgColorTextInput = document.getElementById('element-bg-color-text');
        const bgColor = element.config.backgroundColor || '#ffffff';
        
        if (bgColorInput) bgColorInput.value = bgColor;
        if (bgColorTextInput) bgColorTextInput.value = bgColor;

        // شفافیت
        const opacityInput = document.getElementById('element-opacity');
        const opacityValue = document.getElementById('opacity-value');
        const opacity = (element.config.opacity || 1) * 100;
        
        if (opacityInput) opacityInput.value = opacity;
        if (opacityValue) opacityValue.textContent = opacity + '%';

        // تنظیمات خاص نوع المنت
        this.populateTypeSpecificProperties(element);

        // اضافه کردن event listeners
        this.addPropertyEventListeners(element);
    }

    /**
     * تنظیمات خاص نوع المنت
     */
    populateTypeSpecificProperties(element) {
        const textProperties = document.getElementById('text-properties');
        
        if (element.type === 'text') {
            // نمایش تنظیمات متن
            if (textProperties) textProperties.style.display = 'block';
            
            // پر کردن فیلدهای متن
            const fontSizeInput = document.getElementById('element-font-size');
            const textColorInput = document.getElementById('element-text-color');
            const textColorTextInput = document.getElementById('element-text-color-text');
            
            if (fontSizeInput) fontSizeInput.value = element.config.fontSize || 16;
            
            const textColor = element.config.color || '#000000';
            if (textColorInput) textColorInput.value = textColor;
            if (textColorTextInput) textColorTextInput.value = textColor;
            
        } else {
            // مخفی کردن تنظیمات متن برای سایر انواع
            if (textProperties) textProperties.style.display = 'none';
        }
    }

    /**
     * اضافه کردن event listeners برای تغییر خواص
     */
    addPropertyEventListeners(element) {
        // موقعیت و اندازه
        const xInput = document.getElementById('element-x');
        const yInput = document.getElementById('element-y');
        const widthInput = document.getElementById('element-width');
        const heightInput = document.getElementById('element-height');

        if (xInput) {
            xInput.removeEventListener('change', this._xChangeHandler);
            this._xChangeHandler = (e) => this.updateElementProperty(element.id, 'x', parseInt(e.target.value));
            xInput.addEventListener('change', this._xChangeHandler);
        }

        if (yInput) {
            yInput.removeEventListener('change', this._yChangeHandler);
            this._yChangeHandler = (e) => this.updateElementProperty(element.id, 'y', parseInt(e.target.value));
            yInput.addEventListener('change', this._yChangeHandler);
        }

        if (widthInput) {
            widthInput.removeEventListener('change', this._widthChangeHandler);
            this._widthChangeHandler = (e) => this.updateElementProperty(element.id, 'width', parseInt(e.target.value));
            widthInput.addEventListener('change', this._widthChangeHandler);
        }

        if (heightInput) {
            heightInput.removeEventListener('change', this._heightChangeHandler);
            this._heightChangeHandler = (e) => this.updateElementProperty(element.id, 'height', parseInt(e.target.value));
            heightInput.addEventListener('change', this._heightChangeHandler);
        }

        // رنگ پس‌زمینه
        const bgColorInput = document.getElementById('element-bg-color');
        if (bgColorInput) {
            bgColorInput.removeEventListener('change', this._bgColorChangeHandler);
            this._bgColorChangeHandler = (e) => this.updateElementProperty(element.id, 'backgroundColor', e.target.value);
            bgColorInput.addEventListener('change', this._bgColorChangeHandler);
        }

        // دکمه حذف
        const deleteBtn = document.getElementById('delete-element');
        if (deleteBtn) {
            deleteBtn.removeEventListener('click', this._deleteHandler);
            this._deleteHandler = () => this.deleteElement(element.id);
            deleteBtn.addEventListener('click', this._deleteHandler);
        }
    }

    /**
     * آپدیت خاصیت المنت
     */
    updateElementProperty(elementId, key, value) {
        const element = this.editor.elements.get(elementId);
        if (!element) return;

        const oldValue = element.config[key];
        element.config[key] = value;

        // آپدیت نمایش المنت
        const elementDiv = document.getElementById(elementId);
        if (elementDiv) {
            this.applyElementStyles(elementDiv, element);
            this.setElementContent(elementDiv, element);
        }

        // ثبت تغییر در cache
        this.editor.cacheManager.addChange('update-property', {
            id: elementId,
            key,
            oldValue,
            newValue: value
        });

        console.log(`Updated ${key} for element ${elementId}:`, oldValue, '->', value);
    }

    /**
     * مخفی کردن تنظیمات المنت
     */
    hideElementProperties() {
        const noSelection = document.getElementById('no-selection');
        const propertiesContent = document.getElementById('properties-content');
        
        if (noSelection) noSelection.style.display = 'block';
        if (propertiesContent) propertiesContent.style.display = 'none';

        console.log('Properties hidden');
    }

    /**
     * دریافت عنوان نوع المنت
     */
    getElementTypeTitle(type) {
        const titles = {
            'text': 'متن',
            'image': 'تصویر',
            'video': 'ویدیو',
            'camera': 'دوربین',
            'clock': 'ساعت',
            'weather': 'آب و هوا'
        };
        return titles[type] || type;
    }
}

/**
 * Page Element Class
 */
class PageElement {
    constructor(id, type, config) {
        this.id = id;
        this.type = type;
        this.config = {
            x: 0,
            y: 0,
            width: 100,
            height: 100,
            ...config
        };
        this.zIndex = 1;
    }

    toJSON() {
        return {
            id: this.id,
            type: this.type,
            config: this.config,
            zIndex: this.zIndex
        };
    }

    static fromJSON(data) {
        const element = new PageElement(data.id, data.type, data.config);
        element.zIndex = data.zIndex || 1;
        return element;
    }
}
