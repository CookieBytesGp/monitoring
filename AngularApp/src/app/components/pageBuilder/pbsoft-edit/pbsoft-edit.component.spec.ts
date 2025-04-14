import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PBSoftEditComponent } from './pbsoft-edit.component';

describe('PBSoftEditComponent', () => {
  let component: PBSoftEditComponent;
  let fixture: ComponentFixture<PBSoftEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PBSoftEditComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PBSoftEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
