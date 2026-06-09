import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { CreatePredictionRequest, Prediction, PredictionLeaderboardEntry, UpcomingPrediction } from '../models/prediction.models';

@Injectable({
  providedIn: 'root',
})
export class PredictionsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/predictions`;

  getUpcoming(): Observable<UpcomingPrediction> {
    return this.http.get<UpcomingPrediction>(`${this.baseUrl}/upcoming`);
  }

  submit(request: CreatePredictionRequest): Observable<Prediction> {
    return this.http.post<Prediction>(this.baseUrl, request);
  }

  getMyPredictions(): Observable<Prediction[]> {
    return this.http.get<Prediction[]>(`${this.baseUrl}/me`);
  }

  getLeaderboard(): Observable<PredictionLeaderboardEntry[]> {
    return this.http.get<PredictionLeaderboardEntry[]>(`${this.baseUrl}/leaderboard`);
  }
}
