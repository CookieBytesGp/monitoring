// Page Edit JavaScript Functionality
class PageEditManager {
    constructor() {
        this.isPreviewUpdateInProgress = false;
        this.debounceTimer = null;
        this.debounceDelay = 300;
        
        this.initializeEventListeners();
        this.updatePreview();
    }

    initializeEventListeners() {
        // Size change handlers
        const widthInput = document.getElementById('DisplayWidth');
        const heightInput = document.getElementById('DisplayHeight');
        
        if (widthInput && heightInput) {
            widthInput.addEventListener('input', () => this.debouncedUpdatePreview());
            heightInput.addEventListener('input', () => this.debouncedUpdatePreview());
        }
        
        // Delete confirmation modal
        this.initializeDeleteModal();
        
        // Form validation
        this.initializeFormValidation();
        
        // Auto-save functionality
        this.initializeAutoSave();
    }

    debouncedUpdatePreview() {
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }
        
        this.debounceTimer = setTimeout(() => {
            this.updatePreview();
        }, this.debounceDelay);
    }

    updatePreview() {
        if (this.isPreviewUpdateInProgress) return;
        
        this.isPreviewUpdateInProgress = true;
        
        try {
            const preview = document.querySelector('.preview-display');
            if (!preview) return;

            const widthInput = document.getElementById('DisplayWidth');
            const heightInput = document.getElementById('DisplayHeight');
            
            if (!widthInput || !heightInput) return;

            const width = parseInt(widthInput.value) || 800;
            const height = parseInt(heightInput.value) || 600;
            
            // Calculate container dimensions (max 400px width to fit in UI)
            const maxWidth = 400;
            const aspectRatio = width / height;
            
            let displayWidth, displayHeight;
            
            if (width > height) {
                displayWidth = Math.min(width, maxWidth);
                displayHeight = displayWidth / aspectRatio;
            } else {
                displayHeight = Math.min(height, maxWidth / aspectRatio);
                displayWidth = displayHeight * aspectRatio;
            }
            
            // Apply styles to preview
            preview.style.width = `${displayWidth}px`;
            preview.style.height = `${displayHeight}px`;
            
            // Update placeholder text
            const placeholder = preview.querySelector('.preview-placeholder');
            if (placeholder) {
                placeholder.textContent = `${width} × ${height} پیکسل`;
            }
            
            // Add smooth transition
            preview.style.transition = 'all 0.3s ease';
            
            console.log(`Preview updated: ${width}×${height} -> ${displayWidth}×${displayHeight}`);
            
        } catch (error) {
            console.error('Error updating preview:', error);
        } finally {
            this.isPreviewUpdateInProgress = false;
        }
    }

    initializeDeleteModal() {
        const deleteModal = document.getElementById('deleteModal');
        if (!deleteModal) return;

        deleteModal.addEventListener('show.bs.modal', (event) => {
            const button = event.relatedTarget;
            const pageTitle = button?.getAttribute('data-page-title') || 'این صفحه';
            
            const modalBody = deleteModal.querySelector('.modal-body');
            if (modalBody) {
                modalBody.innerHTML = `
                    <div class="text-center">
                        <i class="fas fa-exclamation-triangle text-warning mb-3" style="font-size: 3rem;"></i>
                        <h5>حذف صفحه</h5>
                        <p>آیا از حذف "${pageTitle}" اطمینان دارید؟</p>
                        <p class="text-danger small">این عمل قابل بازگشت نیست و تمام اطلاعات صفحه حذف خواهد شد.</p>
                    </div>
                `;
            }
        });
    }

    initializeFormValidation() {
        const form = document.querySelector('form');
        if (!form) return;

        form.addEventListener('submit', (event) => {
            if (!this.validateForm()) {
                event.preventDefault();
                event.stopPropagation();
            }
            
            form.classList.add('was-validated');
        });

        // Real-time validation
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });
    }

    validateForm() {
        const form = document.querySelector('form');
        if (!form) return false;

        let isValid = true;
        const requiredFields = form.querySelectorAll('input[required], select[required], textarea[required]');
        
        requiredFields.forEach(field => {
            if (!this.validateField(field)) {
                isValid = false;
            }
        });

        // Validate display dimensions
        const width = parseInt(document.getElementById('DisplayWidth')?.value);
        const height = parseInt(document.getElementById('DisplayHeight')?.value);
        
        if (width < 100 || width > 4000) {
            this.showFieldError(document.getElementById('DisplayWidth'), 'عرض باید بین 100 تا 4000 پیکسل باشد');
            isValid = false;
        }
        
        if (height < 100 || height > 4000) {
            this.showFieldError(document.getElementById('DisplayHeight'), 'ارتفاع باید بین 100 تا 4000 پیکسل باشد');
            isValid = false;
        }

        return isValid;
    }

    validateField(field) {
        if (!field) return true;

        const value = field.value.trim();
        
        if (field.hasAttribute('required') && !value) {
            this.showFieldError(field, 'این فیلد الزامی است');
            return false;
        }

        if (field.type === 'number') {
            const num = parseFloat(value);
            const min = parseFloat(field.getAttribute('min'));
            const max = parseFloat(field.getAttribute('max'));
            
            if (isNaN(num)) {
                this.showFieldError(field, 'لطفاً یک عدد معتبر وارد کنید');
                return false;
            }
            
            if (!isNaN(min) && num < min) {
                this.showFieldError(field, `مقدار نباید کمتر از ${min} باشد`);
                return false;
            }
            
            if (!isNaN(max) && num > max) {
                this.showFieldError(field, `مقدار نباید بیشتر از ${max} باشد`);
                return false;
            }
        }

        this.clearFieldError(field);
        return true;
    }

    showFieldError(field, message) {
        if (!field) return;

        field.classList.add('is-invalid');
        field.classList.remove('is-valid');

        let feedback = field.parentNode.querySelector('.invalid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            field.parentNode.appendChild(feedback);
        }
        feedback.textContent = message;
    }

    clearFieldError(field) {
        if (!field) return;

        field.classList.remove('is-invalid');
        field.classList.add('is-valid');

        const feedback = field.parentNode.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.remove();
        }
    }

    initializeAutoSave() {
        let autoSaveTimer;
        const autoSaveInterval = 30000; // 30 seconds

        const form = document.querySelector('form');
        if (!form) return;

        const startAutoSave = () => {
            autoSaveTimer = setInterval(() => {
                this.saveAsDraft();
            }, autoSaveInterval);
        };

        const stopAutoSave = () => {
            if (autoSaveTimer) {
                clearInterval(autoSaveTimer);
            }
        };

        // Start auto-save when form is interacted with
        form.addEventListener('input', startAutoSave, { once: true });
        
        // Stop auto-save when leaving the page
        window.addEventListener('beforeunload', stopAutoSave);
    }

    async saveAsDraft() {
        try {
            const form = document.querySelector('form');
            if (!form) return;

            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            
            // Save to localStorage as fallback
            localStorage.setItem('pageEditDraft', JSON.stringify({
                data: data,
                timestamp: new Date().toISOString()
            }));

            // Optionally show a subtle notification
            this.showAutoSaveNotification();
            
        } catch (error) {
            console.error('Auto-save failed:', error);
        }
    }

    showAutoSaveNotification() {
        const notification = document.createElement('div');
        notification.className = 'alert alert-info position-fixed';
        notification.style.cssText = `
            bottom: 20px;
            left: 20px;
            z-index: 9999;
            padding: 0.5rem 1rem;
            font-size: 0.875rem;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        notification.innerHTML = '<i class="fas fa-save me-2"></i>تغییرات خودکار ذخیره شد';
        
        document.body.appendChild(notification);
        
        // Fade in
        setTimeout(() => notification.style.opacity = '1', 100);
        
        // Fade out and remove
        setTimeout(() => {
            notification.style.opacity = '0';
            setTimeout(() => notification.remove(), 300);
        }, 2000);
    }

    // Utility methods
    showLoading(button) {
        if (!button) return;
        
        button.disabled = true;
        button.classList.add('loading');
        
        const originalText = button.textContent;
        button.setAttribute('data-original-text', originalText);
    }

    hideLoading(button) {
        if (!button) return;
        
        button.disabled = false;
        button.classList.remove('loading');
        
        const originalText = button.getAttribute('data-original-text');
        if (originalText) {
            button.textContent = originalText;
        }
    }

    // Navigation to editor
    openEditor() {
        const pageId = document.querySelector('input[name="Id"]')?.value;
        if (pageId) {
            window.location.href = `/Page/Editor/${pageId}`;
        }
    }

    // Handle quick actions
    handleQuickAction(action, pageId) {
        switch (action) {
            case 'editor':
                this.openEditor();
                break;
            case 'preview':
                this.openPreview(pageId);
                break;
            case 'duplicate':
                this.duplicatePage(pageId);
                break;
            case 'export':
                this.exportPage(pageId);
                break;
            default:
                console.warn('Unknown action:', action);
        }
    }

    async openPreview(pageId) {
        if (!pageId) return;
        
        try {
            const response = await fetch(`/Page/Preview/${pageId}`);
            if (response.ok) {
                const previewUrl = response.url;
                window.open(previewUrl, '_blank', 'width=800,height=600,scrollbars=yes');
            } else {
                throw new Error('Preview not available');
            }
        } catch (error) {
            console.error('Failed to open preview:', error);
            alert('خطا در بارگذاری پیش‌نمایش');
        }
    }

    async duplicatePage(pageId) {
        if (!pageId) return;
        
        try {
            const response = await fetch(`/Page/Duplicate/${pageId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const result = await response.json();
                if (result.success && result.data?.id) {
                    window.location.href = `/Page/Edit/${result.data.id}`;
                } else {
                    throw new Error('Duplication failed');
                }
            } else {
                throw new Error('Network error');
            }
        } catch (error) {
            console.error('Failed to duplicate page:', error);
            alert('خطا در کپی کردن صفحه');
        }
    }

    async exportPage(pageId) {
        if (!pageId) return;
        
        try {
            const response = await fetch(`/Page/Export/${pageId}`);
            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `page-${pageId}.json`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            } else {
                throw new Error('Export failed');
            }
        } catch (error) {
            console.error('Failed to export page:', error);
            alert('خطا در دانلود فایل');
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new PageEditManager();
});

    // Global function for backward compatibility
    function updatePreview() {
        const manager = window.pageEditManager;
        if (manager) {
            manager.updatePreview();
        }
    }

    // Status change functionality
    async function changeStatus(pageId, newStatus) {
        if (!pageId || !newStatus) return;
        
        try {
            const response = await fetch(`/Page/ChangeStatus/${pageId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ status: newStatus })
            });
            
            if (response.ok) {
                location.reload();
            } else {
                throw new Error('Status change failed');
            }
        } catch (error) {
            console.error('Failed to change status:', error);
            alert('خطا در تغییر وضعیت صفحه');
        }
    }

    // Delete confirmation functionality
    function confirmDelete(pageId, pageTitle) {
        if (!pageId) return;
        
        const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        document.getElementById('deletePageTitle').textContent = pageTitle || 'این صفحه';
        document.getElementById('deleteForm').action = `/Page/Delete/${pageId}`;
        modal.show();
    }

    // Make functions global for onclick handlers
    window.updatePreview = updatePreview;
    window.changeStatus = changeStatus;
    window.confirmDelete = confirmDelete;
