import {Injectable} from '@angular/core';
import {BaseProductModel} from '../models/base-product.model';
import {BaseHttpService} from "../../../shared/services/base-http.service";
import {ApiGetArs} from "../../../shared/models/models";

@Injectable({
    providedIn: 'root',
})
export class ProductApi extends BaseHttpService {
    private readonly domain = "product";

    public getProducts(category: string): BaseProductModel[] | undefined {
        const args: ApiGetArs[] = [{
            property: "type",
            value: category
        }];
        return this.get<BaseProductModel[]>(this.domain, args)
    }

    public getProduct<TProduct>(category: string, id: string): TProduct | undefined {
        const args: ApiGetArs[] = [{
            property: "type",
            value: category
        }, {property: "id", value: id}];

        return this.get<TProduct>(this.domain, args)

    }
}
