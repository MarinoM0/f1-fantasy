import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamBuilderPageComponent } from './team-builder-page.component';

describe('TeamBuilderPageComponent', () => {
  let component: TeamBuilderPageComponent;
  let fixture: ComponentFixture<TeamBuilderPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamBuilderPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamBuilderPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
