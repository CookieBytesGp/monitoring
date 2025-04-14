import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ElementBaseComponent } from './element-base.component';

describe('ElementBaseComponent', () => {
  let component: ElementBaseComponent;
  let fixture: ComponentFixture<ElementBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ElementBaseComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ElementBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
