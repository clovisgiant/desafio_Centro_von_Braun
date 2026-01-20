import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, BehaviorSubject } from "rxjs";
import { tap } from "rxjs/operators";
import { environment } from "../../environments/environment";
import { LoginRequest, LoginResponse } from "../models/device.model";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(this.getToken());
  public token$ = this.tokenSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<LoginResponse> {
    const url = `${environment.apiUrl}/auth/login`;
    const request: LoginRequest = { username, password };

    return this.http.post<LoginResponse>(url, request).pipe(
      tap((response) => {
        localStorage.setItem("token", response.accessToken);
        this.tokenSubject.next(response.accessToken);
      })
    );
  }

  logout(): void {
    localStorage.removeItem("token");
    this.tokenSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem("token");
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}