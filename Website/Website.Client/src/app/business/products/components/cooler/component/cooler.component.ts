import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductApi} from '../../../api/product.api';
import {toSignal} from '@angular/core/rxjs-interop';
import {ProductTypes} from '../../../../../shared/models/enums';
import {NgOptimizedImage} from '@angular/common';
import {map} from "rxjs";
import {CoolerModel} from "../models/cooler.model";

@Component({
    selector: 'app-cooler',
    imports: [
        NgOptimizedImage
    ],
    templateUrl: './cooler.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './cooler.component.css',
})
export class CoolerComponent implements OnInit {
    readonly category = ProductTypes.COOLER;
    protected product = signal<CoolerModel | undefined>(undefined)
    private api = inject(ProductApi);
    private route = inject(ActivatedRoute);

    ngOnInit() {
        const id = toSignal(this.route.paramMap.pipe(map(params => params.get('id'))));
        if (id()) {
            const product = this.api.getProduct<CoolerModel>(this.category, id()!);
            this.product.set(product);
        }
    }
}
