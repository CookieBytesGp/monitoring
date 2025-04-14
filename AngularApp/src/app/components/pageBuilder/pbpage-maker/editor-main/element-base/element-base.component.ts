import { Component, EventEmitter, Input, Output, ElementRef, Renderer2, ViewEncapsulation } from '@angular/core';
import { BaseElemntModel } from '../../../../../models/PageBuilder/BaseElementModel';

@Component({
  selector: 'element-base',
  standalone: true,
  imports: [],
  templateUrl: './element-base.component.html',
  styleUrl: './element-base.component.css',
  encapsulation: ViewEncapsulation.None
})
export class ElementBaseComponent {
  @Input() innerElement: BaseElemntModel | undefined; // Input to receive the element data
  @Output() elementChange = new EventEmitter<BaseElemntModel>(); // Output to emit the updated element to the parent
  
  
  emitElementChange(): void {
    if (this.innerElement) {
      this.elementChange.emit(this.innerElement);
    }
  }
 
  resizing: boolean = false;
  resizeDirection: string = '';
  startX: number = 0;
  startY: number = 0;
  startWidth: number = 0;
  startHeight: number = 0;
  startLeft: number = 0;
  startTop: number = 0;

  constructor(private renderer: Renderer2, private el: ElementRef) {}

  ngOnChanges() {
    if (this.innerElement) {
      this.injectCustomCss(this.innerElement.templateBody.customCss);
    }
  }

  // Start resizing
  startResize(event: MouseEvent, direction: string): void {
    event.preventDefault();
    this.resizing = true;
    this.resizeDirection = direction;

    const elemntBox = this.el.nativeElement.closest('.ElemntBox');
    if (!elemntBox) return;

    this.startX = event.clientX;
    this.startY = event.clientY;
    this.startWidth = elemntBox.offsetWidth;
    this.startHeight = elemntBox.offsetHeight;
    this.startLeft = elemntBox.offsetLeft;
    this.startTop = elemntBox.offsetTop;

    document.addEventListener('mousemove', this.resize.bind(this));
    document.addEventListener('mouseup', this.stopResize.bind(this));
  }

  // Perform resizing
  resize(event: MouseEvent): void {
    if (!this.resizing || !this.innerElement) return;

    const elemntBox = this.el.nativeElement.closest('.ElemntBox');
    if (!elemntBox) return;

    const deltaX = event.clientX - this.startX;
    const deltaY = event.clientY - this.startY;

    let newWidth = this.startWidth;
    let newHeight = this.startHeight;
    let newLeft = this.startLeft;
    let newTop = this.startTop;

    switch (this.resizeDirection) {
      case 'right':
        newWidth = Math.max(this.startWidth + deltaX, 100); // Minimum width
        break;
      case 'left':
        if (this.startLeft + deltaX >= 0) {
          newWidth = Math.max(this.startWidth - deltaX, 100); // Minimum width
          newLeft = this.startLeft + deltaX;
        }
        break;
      case 'bottom':
        newHeight = Math.max(this.startHeight + deltaY, 50); // Minimum height
        break;
      case 'top':
        if (this.startTop + deltaY >= 0) {
          newHeight = Math.max(this.startHeight - deltaY, 50); // Minimum height
          newTop = this.startTop + deltaY;
        }
        break;
    }

    // Apply styles dynamically
    this.renderer.setStyle(elemntBox, 'width', `${newWidth}px`);
    this.renderer.setStyle(elemntBox, 'height', `${newHeight}px`);
    this.renderer.setStyle(elemntBox, 'left', `${newLeft}px`);
    this.renderer.setStyle(elemntBox, 'top', `${newTop}px`);

    // Update the CustomCss string
    this.updateCustomCss(newWidth, newHeight, newTop, newLeft);
  }

  // Stop resizing
  stopResize(): void {
    this.resizing = false;
    this.resizeDirection = '';
    document.removeEventListener('mousemove', this.resize.bind(this));
    document.removeEventListener('mouseup', this.stopResize.bind(this));
  }

  // Update the CustomCss string for the element and inject it into a <style> tag
  private updateCustomCss(width: number, height: number, top: number, left: number): void {
    if (!this.innerElement) return;

    const boxSelector = `.ElemntBox[data-id="${this.innerElement.id}"]`;
    const newBoxStyle = `width: ${width}px; height: ${height}px; left: ${left}px; top: ${top}px;`;

    let css = this.innerElement.templateBody.customCss || '';
    css = this.updateCSSRule(css, boxSelector, newBoxStyle);

    this.innerElement.templateBody.customCss = css;
    this.injectCustomCss(css);
    this.elementChange.emit(this.innerElement);
  }

  // Helper to update or add a CSS rule
  private updateCSSRule(css: string, selector: string, newStyle: string): string {
    // Escape special characters in the selector for use in the regular expression
    const escapedSelector = selector.replace(/[\[\]]/g, '\\$&');
    const regex = new RegExp(`${escapedSelector}\\s*\\{[^}]*\\}`, 'g');
    const rule = `${selector} { ${newStyle} }`;

    if (regex.test(css)) {
      return css.replace(regex, rule);
    } else {
      return css + '\n' + rule;
    }
  }

  // Inject the updated CustomCss into a <style> tag
  private injectCustomCss(css: string): void {


  if (this.innerElement?.templateBody.customCss && css) {
    // Remove any previously added <style> tags
    const existingStyle = this.el.nativeElement.querySelector('style.custom-style');
    if (existingStyle) {
      this.renderer.removeChild(this.el.nativeElement, existingStyle);
    }

    // Create a new <style> tag
    const styleElement = this.renderer.createElement('style');
    this.renderer.addClass(styleElement, 'custom-style');
    this.renderer.setProperty(styleElement, 'textContent', css);

    // Append the <style> tag to the component's host element
    this.renderer.appendChild(this.el.nativeElement, styleElement);
  }
}
}