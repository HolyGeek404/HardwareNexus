import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ProductCategory} from '../../../../../shared/models/enums';

@Component({
    selector: 'app-category-card',
    imports: [
        NgOptimizedImage,
        RouterLink
    ],
    templateUrl: './category-card.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './category-card.component.css'
})
export class CategoryCardComponent {
    category = input.required<ProductCategory>();
    categoryImgPath = computed(() => {
        return "categories/" + this.category().toLowerCase() + ".svg";
    });
}
