import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductApi} from '../../../api/product.api';
import {toSignal} from '@angular/core/rxjs-interop';
import {CpuModel} from '../models/cpu.model';
import {ProductTypes} from '../../../../../shared/models/enums';
import {NgOptimizedImage} from '@angular/common';
import {map} from "rxjs";

@Component({
    selector: 'app-cpu',
    imports: [
        NgOptimizedImage
    ],
    templateUrl: './cpu.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './cpu.component.css',
})
export class CpuComponent implements OnInit {
    readonly category = ProductTypes.CPU;
    protected product = signal<CpuModel | undefined>(undefined)
    private api = inject(ProductApi);
    private route = inject(ActivatedRoute);

    ngOnInit() {
        const id = toSignal(this.route.paramMap.pipe(map(params => params.get('id'))));
        if (id()) {
            const product = this.api.getProduct<CpuModel>(this.category, id()!);
            this.product.set(product);
        }
    }
}
