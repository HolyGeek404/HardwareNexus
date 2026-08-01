import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {BaseProductModel} from '../../../models/base-product.model';
import {CpuDetailsComponent} from '../../cpu/component/cpu-details/cpu-details.component';
import {GpuDetailsComponent} from '../../gpu/component/gpu-details/gpu-details.component';
import {CoolerDetailsComponent} from '../../cooler/component/cooler-details/cooler-details.component';
import {ProductTypes} from '../../../../../shared/models/enums';
import {toSignal} from "@angular/core/rxjs-interop";
import {map} from "rxjs";
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
    category = toSignal(this.route.paramMap.pipe(map(params => params.get('category'))));
    products = signal<BaseProductModel[]>([]);
    protected readonly ProductTypes = ProductTypes;

    ngOnInit() {
        if (this.category()) {
            const products = this.api.getProducts(this.category()!);
            if (products && products.length > 0) {
                this.products.set(products);
            }
        }
    }
}
