import { Component, ElementRef, EventEmitter, Input, Output, Renderer2 } from '@angular/core';
import { PageModel } from '../../../../models/PageBuilder/PageModel';
import { BaseElemntModel } from '../../../../models/PageBuilder/BaseElementModel';
import { ElementBaseComponent } from './element-base/element-base.component';
import { CommonModule } from '@angular/common';
import { StyleHelper } from '../../../../helpers/StyleHelper';
interface Position {
  left: number;
  top: number;
}

interface DragState {
  startPosition: Position;
  mouseStart: Position;
}
@Component({
  selector: 'editor-main',
  standalone: true,
  imports: [ElementBaseComponent, CommonModule],
  templateUrl: './editor-main.component.html',
  styleUrl: './editor-main.component.css'
})
export class EditorMainComponent {
  //#region Properties and Fields
  @Input() elements: BaseElemntModel[] | undefined = [];
  @Output() elementsChangeOuter = new EventEmitter<BaseElemntModel[]>();
  private mutationObserver!: MutationObserver;
  private styleHelper: StyleHelper;
  private dragState: Map<string, DragState> = new Map();
  private rafId: number | null = null;
  private updateQueue: Set<string> = new Set();
  //#endregion

  //#region Lifecycle Hooks
  constructor(private el: ElementRef, private renderer: Renderer2) {
    this.styleHelper = new StyleHelper();
  }

  ngAfterViewInit() {
    this.initializeMutationObserver();
    this.attachEventListenersToExistingElements();
  }

  ngOnDestroy() {
    if (this.mutationObserver) {
      this.mutationObserver.disconnect();
    }
  }
  //#endregion

  //#region Element Management
  onElementChange(updatedElement: BaseElemntModel): void {
    if (this.elements) {
      const index = this.elements.findIndex(el => el.id === updatedElement.id);
      if (index !== -1) {
        this.elements[index] = updatedElement;
        this.elementsChangeOuter.emit(this.elements);
        const styles = this.extractStylesFromCss(updatedElement.templateBody.customCss, updatedElement.id);
        this.applyStylesToElement(updatedElement.id, styles);
      }
    }
  }

  deleteElement(elementId: string): void {
    if (this.elements) {
      this.elements = this.elements.filter(element => element.id !== elementId);
      this.elementsChangeOuter.emit(this.elements);
      console.log(`Element with ID ${elementId} deleted.`);
    }
  }

