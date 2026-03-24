import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyTeamPageComponent } from './my-team-page.component';

describe('MyTeamPageComponent', () => {
  let component: MyTeamPageComponent;
  let fixture: ComponentFixture<MyTeamPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyTeamPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MyTeamPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
