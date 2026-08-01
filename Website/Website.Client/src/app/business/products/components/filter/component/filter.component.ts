import {ChangeDetectionStrategy, Component, input, output} from '@angular/core';
import {ProductTypes} from '../../../../../shared/models/enums';

export interface ProductFilterSelection {
    team: string[];
    socket: string[];
    cores: string[];
    architecture: string[];
    priceMin: string;
    priceMax: string;
}

@Component({
    selector: 'app-product-filter',
    standalone: true,
    templateUrl: './filter.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './filter.component.css'
})
export class FilterComponent {
    productType = input.required<ProductTypes>();
    filtersApplied = output<ProductFilterSelection>();


}