  selectElement(element: HTMLElement) {
    console.log('Element selected:', element);
  }
  //#endregion



//#region Initialization
private initializeMutationObserver(): void {
  this.mutationObserver = new MutationObserver(() => {
    this.attachEventListenersToExistingElements();
  });

  this.mutationObserver.observe(this.el.nativeElement, {
    childList: true,
    subtree: true
  });
}

private attachEventListenersToExistingElements(): void {
  const elemBoxes = this.el.nativeElement.querySelectorAll('.ElemntBox');
  elemBoxes.forEach((box: HTMLElement) => {
    if (!box.hasAttribute('data-listener-attached')) {
      this.renderer.listen(box, 'mousedown', (event: MouseEvent) => this.onDragStart(event, box));
      box.setAttribute('data-listener-attached', 'true');
    }
  });
}
//#endregion

//#region Drag and Drop Functionality
private onDragStart(event: MouseEvent, box: HTMLElement): void {
  if (!box || event.button !== 0) return;
  
  const elementId = box.getAttribute('data-id');
  if (!elementId) return;

  event.preventDefault();
  event.stopPropagation();

  // Get container and element positions
  const containerRect = this.el.nativeElement.getBoundingClientRect();
  const elementRect = box.getBoundingClientRect();
  const computedStyle = window.getComputedStyle(box);

  // Calculate position relative to container
  const initialPosition = {
    left: elementRect.left - containerRect.left,
    top: elementRect.top - containerRect.top
  };

  // Account for margins
  const margins = {
    left: parseInt(computedStyle.marginLeft || '0', 10),
    top: parseInt(computedStyle.marginTop || '0', 10)
  };

  this.dragState.set(elementId, {
    startPosition: {
      left: initialPosition.left - margins.left,
      top: initialPosition.top - margins.top
    },
    mouseStart: {
      left: event.clientX,
      top: event.clientY
    }
  });

  // Set initial position if not already set
  this.renderer.setStyle(box, 'position', 'absolute');
  if (!box.style.left) {
    this.renderer.setStyle(box, 'left', `${initialPosition.left}px`);
  }
  if (!box.style.top) {
    this.renderer.setStyle(box, 'top', `${initialPosition.top}px`);
  }

  let lastX = event.clientX;
  let lastY = event.clientY;

  const onMouseMove = (e: MouseEvent) => {
    if (!this.dragState.has(elementId)) return;
    
    // Calculate delta from last position to reduce lag
    const deltaX = e.clientX - lastX;
    const deltaY = e.clientY - lastY;
    lastX = e.clientX;
    lastY = e.clientY;

    const currentLeft = parseInt(box.style.left || '0', 10);
    const currentTop = parseInt(box.style.top || '0', 10);

    const newPosition = {
      left: currentLeft + deltaX,
      top: currentTop + deltaY
    };

    // Direct style update for smooth movement
    this.renderer.setStyle(box, 'left', `${newPosition.left}px`);
    this.renderer.setStyle(box, 'top', `${newPosition.top}px`);
  };

  const onMouseUp = () => {
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
    this.dragState.delete(elementId);
    this.finalizeElementPosition(box);
  };

  // Ensure position is set before starting drag
  this.renderer.setStyle(box, 'position', 'absolute');
  if (!box.style.left) {
    this.renderer.setStyle(box, 'left', `${initialPosition.left}px`);
  }
  if (!box.style.top) {
    this.renderer.setStyle(box, 'top', `${initialPosition.top}px`);
  }

  document.addEventListener('mousemove', onMouseMove);
  document.addEventListener('mouseup', onMouseUp);
}

private finalizeElementPosition(element: HTMLElement): void {
  const elementId = element.getAttribute('data-id');
  if (!elementId || !this.elements) return;

  const elementIndex = this.elements.findIndex(el => el.id === elementId);
  if (elementIndex === -1) return;

  // Get final position directly from style
  const finalPosition = {
    left: parseInt(element.style.left || '0', 10),
    top: parseInt(element.style.top || '0', 10)
  };

  // Update the element's styles in the model
  const updatedElements = [...this.elements];
  const currentElement = { ...updatedElements[elementIndex] };
  
  currentElement.templateBody = {
    ...currentElement.templateBody,
    customCss: this.styleHelper.updateCSS(
      currentElement.templateBody.customCss,
      `.ElemntBox[data-id="${elementId}"]`,
      { 
        position: 'absolute',
        left: `${finalPosition.left}px`, 
        top: `${finalPosition.top}px`
      }
    )
  };

  updatedElements[elementIndex] = currentElement;
  this.elements = updatedElements;
  this.elementsChangeOuter.emit(this.elements);
}
//#endregion

  //#region CSS Management
  updateCSS(currentCSS: string, selector: string, properties: { [key: string]: string }): string {
    this.styleHelper.parseAndAddStyles(currentCSS);
    this.styleHelper.addStyle(selector, properties);
    return this.styleHelper.generateCSS();
  }

  private applyStylesToElement(elementId: string, styles: { [key: string]: string }): void {
    const element = document.querySelector(`.ElemntBox[data-id="${elementId}"]`);
    if (element) {
      Object.entries(styles).forEach(([key, value]) => {
        this.renderer.setStyle(element, key, value);
      });
    }
  }

  extractStylesFromCss(customCss: string, elementId: string): { [key: string]: string } {
    const selector = `.ElemntBox[data-id="${elementId}"]`;
    this.styleHelper.parseAndAddStyles(customCss);
    return this.styleHelper.getStyle(selector) || {};
  }

  synchronizeCSS(css: string, action: string, element: HTMLElement) {
    this.styleHelper.parseAndAddStyles(css);
    console.log('Synchronizing CSS:', css, 'Action:', action, 'Element:', element);
  }
  //#endregion
}