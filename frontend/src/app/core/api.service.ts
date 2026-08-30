import { HttpClient } from '@angular/common/http'; import { Injectable, inject } from '@angular/core'; import { Observable } from 'rxjs'; import { environment } from '../../environments/environment.generated'; import { AuthResponse, User } from './models';
@Injectable({ providedIn: 'root' })
export class ApiService { private readonly http = inject(HttpClient); private readonly base = `${environment.apiUrl}/api`;
  register(body: { name: string; email: string; password: string; confirmPassword: string }): Observable<AuthResponse> { return this.http.post<AuthResponse>(`${this.base}/auth/register`, body); }
  login(body: { email: string; password: string }): Observable<AuthResponse> { return this.http.post<AuthResponse>(`${this.base}/auth/login`, body); }
  profile(): Observable<User> { return this.http.get<User>(`${this.base}/profile`); }
  updateProfile(body: { name: string; email: string; password: string | null; confirmPassword: string | null }): Observable<User> { return this.http.put<User>(`${this.base}/profile`, body); }
}
