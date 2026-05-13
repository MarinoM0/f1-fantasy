import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { CreateLeagueRequest, JoinLeagueRequest, League, LeagueLeaderboardEntry, LeagueSummary } from '../models/league.models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class LeaguesApiService {

  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/leagues`;

  createLeague(request: CreateLeagueRequest): Observable<League> {
    return this.http.post<League>(this.baseUrl, request);
  }

  joinLeague(request: JoinLeagueRequest): Observable<League> {
    return this.http.post<League>(`${this.baseUrl}/join`, request);
  }

  leaveLeague(leagueId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${leagueId}/leave`, {});
  }

  deleteLeague(leagueId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${leagueId}`);
  }

  getMyLeagues(): Observable<LeagueSummary[]> {
    return this.http.get<LeagueSummary[]>(`${this.baseUrl}/me`);
  }

  getLeague(leagueId: number): Observable<League> {
    return this.http.get<League>(`${this.baseUrl}/${leagueId}`);
  }

  getLeaderboard(leagueId: number): Observable<LeagueLeaderboardEntry[]> {
    return this.http.get<LeagueLeaderboardEntry[]>(`${this.baseUrl}/${leagueId}/leaderboard`);
  }
}
