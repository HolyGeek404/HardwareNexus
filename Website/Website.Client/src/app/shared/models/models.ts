import {HttpParams} from "@angular/common/http";

export interface UserModel {
  name: string;
  surname: string;
  email: string;
}

export interface ApiGetArs{
  property: string;
  value: string;
}

export interface BaseRequest{
  domain: string;
}

export interface BaseGetRequest extends BaseRequest{
  params:HttpParams;
}