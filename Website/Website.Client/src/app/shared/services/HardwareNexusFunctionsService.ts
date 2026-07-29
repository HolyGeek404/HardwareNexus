import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {ProductTypesEnum} from '../../business/products/enums/product-types.enum';
import {RequestsModel} from '../../business/user/components/sign-up/models/requests.model';
import {AccountVerificationRequest} from '../../business/user/models/account-verification.model';
import {FilterModel} from '../../business/products/components/filter/models/filter.model';
import {CartItemResponse} from "../../business/cart/models/CartItemResponse";

@Injectable({ providedIn: 'root' })
export class HardwareNexusFunctionsService {
  private baseUrl = '/api/gateway/';
  private authOptions = { withCredentials: true, responseType: 'text' as const };

  constructor(private http: HttpClient) {}



  getProductFilters(types: ProductTypesEnum): Observable<FilterModel> {
    return this.http.get<FilterModel>(`${this.baseUrl}product/${types}/filters`);
  }

  signIn(email: string, password: string): Observable<string> {
    return this.http.post(`${this.baseUrl}user/signin`, {email, password}, this.authOptions);
  }
  signUp(signUp: RequestsModel): Observable<boolean> {
    return this.http.post<boolean>(`${this.baseUrl}user/signup`, signUp);
  }
  accountVerification(accVerf: AccountVerificationRequest): Observable<boolean>{
    return this.http.post<boolean>(`${this.baseUrl}user/accountverification`, accVerf);

  }

  getCart(userId: string): Observable<CartItemResponse[]> {
    return this.http.get<CartItemResponse[]>(`${this.baseUrl}cart`, {
      params: { userId }
    });
  }
}
