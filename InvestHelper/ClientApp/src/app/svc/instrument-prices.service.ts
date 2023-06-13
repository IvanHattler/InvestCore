import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';


export class TickerInfoBase {
  public tickerType: InstrumentType;
  public ticker: string;
  public classCode: string;

  constructor(tickerType: InstrumentType, ticker: string, classCode: string) {
    this.tickerType = tickerType;
    this.ticker = ticker;
    this.classCode = classCode;
  }
}

export class TickerPriceInfo extends TickerInfoBase {
  public count: number | undefined;
  public price: number | undefined;

  public get value(): number | undefined {
    if (this.count == null || this.price == null) {
      return undefined;
    }
    return this.count * this.price;
  }

  constructor(tickerType: InstrumentType, ticker: string, classCode: string, count: number) {
    super(tickerType, ticker, classCode);
    this.count = count;
  }
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

  public getAll(): Observable<TickerPriceInfo[]> {
    const API_URL = 'InstrumentPrices/GetAll';
    return this.http.get<TickerPriceInfo[]>(this.baseUrl + API_URL);
  }

  public getPrices(tickerInfos: TickerInfoBase[]): Observable<Map<string, number>> {
    const API_URL = 'InstrumentPrices/GetPrices';
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };

    return this.http.post<Map<string, number>>(this.baseUrl + API_URL,
      JSON.stringify(tickerInfos),
      httpOptions);
  }
}
