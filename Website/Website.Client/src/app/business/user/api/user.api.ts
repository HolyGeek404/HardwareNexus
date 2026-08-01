import {Injectable} from "@angular/core";
import {BaseHttpService} from "../../../shared/services/base-http.service";
import {Observable} from "rxjs";

@Injectable({
    providedIn: 'root',
})
export class UserApi extends BaseHttpService{

    // signIn(email: string, password: string): Observable<string> {
    //     return this.http.
    // }
}