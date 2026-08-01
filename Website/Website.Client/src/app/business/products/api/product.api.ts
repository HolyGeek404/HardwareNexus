import {Injectable} from '@angular/core';
import {BaseProductModel} from '../models/base-product.model';
import {BaseHttpService} from "../../../shared/services/base-http.service";
import {Observable} from "rxjs";

@Injectable({
    providedIn: 'root',
})
export class ProductApi extends BaseHttpService {
    private readonly domain = "product";

    public getProducts(category: string): Observable<BaseProductModel[]> {
        return this.get<BaseProductModel[]>(`${this.domain}/${category}`);
    }

    public getProduct<TProduct>(category: string, id: string): Observable<TProduct> {
        return this.get<TProduct>(`${this.domain}/${category}/${id}`);
    }
}
