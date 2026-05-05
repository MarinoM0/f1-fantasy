import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Router } from '@angular/router';
import { AuthStateService } from '../../features/auth/services/auth-state.service';

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
  private readonly authState = inject(AuthStateService);

  readonly currentUser = this.authState.currentUser;
  readonly isAuthenticated = this.authState.isAuthenticated;

  logout(): void {
    this.authState.logout();
    this.router.navigate(['/dashboard'])
  }

  isAuthRoute(): boolean {
    return this.router.url === '/login' || this.router.url === '/register';
  }

  isFantasyRoute(): boolean {
    const fantasyUrls = ['/', '/dashboard', '/team-builder', '/my-team'];
    return fantasyUrls.includes(this.router.url);
  }

  toggleNav(): void {
    this.isNavOpen.update((value) => !value);
  }

  closeNav(): void {
    this.isNavOpen.set(false);
  }
}
