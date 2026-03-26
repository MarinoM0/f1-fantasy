import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Router } from '@angular/router';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppShellComponent {
  private readonly router = inject(Router);
  readonly isNavOpen = signal(false);

  isAuthRoute(): boolean {
    return this.router.url === '/login' || this.router.url === '/register';
  }

  toggleNav(): void {
    this.isNavOpen.update((value) => !value);
  }

  closeNav(): void {
    this.isNavOpen.set(false);
  }
}
