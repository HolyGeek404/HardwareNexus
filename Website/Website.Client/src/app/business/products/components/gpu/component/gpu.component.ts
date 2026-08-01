import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {GpuModel} from '../models/gpu.model';
import {ProductApi} from "../../../api/product.api";
import {map} from "rxjs";
import {ActivatedRoute} from "@angular/router";
import {toSignal} from "@angular/core/rxjs-interop";
import {ProductTypes} from "../../../../../shared/models/enums";

@Component({
    selector: 'app-gpu',
    imports: [
        NgOptimizedImage
    ],
    templateUrl: './gpu.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './gpu.component.css',
})
export class GpuComponent implements OnInit {
    readonly category = ProductTypes.GPU;
    protected product = signal<GpuModel | undefined>(undefined)
    private api = inject(ProductApi);
    private route = inject(ActivatedRoute);

    ngOnInit() {
        const id = toSignal(this.route.paramMap.pipe(map(params => params.get('id'))));
        if (id()) {
            const product = toSignal(this.api.getProduct<GpuModel>(this.category, id()!));
            if (product()) {
                this.product.set(product()!);
            }
        }
    }
}
