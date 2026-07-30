import {ChangeDetectionStrategy, Component, DestroyRef, OnInit, signal} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {ProductService} from '../../../services/product.service';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {ProductTypesEnum} from '../../../enums/product-types.enum';
import {NgOptimizedImage} from '@angular/common';
import {GpuModel} from '../models/gpu.model';
import {loadProduct} from '../../../functions/product.functions';

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
  protected gpuProduct = signal<GpuModel | undefined>(undefined)

  constructor(
    private router: ActivatedRoute,
    private productService: ProductService,
    private destroyRef: DestroyRef
  ) {}

  ngOnInit() {
    loadProduct<GpuModel>(ProductTypesEnum.GPU, this.productService, this.router)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(product => this.gpuProduct.set(product));
  }
}
