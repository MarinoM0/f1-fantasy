import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-team-builder-page',
  templateUrl: './team-builder-page.component.html',
  styleUrl: './team-builder-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeamBuilderPageComponent {}