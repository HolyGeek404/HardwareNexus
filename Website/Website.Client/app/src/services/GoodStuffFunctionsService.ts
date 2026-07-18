import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {ProductTypes} from '../models/product/ProductTypes';
import {SignUpRequest} from '../models/user/SignUpRequest';
import {AccountVerificationRequest} from '../models/user/AccountVerification';
import {ProductFilters} from '../models/product/ProductFilters';
import {CartItemResponse} from '../models/cart/CartItemResponse';

@Injectable({providedIn: 'root'})
export class GoodStuffFunctionsService {
    private baseUrl = '/api/gateway/';
    private authOptions = {withCredentials: true, responseType: 'text' as const};

    constructor(private http: HttpClient) {
    }


    getProductFilters(types: ProductTypes): Observable<ProductFilters> {
        return this.http.get<ProductFilters>(`${this.baseUrl}product/${types}/filters`);
    }

    signIn(email: string, password: string): Observable<string> {
        return this.http.post(`${this.baseUrl}user/signin`, {email, password}, this.authOptions);
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
}
