import {Injectable} from "@angular/core";
import {BaseHttpService} from "../../../shared/services/base-http.service";
import {Observable} from "rxjs";
import {UserModel} from "../../../shared/models/models";

@Injectable({
    providedIn: 'root',
})
export class UserApi extends BaseHttpService {
    public getUserData(): Observable<UserModel> {
        return this.get(`user/me`)
    }

    public signOut(): Observable<void> {
        return this.post<void, void>(`user/signout`, undefined);
    }
}
