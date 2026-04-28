import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
    TeamBuilderConstructor,
    TeamBuilderDriver
} from '../models/team-builder.models';

@Injectable({
    providedIn: 'root'
})
export class TeamBuilderApiService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiBaseUrl}/teambuilder`;

    getDrivers(): Observable<TeamBuilderDriver[]> {
        return this.http.get<TeamBuilderDriver[]>(`${this.baseUrl}/drivers`);
    }

    getConstructors(): Observable<TeamBuilderConstructor[]> {
        return this.http.get<TeamBuilderConstructor[]>(`${this.baseUrl}/constructors`);
    }
}