import {ChangeDetectionStrategy, Component} from '@angular/core';
import {
  CategoryCardComponent
} from "../../../../business/products/components/category-card/component/category-card.component";
import {ProductTypes} from "../../../models/enums";

@Component({
    selector: 'app-home',
    imports: [CategoryCardComponent],
    templateUrl: './home.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './home.component.css'
})
export class HomeComponent {
    categories = Object.values(ProductTypes);
}
