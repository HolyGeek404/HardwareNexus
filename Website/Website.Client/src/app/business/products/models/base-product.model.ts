import {ProductCategory} from '../../../shared/models/enums';

export interface BaseProductModel {
  name: string;
  team: string;
  price: string;
  productImg: string;
  category: ProductCategory;
  id: string;
  productId: string;
  warranty: string;
  producerCode: string;
}
