import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateFantasyTeamRequest,
  FantasyTeam
} from '../models/fantasy-team.models';

@Injectable({
  providedIn: 'root'
})
export class FantasyTeamApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/fantasyteam`;

  createTeam(request: CreateFantasyTeamRequest): Observable<FantasyTeam> {
    return this.http.post<FantasyTeam>(this.baseUrl, request);
  }

  updateTeam(request: CreateFantasyTeamRequest): Observable<FantasyTeam> {
    return this.http.put<FantasyTeam>(this.baseUrl, request);
  }

  getMyTeam(): Observable<FantasyTeam> {
    return this.http.get<FantasyTeam>(`${this.baseUrl}/me`);
  }
}
