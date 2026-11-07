import {Injectable} from "@angular/core";
import {BaseHttpService} from "../../../shared/services/base-http.service";
import {Observable} from "rxjs";

@Injectable({
    providedIn: 'root',
})
export class UserApi extends BaseHttpService {
    public signIn(email: string, password: string): Observable<any> {
        return this.post<{ email: string, password: string }, any>(
            `user/signin`,
            {email, password})
    }

    public getUserData(): Observable<any> {
        return this.get(`user/me`)
    }
}