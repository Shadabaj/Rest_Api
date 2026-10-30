import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Environment } from '../../../Environment/Enviroment';
import { API } from '../../Urls/API';

@Injectable({ providedIn: 'root' })
export class PortalApiService {
  private readonly base = Environment.apiUrl;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<any> {
    return this.http.get(this.base + API.DASHBOARD);
  }

  getHolidays(year?: number): Observable<any[]> {
    const q = year ? `?year=${year}` : '';
    return this.http.get<any[]>(this.base + API.HOLIDAYS + q);
  }

  createHoliday(body: any): Observable<any> {
    return this.http.post(this.base + API.HOLIDAYS, body);
  }

  deleteHoliday(id: number): Observable<any> {
    return this.http.delete(this.base + API.HOLIDAYS + '/' + id);
  }

  getLeaveTypes(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.LEAVE_TYPES);
  }

  getLeaveRequests(status?: string): Observable<any[]> {
    const q = status ? `?status=${status}` : '';
    return this.http.get<any[]>(this.base + API.LEAVE_REQUESTS + q);
  }

  getDepartments(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.DEPARTMENTS);
  }

  getDesignations(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.DESIGNATIONS);
  }

  getEmployees(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.EMPLOYEES);
  }

  getShifts(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.SHIFTS);
  }

  getShiftAssignments(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.SHIFT_ASSIGNMENTS);
  }

  getAttendanceDaily(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ATTENDANCE_DAILY);
  }

  getAttendancePunches(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ATTENDANCE_PUNCHES);
  }

  getAttendanceAdjustments(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ATTENDANCE_ADJUSTMENTS);
  }

  getCountries(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.COUNTRIES);
  }

  getStates(countryId?: number): Observable<any[]> {
    const q = countryId ? `?countryId=${countryId}` : '';
    return this.http.get<any[]>(this.base + API.STATES + q);
  }

  getCities(stateId?: number): Observable<any[]> {
    const q = stateId ? `?stateId=${stateId}` : '';
    return this.http.get<any[]>(this.base + API.CITIES + q);
  }

  getWorkLocations(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.WORK_LOCATIONS);
  }

  getRoles(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ROLES);
  }

  provisionEmployee(body: any): Observable<any> {
    return this.http.post(this.base + API.EMPLOYEES_PROVISION, body);
  }

  deleteEmployee(id: number): Observable<any> {
    return this.http.delete(this.base + API.EMPLOYEES + '/' + id);
  }

  submitLeave(body: any): Observable<any> {
    return this.http.post(this.base + API.ME_LEAVES_SUBMIT, body);
  }

  reviewLeave(id: number, body: any): Observable<any> {
    return this.http.put(this.base + API.LEAVE_REQUESTS + '/' + id + '/review', body);
  }

  getMyAttendanceMonth(year?: number, month?: number): Observable<any> {
    const params: string[] = [];
    if (year) params.push(`year=${year}`);
    if (month) params.push(`month=${month}`);
    const q = params.length ? '?' + params.join('&') : '';
    return this.http.get(this.base + API.ME_ATTENDANCE_MONTH + q);
  }

  seedIndiaHolidays(): Observable<any> {
    return this.http.post(this.base + API.HOLIDAYS_SEED_INDIA, {});
  }

  getTrips(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.TRIPS);
  }

  createTrip(body: any): Observable<any> {
    return this.http.post(this.base + API.TRIPS, body);
  }

  payTrip(id: number, body: any): Observable<any> {
    return this.http.post(this.base + API.TRIPS + '/' + id + '/pay', body);
  }

  getMyProfile(): Observable<any> {
    return this.http.get(this.base + API.ME_PROFILE);
  }

  getMyAttendance(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ME_ATTENDANCE);
  }

  getMyPunches(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ME_PUNCHES);
  }

  getMyLeaves(): Observable<any[]> {
    return this.http.get<any[]>(this.base + API.ME_LEAVES);
  }

  getClockStatus(): Observable<any> {
    return this.http.get(this.base + API.ME_CLOCK_STATUS);
  }

  clockIn(): Observable<any> {
    return this.http.post(this.base + API.ME_CLOCK_IN, {});
  }

  clockOut(): Observable<any> {
    return this.http.post(this.base + API.ME_CLOCK_OUT, {});
  }
}
