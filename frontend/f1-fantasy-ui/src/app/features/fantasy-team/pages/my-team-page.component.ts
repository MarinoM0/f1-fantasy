import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-my-team-page',
  templateUrl: './my-team-page.component.html',
  styleUrl: './my-team-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyTeamPageComponent {}