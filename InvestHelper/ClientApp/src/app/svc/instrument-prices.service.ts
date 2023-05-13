import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'InstrumentPrices/GetPrices';

export interface ITickerInfoBase {
  TickerType: InstrumentType;
  Ticker: string;
  ClassCode: string;
}

export enum InstrumentType {
  Share,
  Bond,
  Etf,
}

@Injectable({
  providedIn: 'root'
})
export class InstrumentPricesService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  public getPrices(tickerInfos: ITickerInfoBase[]): Observable<Map<string, number>> {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };

    return this.http.post<Map<string, number>>(this.baseUrl + API_URL,
      JSON.stringify(tickerInfos),
      httpOptions);
  }
}
