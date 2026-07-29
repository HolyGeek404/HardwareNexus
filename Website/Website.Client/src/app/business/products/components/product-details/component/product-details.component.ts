import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {NgOptimizedImage} from '@angular/common';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ProductService} from '../../../../user/services/product.service';
import {BaseProductModel} from '../../../models/base-product.model';
import {CpuDetailsComponent} from '../../cpu/component/cpu-details/cpu-details.component';
import {GpuDetailsComponent} from '../../gpu/component/gpu-details/gpu-details.component';
import {CoolerDetailsComponent} from '../../cooler/component/cooler-details/cooler-details.component';
import {ProductTypesEnum} from '../../../enums/product-types.enum';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {map, switchMap} from 'rxjs';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [NgOptimizedImage, RouterLink, CpuDetailsComponent, GpuDetailsComponent, CoolerDetailsComponent],
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailsComponent implements OnInit{
  private destroyRef = inject(DestroyRef)
  category = signal<string>("")
  products = signal<BaseProductModel[]>([]);

    constructor(private router: ActivatedRoute, private productService: ProductService) {}

    ngOnInit() {
      this.router.paramMap.pipe(
        map(params => {this.category.set(params.get('category') as string)}),
        switchMap(() => {
          return this.productService.getProductsBaseInfo(this.category())
        }),
        takeUntilDestroyed(this.destroyRef)
      )
        .subscribe(result => {this.products.set(result);});
    }

  protected readonly ProductTypes = ProductTypesEnum;
}
