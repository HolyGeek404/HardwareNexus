import {map, Observable, switchMap} from 'rxjs';
import {ProductTypesEnum} from '../enums/product-types.enum';
import {ActivatedRoute} from '@angular/router';
import {ProductService} from '../services/product.service';


export function loadProduct<T>(
  type: ProductTypesEnum,
  productService: ProductService,
  route: ActivatedRoute
): Observable<T> {
  return route.paramMap.pipe(
    map(params => params.get('id')),
    switchMap(id => productService.getProduct<T>(type, id!))
  );
}
