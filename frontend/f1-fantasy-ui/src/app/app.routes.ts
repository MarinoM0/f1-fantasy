import { Routes } from '@angular/router';
import { AppShellComponent } from './core/layout/app-shell.component';
import { LoginPageComponent } from './features/auth/pages/login-page.component';
import { RegisterPageComponent } from './features/auth/pages/register-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { TeamBuilderPageComponent } from './features/fantasy-team/pages/team-builder-page.component';
import { MyTeamPageComponent } from './features/fantasy-team/pages/my-team-page.component';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardPageComponent },
      { path: 'team-builder', component: TeamBuilderPageComponent },
      { path: 'my-team', component: MyTeamPageComponent },
      { path: 'login', component: LoginPageComponent },
      { path: 'register', component: RegisterPageComponent }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];