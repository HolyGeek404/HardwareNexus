import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {ProductTypes} from '../models/enums';
import {SignUpRequest} from '../../business/user/components/sign-up/models/model';
import {AccountVerificationRequest} from '../../business/user/models/model';
import {Model} from '../../business/products/components/filter/models/model';
import {CartItemResponse} from "../../business/cart/models/CartItemResponse";
import {environment} from "../../../environments/environment";

@Injectable({providedIn: 'root'})
export class BaseHttpService {
    private baseUrl = environment.api_gateway_url;
    private authOptions = {withCredentials: true, responseType: 'text' as const};

    constructor(private http: HttpClient) {
    }

    getProductFilters(types: ProductTypes): Observable<Model> {
        return this.http.get<Model>(`${this.baseUrl}product/${types}/filters`);
    }

    signUp(signUp: SignUpRequest): Observable<boolean> {
        return this.http.post<boolean>(`${this.baseUrl}user/signup`, signUp);
    }

    accountVerification(accVerf: AccountVerificationRequest): Observable<boolean> {
        return this.http.post<boolean>(`${this.baseUrl}user/accountverification`, accVerf);

    }

    getCart(userId: string): Observable<CartItemResponse[]> {
        return this.http.get<CartItemResponse[]>(`${this.baseUrl}cart`, {
            params: {userId}
        });
    }

    get<TResponse>(url: string): Observable<TResponse> {
        return this.http.get<TResponse>(`${this.baseUrl}/${url}`, {withCredentials: true});
    }

    post<TRequest, TResponse>(url: string, body: TRequest): Observable<TResponse> {
        return this.http.post<TResponse>(`${this.baseUrl}/${url}`, body, {withCredentials: true});
    }
}
