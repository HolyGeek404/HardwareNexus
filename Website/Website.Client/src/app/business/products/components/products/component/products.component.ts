import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {BaseProductModel} from '../../../models/base-product.model';
import {CpuDetailsComponent} from '../../cpu/component/cpu-details/cpu-details.component';
import {GpuDetailsComponent} from '../../gpu/component/gpu-details/gpu-details.component';
import {CoolerDetailsComponent} from '../../cooler/component/cooler-details/cooler-details.component';
import {ProductTypes} from '../../../../../shared/models/enums';
import {toObservable, toSignal} from "@angular/core/rxjs-interop";
import {filter, map, switchMap} from "rxjs";
import {ProductApi} from "../../../api/product.api";

@Component({
    selector: 'app-products',
    standalone: true,
    imports: [NgOptimizedImage, RouterLink, CpuDetailsComponent, GpuDetailsComponent, CoolerDetailsComponent],
    templateUrl: './products.component.html',
    styleUrls: ['./products.component.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsComponent {
    route = inject(ActivatedRoute);
    api = inject(ProductApi);
    category = toSignal(this.route.paramMap.pipe(map(params => params.get('category'))), {initialValue: null});

    products = toSignal(toObservable(this.category).pipe(
            filter((category): category is string => category !== null),
            switchMap(category => this.api.getProducts(category))
        ),
        {initialValue: [] as BaseProductModel[]}
    );
    protected readonly ProductTypes = ProductTypes;
}
