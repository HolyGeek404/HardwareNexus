import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {ProductTypes} from '../models/enums';
import {RequestsModel} from '../../business/user/components/sign-up/models/requests.model';
import {AccountVerificationRequest} from '../../business/user/models/account-verification.model';
import {FilterModel} from '../../business/products/components/filter/models/filter.model';
import {CartItemResponse} from "../../business/cart/models/CartItemResponse";
import {ApiGetArs, BaseGetRequest} from "../models/models";
import {toSignal} from "@angular/core/rxjs-interop";

@Injectable({providedIn: 'root'})
export class BaseHttpService {
    private baseUrl = '/api/gateway';
    private authOptions = {withCredentials: true, responseType: 'text' as const};

    constructor(private http: HttpClient) {
    }

    getProductFilters(types: ProductTypes): Observable<FilterModel> {
        return this.http.get<FilterModel>(`${this.baseUrl}product/${types}/filters`);
    }

    signUp(signUp: RequestsModel): Observable<boolean> {
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

    protected get<TResponse>(domain: string, args: ApiGetArs[]): TResponse | undefined {
        const request = this.buildGetRequest(domain, args);
        const response = toSignal<TResponse>(this.http.get<TResponse>(request.domain, {params: request.params}))
        return response();
    }

    private buildGetRequest(domain: string, args: ApiGetArs[]): BaseGetRequest {
        let params = new HttpParams();
        for (let arg of args) {
            params.set(arg.property, arg.value);
        }
        return {
            domain: `${this.baseUrl}/${domain}`,
            params: params,
        }
    }
}
