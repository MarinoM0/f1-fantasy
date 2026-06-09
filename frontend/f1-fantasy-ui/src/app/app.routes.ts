import { Routes } from '@angular/router';
import { AppShellComponent } from './core/layout/app-shell.component';
import { LoginPageComponent } from './features/auth/pages/login-page.component';
import { RegisterPageComponent } from './features/auth/pages/register-page.component';
import { DashboardPageComponent } from './features/dashboard/pages/dashboard-page.component';
import { TeamBuilderPageComponent } from './features/fantasy-team/pages/team-builder-page.component';
import { MyTeamPageComponent } from './features/fantasy-team/pages/my-team-page.component';
import { LeagueListPage } from './features/leagues/pages/league-list-page';
import { LeagueDetailPageComponent } from './features/leagues/pages/league-detail-page';
import { authGuard } from './core/guards/auth.guard';
import { PredictionsPage } from './features/predictions/pages/predictions-page';

export const routes: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardPageComponent },
      { path: 'team-builder', component: TeamBuilderPageComponent },
      { path: 'my-team', component: MyTeamPageComponent, canActivate: [authGuard] },
      { path: 'leagues', component: LeagueListPage, canActivate: [authGuard] },
      { path: 'leagues/:id', component: LeagueDetailPageComponent, canActivate: [authGuard] },
      { path: 'predictions', component: PredictionsPage, canActivate: [authGuard]},
      { path: 'login', component: LoginPageComponent },
      { path: 'register', component: RegisterPageComponent }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];